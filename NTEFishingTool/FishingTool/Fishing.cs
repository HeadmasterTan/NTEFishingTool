using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using static NTEFishingTool.FishingTool.ImageHandler;

namespace NTEFishingTool.FishingTool
{
    enum EFishState
    {
        GameIdle, // 游戏界面待机中
        Idle, // 钓鱼界面待机中
        Fishing, // 钓鱼中
        BuyBait, // 购买鱼饵
        SaleFish, // 出售鱼获
        Back, // 回退到游戏待机界面，充当状态重置
        MoonCard, // 处理月卡逻辑
    }

    internal class Fishing
    {
        private const string GAME_PROCESS_NAME = "HTGame"; // 异环的程序名
        private const int FISHING_INTERVAL_MS = 1000 / 60; // 默认60帧率

        private Process _prcGame; // 游戏进程对象
        private IntPtr _intPtrGame; // 游戏窗口句柄
        private EFishState _curFishState = EFishState.Idle; // 当前的钓鱼状态

        private TaskController _tasks;
        private bool _isTaskRunning;
        private string _curRunningTaskName;

        private static Fishing _uniqueInstance;
        private static readonly object _lock = new object();

        public IntPtr IntPtrGame
        {
            get => _intPtrGame;
        }

        public EFishState CurFishState
        {
            get => _curFishState;
            set => _curFishState = value;
        }

        private Fishing() { }

        /// <summary>
        /// 懒汉式单例模式 - 线程安全（双重检查锁）
        /// </summary>
        /// <returns></returns>
        public static Fishing GetInstance()
        {
            // 一重检查：避免不必要的加锁开销
            if (_uniqueInstance == null)
            {
                lock (_lock)
                {
                    // 二重检查：确保多线程并发时只创建一次
                    if (_uniqueInstance == null)
                    {
                        _uniqueInstance = new Fishing();
                    }
                }
            }
            return _uniqueInstance;
        }

