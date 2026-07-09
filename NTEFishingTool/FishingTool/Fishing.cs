using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
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
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string GAME_PROCESS_NAME = "HTGame"; // 异环的程序名
        private const int FISHING_INTERVAL_MS = 1000 / 60; // 默认60帧率

        private Process _prcGame; // 游戏进程对象
        private IntPtr _intPtrGame; // 游戏窗口句柄
        private EFishState _curFishState = EFishState.Idle; // 当前的钓鱼状态
        private int _fishingPointDistance = 20; // 钓鱼光标与绿条中心的距离阈值。

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

        private void InitializeData()
        {
            ProcessHandler.SetForegroundWindow(_intPtrGame);
            Thread.Sleep(200);

            GetPureClientRect(_intPtrGame, out RECT windowRect);
            if (windowRect != TemplateController._curWindowRect)
            {
                if (TemplateController._curWindowRect.Right != 0)
                {
                    Log.Info("【InitializeData】窗口尺寸发生变化，重新初始化数据");
                }

                TemplateController.InitRatio(windowRect);
                TemplateController.InitializeImages(_intPtrGame);
                _fishingPointDistance = 20 * ((windowRect.Bottom - windowRect.Top) / 720);
            }
        }

        public void Start()
        {
            try
            {
                if (_tasks == null)
                {
                    Log.Info("【Start】=========================================================");
                    Log.Info("【Start】工具启动，开始初始化数据...");

                    _prcGame = ProcessHandler.GetProcess(GAME_PROCESS_NAME);

                    if (_prcGame == null)
                    {
                        throw new Exception("游戏未运行");
                    }

                    // 初始化一些数据。
                    _intPtrGame = _prcGame.MainWindowHandle;
                    InitializeData();

                    // 数据初始化完成，开始执行钓鱼任务
                    _tasks = new TaskController();
                    _curRunningTaskName = "FishingLoop";
                    _tasks.StartTask(_curRunningTaskName, FishingLoop);
                    _isTaskRunning = true;
                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                    Log.Info("【Start】数据初始化完毕");
                    Log.Info("【Start】=========================================================");
                }
                else if (!_isTaskRunning)
                {
                    InitializeData();

                    _tasks.ResumeTasks(_curRunningTaskName);
                    _isTaskRunning = true;
                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "启动钓鱼任务时发生错误\n");
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
            }
        }

        private long _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        private async Task FishingLoop(ConcurrentDictionary<string, TaskCompletionSource<bool>> tcsDict, CancellationToken token)
        {
            int lostFishingCount = 0; // 连续丢失钓鱼绿条或光标次数
            byte curKeyScanCode = 0; // 当前按下的键的扫描码

            int lostFishCount = 0; // 鱼溜走次数
            long lastLostFishTime = DateTimeOffset.Now.ToUnixTimeSeconds(); // 上次遛鱼失败的时间戳

            bool writeFishingLogFlag = true; // 是否记录一次钓鱼中状态。

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (tcsDict.TryGetValue("FishingLoop", out var tcs))
                    {
                        // 如果tcs.Task未完成，代码会在此处异步挂起，不阻塞线程池线程
                        await tcs.Task;
                    }
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
                                FishScene.GoToFishingIdle();
                                lostFishCount = 0;
                                _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                continue;
                            case EFishState.BuyBait:
                                FishScene.HandleBuyBait(_intPtrGame);
                                lostFishCount = 0;
                                _curFishState = EFishState.Idle;
                                _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                continue;
                            // 出售逻辑已合并到鱼饵为空逻辑。
                            //case EFishState.SaleFish:
                            //    FishScene.HandleSaleFish(_intPtrGame);
                            //    _curFishState = EFishState.Fishing;
                            //    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                            //    continue;
                            case EFishState.MoonCard:
                                FishScene.HandleMoonCard();
                                _curFishState = EFishState.Fishing;
                                _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                continue;
                        }

                        Point? locBar = null;
                        Point? locPoint = null;

                        using (Bitmap fishBarCropImg = CaptureWindow(_intPtrGame, ETemplateName.FishingPoint))
                        {
                            locBar = FishScene.GetFishBarPoint(true, fishBarCropImg);
                            locPoint = TemplateController.MatchTemplateImgByName(_intPtrGame, ETemplateName.FishingPoint, fishBarCropImg);
                        }

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
                            if (writeFishingLogFlag)
                            {
                                Log.Info("【Fishing】遛鱼中...");
                                lostFishCount = 0; // 能钓到鱼，遛鱼失败次数重置。
                                writeFishingLogFlag = false; // 每次遛鱼开始时记录一次日志，避免日志过于频繁。
                            }

                            //Console.WriteLine($"绿条位置: ({locBar.Value.X}, {locBar.Value.Y}) == 光标位置: ({locPoint.Value.X}, {locPoint.Value.Y})");

                            _curFishState = EFishState.Fishing;
                            _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                            int distance = Math.Abs(locBar.Value.X - locPoint.Value.X);

                            if (distance < _fishingPointDistance)
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
                            writeFishingLogFlag = true;

                            if (curKeyScanCode != 0)
                            {
                                ToggleFishingPressKey(curKeyScanCode, 0);
                                curKeyScanCode = 0;
                            }

                            // 判断是否进入上钩逻辑
                            Point? clickToFishingLoc =
                                TemplateController.MatchTemplateImgByName(_intPtrGame, ETemplateName.TakesTheBait);
                            if (clickToFishingLoc != null)
                            {
                                Log.Info("【Fishing】检测到鱼上钩，开始遛鱼");

                                SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                                lostFishingCount = 0;
                                _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                                continue;
                            }

                            // 判断是否进入收杆逻辑
                            Point? weightGramLoc =
                                TemplateController.MatchTemplateImgByName(_intPtrGame, ETemplateName.FishWeightGram);
                            if (weightGramLoc != null)
                            {
                                Log.Info("【Fishing】成功钓起了一条鱼");

                                FishScene.RandomThreadSleep(1500);
                                FishScene.HandleClickToClose();

                                Log.Info("【Fishing】重新抛竿");
                                // 立刻继续钓鱼
                                SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                                _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                                continue;
                            }

                            // 处理是否处在游戏待机界面。
                            Point? enterFLoc = TemplateController.MatchTemplateImgByName(_intPtrGame, ETemplateName.EnterFKeyToFishing);
                            if (enterFLoc != null)
                            {
                                _curFishState = EFishState.GameIdle;
                                continue;
                            }

                            // 处理中间提示条逻辑（中间提示条会遮挡A键提示）。
                            // 中间提示条出现的可能性目前有三种，一是遛鱼失败，二是鱼饵用完了，三是鱼舱满了。
                            // 但无论是哪一种，再按一次F，如果不再出现中间提示条，说明是遛鱼失败
                            // 如果依然出现提示条，那么先执行卖鱼，然后是尝试更换鱼饵，如果更换鱼饵失败则进入购买鱼饵逻辑。
                            if (FishScene.CheckIsCenterTips())
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

                                    Log.Info("【FishingLoop】检测到遛鱼失败...当前已统计最近遛鱼失败 {0} 次", lostFishCount);
                                    _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                    continue;
                                }

                                Log.Info("【FishingLoop】检测到鱼饵用完...");
                                FishScene.HandleBaitEmpty(_intPtrGame);

                                _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                continue;
                            }

                            // 处理莫名退回到登录界面的问题
                            Point? announcementLoc = TemplateController.MatchTemplateImgByName(_intPtrGame, ETemplateName.LoginPageAnnouncement);
                            Point? announcementLightLoc = TemplateController.MatchTemplateImgByName(_intPtrGame, ETemplateName.LoginPageAnnouncementLight);
                            if (announcementLoc != null || announcementLightLoc != null)
                            {
                                Log.Info("【FishingLoop】检测到被踢回登录界面...");

                                FishScene.RandomThreadSleep(2500);

                                FishScene.HandleClickToClose();

                                Log.Info("【FishingLoop】正在进入游戏...");

                                _curFishState = EFishState.GameIdle;
                                _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                                continue;
                            }

                            // 处理月卡，并非整点结算月卡，因为此匹配逻辑可能会比较耗时，所以降低其优先级。
                            Point? moonCardLoc = TemplateController.MatchTemplateImgByName(_intPtrGame, ETemplateName.MoonCard);
                            if (moonCardLoc != null)
                            {
                                Log.Info("【FishingLoop】检测到月卡提示...");

                                _curFishState = EFishState.MoonCard;
                                continue;
                            }

                            // 超过40秒没有任何操作，主动回退到游戏待机界面
                            if (DateTimeOffset.Now.ToUnixTimeSeconds() - _lastOperationTime >= 40)
                            {
                                Log.Info("【FishingLoop】检测到超过40秒没有任何操作...");
                                _curFishState = EFishState.Back;
                                continue;
                            }

                            // 被标记为待机状态，主动触发一次抛竿
                            if (_curFishState == EFishState.Idle)
                            {
                                Log.Info("【FishingLoop】检测到钓鱼待机状态，执行抛竿");

                                SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                                lostFishingCount = 0;
                                _curFishState = EFishState.Fishing;
                                _lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                            }

                            // 或者超过10秒没有任何操作，主动触发一次抛竿
                            if (DateTimeOffset.Now.ToUnixTimeSeconds() - _lastOperationTime >= 10)
                            {
                                Log.Info("【FishingLoop】检测到已超过10秒没有任何操作，尝试抛竿/开始遛鱼");

                                SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                                lostFishingCount = 0;
                                _curFishState = EFishState.Fishing;
                            }

                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "【FishingLoop】执行钓鱼循环时发生异常\n");

                        // 报错了不管怎么样，重置下按键事件，然后直接回退到游戏待机界面，重新来过。
                        if (curKeyScanCode != 0)
                        {
                            ToggleFishingPressKey(curKeyScanCode, 0);
                            curKeyScanCode = 0;
                        }
                        _curFishState = EFishState.Back;
                    }

                    Thread.Sleep(FISHING_INTERVAL_MS);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("【FishingLoop】任务 [FishingLoop] 收到取消信号，任务已停止");
            }
            catch (Exception e)
            {
                Log.Error(e, "【FishingLoop】任务 [FishingLoop] 在进入循环前发生异常\n");

                ShowErrorMessage(e.Message);
            }
        }
    }
}