        public void Start()
        {
            try
            {
                if (_tasks == null)
                {
                    _prcGame = ProcessHandler.GetProcess(GAME_PROCESS_NAME);

                    if (_prcGame == null)
                    {
                        throw new Exception("游戏未运行");
                    }

                    // 初始化一些数据。
                    _intPtrGame = _prcGame.MainWindowHandle;
                    ProcessHandler.SetForegroundWindow(_intPtrGame);
                    Thread.Sleep(200);
                    _tasks = new TaskController();

                    TemplateController.InitializeImages(_intPtrGame);

                    GetPureClientRect(_intPtrGame, out RECT windowRect);
                    TemplateController.InitRatio(windowRect);

                    // 游戏进程和句柄均已记录，开始执行钓鱼任务
                    _isTaskRunning = true;
                    _curRunningTaskName = "FishingLoop";
                    _tasks.StartTask(_curRunningTaskName, FishingLoop);
                }
                else if (!_isTaskRunning)
                {
                    ProcessHandler.SetForegroundWindow(_intPtrGame);
                    Thread.Sleep(200);

                    _tasks.ResumeTasks(_curRunningTaskName);
                    _isTaskRunning = true;
                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                }
            }
            catch(Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        public void Pause()
        {
            if (_tasks != null && _isTaskRunning)
            {
                _tasks.PauseTask(_curRunningTaskName);
                _isTaskRunning = false;
            }
        }

        public void Stop()
        {
            _tasks?.StopAllTasks();
        }

        private void ShowErrorMessage(string message)
        {
            string text = $"{message}\n请关闭工具后重新打开";
            string caption = "不温馨提示";

            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ToggleFishingPressKey(byte curCode, byte newCode)
        {
            if (curCode == newCode)
            {
                return;
            }
            if (curCode == 0)
            {
                SimulateEventHandler.SendScanCodeKeyDown(newCode);
                return;
            }
            if (newCode == 0)
            {
                SimulateEventHandler.SendScanCodeKeyUp(curCode);
                return;
            }
            if (newCode == SimulateEventHandler.SCAN_A)
            {
                SimulateEventHandler.SendScanCodeKeyUp(SimulateEventHandler.SCAN_D);
                SimulateEventHandler.SendScanCodeKeyDown(newCode);
                return;
            }
            if (newCode == SimulateEventHandler.SCAN_D)
            {
                SimulateEventHandler.SendScanCodeKeyUp(SimulateEventHandler.SCAN_A);
                SimulateEventHandler.SendScanCodeKeyDown(newCode);
                return;
            }
        }

        private long _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        private async Task FishingLoop(ConcurrentDictionary<string, TaskCompletionSource<bool>> tcsDict, CancellationToken token)
        {
            int lostFishingCount = 0; // 连续丢失钓鱼绿条或光标次数
            byte curKeyScanCode = 0; // 当前按下的键的扫描码

            int lostFishCount = 0; // 鱼溜走次数
            long lastLostFishTime = DateTimeOffset.Now.ToUnixTimeSeconds(); // 上次遛鱼失败的时间戳

            // 用于光标匹配。
            double matchMinSimilarity = 0.9; // 模板匹配的最小相似度

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (tcsDict.TryGetValue("FishingLoop", out var tcs))
                    {
                        // 如果tcs.Task未完成，代码会在此处异步挂起，不阻塞线程池线程
                        await tcs.Task;
                    }

                    // 循环捕获游戏窗口图像并处理
                    using (Bitmap windowImg = CaptureWindow(_intPtrGame))
                    {
                        // 连续5次及以上遛鱼失败，大概率是钓鱼绿条和光标无法正确识别，重置一下状态试试。
                        if (lostFishCount >= 5)
                        {
                            _curFishState = EFishState.Back;
                        }

                        try
                        {
                            switch (_curFishState)
                            {
                                case EFishState.Back:
                                    FishScene.BackToGameIdle();
                                    lostFishCount = 0;
                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                    continue;
                                case EFishState.GameIdle:
                                    FishScene.GoToFishingIdle(windowImg);
                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                    continue;
                                case EFishState.BuyBait:
                                    FishScene.HandleBuyBait(_intPtrGame);
                                    _curFishState = EFishState.Fishing;
                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                    continue;
                                case EFishState.SaleFish:
                                    FishScene.HandleSaleFish(_intPtrGame);
                                    _curFishState = EFishState.Fishing;
                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                    continue;
                                case EFishState.MoonCard:
                                    FishScene.HandleMoonCard(windowImg);
                                    _curFishState = EFishState.Fishing;
                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                    continue;
                            }

                            Point? locBar = FishScene.GetFishBarPoint(windowImg, true);
                            Point? locPoint = TemplateController.MathTemplateImgByName(windowImg, _intPtrGame, ETemplateName.FishingPoint);

                            if (locBar == null || locPoint == null)
                            {
                                lostFishingCount++;
                            }
                            else
                            {
                                lostFishingCount = 0;
                            }

                            // 判断是否进入遛鱼逻辑，优先级较高。
                            if (locBar != null && locPoint != null)
                            {
                                lostFishCount = 0;
                                //Console.WriteLine($"绿条位置: ({locBar.Value.X}, {locBar.Value.Y}) == 光标位置: ({locPoint.Value.X}, {locPoint.Value.Y})");

                                _curFishState = EFishState.Fishing;
                                _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                int distance = locBar.Value.X - locPoint.Value.X;

                                if (distance < 20 && distance > -20)
                                {
                                    ToggleFishingPressKey(curKeyScanCode, 0);
                                    curKeyScanCode = 0;
                                }
                                else if (locBar.Value.X > locPoint.Value.X)
                                {
                                    ToggleFishingPressKey(curKeyScanCode, SimulateEventHandler.SCAN_D);
                                    curKeyScanCode = SimulateEventHandler.SCAN_D;
                                }
                                else if (locBar.Value.X < locPoint.Value.X)
                                {
                                    ToggleFishingPressKey(curKeyScanCode, SimulateEventHandler.SCAN_A);
                                    curKeyScanCode = SimulateEventHandler.SCAN_A;
                                }
                            }
                            else if (lostFishingCount >= 10) // 连续多次未找到钓鱼绿条和光标，进入各项判断逻辑。
                            {
                                if (curKeyScanCode != 0)
                                {
                                    ToggleFishingPressKey(curKeyScanCode, 0);
                                    curKeyScanCode = 0;
                                }

                                // 判断是否进入上钩逻辑
                                Point? clickToFishingLoc =
                                    TemplateController.MathTemplateImgByName(windowImg, _intPtrGame, ETemplateName.TakesTheBait);
                                if (clickToFishingLoc != null)
                                {
                                    SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                                    lostFishingCount = 0;
                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                                    continue;
                                }

                                // 判断是否进入收杆逻辑
                                Point? weightGramLoc =
                                    TemplateController.MathTemplateImgByName(windowImg, _intPtrGame, ETemplateName.FishWeightGram);
                                if (weightGramLoc != null)
                                {
                                    FishScene.HandleClickToClose();

                                    // 立刻继续钓鱼
                                    SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                                    continue;
                                }

                                // 处理莫名退回到登录界面的问题
                                Point? announcementLoc = TemplateController.MathTemplateImgByName(windowImg, _intPtrGame, ETemplateName.LoginPageAnnouncement);
                                Point? announcementLightLoc = TemplateController.MathTemplateImgByName(windowImg, _intPtrGame, ETemplateName.LoginPageAnnouncementLight);
                                if (announcementLoc != null || announcementLightLoc != null)
                                {
                                    Thread.Sleep(2500);

                                    FishScene.HandleClickToClose();

                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                                    continue;
                                }

                                // 处理是否处在游戏待机界面。
                                Point? enterFLoc = TemplateController.MathTemplateImgByName(windowImg, _intPtrGame, ETemplateName.EnterFKeyToFishing);
                                if (enterFLoc != null)
                                {
                                    _curFishState = EFishState.GameIdle;
                                    continue;
                                }

                                // 处理月卡，并非整点结算月卡
                                Point? moonCardLoc = TemplateController.MathTemplateImgByName(windowImg, _intPtrGame, ETemplateName.MoonCard);
                                if (moonCardLoc != null)
                                {
                                    _curFishState = EFishState.MoonCard;
                                    continue;
                                }

                                // 处理中间提示条逻辑（中间提示条会遮挡A键提示）。
                                // 中间提示条出现的可能性目前有三种，一是遛鱼失败，二是鱼饵用完了，三是鱼舱满了。
                                // 但无论是哪一种，先按一次F，如果不再出现中间提示条，说明是遛鱼失败
                                // 如果依然出现提示条，那么先执行卖鱼，然后是尝试更换鱼饵，如果更换鱼饵失败则进入购买鱼饵逻辑。
                                if (FishScene.CheckIsCenterTips(windowImg))
                                {
                                    // 按下F抛竿不再触发中间提示条，走鱼溜走了的逻辑。
                                    if (FishScene.CheckIsLostFish())
                                    {
                                        long currentLostTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                        // 如果距离上次遛鱼失败未超过3分钟，则计入失败次数。
                                        if (currentLostTime - lastLostFishTime < 60 * 3 || lostFishCount == 0)
                                        {
                                            lostFishCount++;
                                            lastLostFishTime = currentLostTime;
                                        }

                                        _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                        continue;
                                    }

                                    FishScene.HandleBaitEmpty(_intPtrGame);
                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                    continue;
                                }

                                // 超过40秒没有任何操作，主动回退到游戏待机界面
                                if (DateTimeOffset.Now.ToUnixTimeSeconds() - _lastOperationTime >= 40)
                                {
                                    _curFishState = EFishState.Back;
                                    continue;
                                }

                                // 被标记为待机状态，或者超过10秒没有任何操作，主动触发一次抛竿
                                if (_curFishState == EFishState.Idle
                                    || DateTimeOffset.Now.ToUnixTimeSeconds() - _lastOperationTime >= 10)
                                {
                                    SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                                    lostFishingCount = 0;
                                    _curFishState = EFishState.Fishing;
                                }

                                continue;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("===========================内层===========================");
                            Console.WriteLine(e.Message);
                            Console.WriteLine("===========================内层===========================");
                            // 报错了不管怎么样，重置下按键事件，然后直接回退到游戏待机界面，重新来过。
                            if (curKeyScanCode != 0)
                            {
                                ToggleFishingPressKey(curKeyScanCode, 0);
                                curKeyScanCode = 0;
                            }
                            _curFishState = EFishState.Back;
                        }
                    }

                    Thread.Sleep(FISHING_INTERVAL_MS);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("任务 [FishingLoop] 收到取消信号");
            }
            catch (Exception e)
            {
                Console.WriteLine("==========================外层==========================");
                Console.WriteLine(e.Message);
                Console.WriteLine("==========================外层==========================");
                ShowErrorMessage(e.Message);
            }
            finally
            {
                Console.WriteLine("任务 [FishingLoop] 已停止");
            }
        }
    }
}
