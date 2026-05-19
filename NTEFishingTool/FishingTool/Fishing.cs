using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NTEFishingTool.FishingTool
{
    enum EFishState
    {
        GameIdle, // 游戏界面待机中
        Idle, // 钓鱼界面待机中
        Fishing, // 钓鱼中
        BuyBait, // 购买鱼饵
        SaleFish, // 出售鱼获
        Back, // 回退到游戏待机界面
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
            if (_tasks == null)
            {
                _prcGame = ProcessHandler.GetProcess(GAME_PROCESS_NAME);

                if (_prcGame == null)
                {
                    throw new Exception($"未找到运行中的游戏【异环({GAME_PROCESS_NAME})】");
                }

                _intPtrGame = _prcGame.MainWindowHandle;
                ProcessHandler.SetForegroundWindow(_intPtrGame);
                Thread.Sleep(200);
                _tasks = new TaskController();

                // 游戏进程和句柄均已记录，开始执行钓鱼任务
                _isTaskRunning = true;
                _curRunningTaskName = "FishingLoop";
                _tasks.StartTask("FishingLoop", FishingLoop);
            }
            else if (!_isTaskRunning)
            {
                _tasks.ResumeTasks(_curRunningTaskName);
                _isTaskRunning = true;
            }
        }

        public void Resume()
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

        private async Task FishingLoop(ConcurrentDictionary<string, TaskCompletionSource<bool>> tcsDict, CancellationToken token)
        {
            int lostFishingCount = 0; // 连续丢失钓鱼绿条或光标次数
            byte curKeyScanCode = 0; // 当前按下的键的扫描码

            long lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

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
                    Bitmap windowImg = ImageHandler.CaptureWindow(_intPtrGame);

                    try
                    {
                        switch (_curFishState)
                        {
                            case EFishState.Back:
                                BackToGameIdle();
                                lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                continue;
                            case EFishState.GameIdle:
                                GoToFishingIdle(windowImg);
                                lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                continue;
                            case EFishState.BuyBait:
                                Transaction.HandleBuyOrChangeBait(_intPtrGame);
                                _curFishState = EFishState.Fishing;
                                lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                continue;
                            case EFishState.SaleFish:
                                Transaction.HandleSaleFish(_intPtrGame);
                                _curFishState = EFishState.Fishing;
                                lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                continue;
                            case EFishState.MoonCard:
                                HandleMoonCard(windowImg);
                                _curFishState = EFishState.Fishing;
                                lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                continue;
                        }
                    }
                    catch(Exception e)
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
                        continue;
                    }

                    (OpenCvSharp.Point? locBar, OpenCvSharp.Point? locPoint) = FishScene.GetFishBarAndPoint(windowImg);

                    if (locBar == null || locPoint == null)
                    {
                        lostFishingCount++;
                    }
                    else
                    {
                        lostFishingCount = 0;
                    }

                    // 判断是否进入溜鱼逻辑
                    if (locBar != null && locPoint != null)
                    {
                        _curFishState = EFishState.Fishing;
                        lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                        if (locBar.Value.X > locPoint.Value.X)
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
                    else if (lostFishingCount >= 10) // 连续多次未找到钓鱼绿条或光标，进入各项判断逻辑
                    {
                        if (curKeyScanCode != 0)
                        {
                            ToggleFishingPressKey(curKeyScanCode, 0);
                            curKeyScanCode = 0;
                        }

                        // 判断是否鱼溜走了
                        Point? fishingFailLoc =
                            FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.FishingFail);
                        if (fishingFailLoc != null)
                        {
                            Thread.Sleep(3000); // 等待提示消失，以免再进入此判断
                            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                            lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                            continue;
                        }

                        // 判断是否进入上钩逻辑
                        Point? clickToFishingLoc =
                            FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.ClickToFishing);
                        if (clickToFishingLoc != null)
                        {
                            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                            lostFishingCount = 0;
                            lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                            continue;
                        }

                        // 判断是否进入收杆逻辑
                        Point? closeTipsLoc =
                            FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.ClickToClose);
                        if (closeTipsLoc != null)
                        {
                            Point tempLoc = new Point(closeTipsLoc.Value.X, closeTipsLoc.Value.Y + 15); // Y轴略微向下，防遮挡
                            SimulateEventHandler.MouseClick(tempLoc);
                            Thread.Sleep(1000);

                            // 立刻继续钓鱼
                            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                            lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                            continue;
                        }

                        // 处理是否空鱼饵
                        if (Transaction.CheckIsBaitEmpty(windowImg, _intPtrGame))
                        {
                            _curFishState = EFishState.BuyBait;
                            continue;
                        }

                        // 处理鱼舱是否已满
                        if (Transaction.IsFishHoldFull(windowImg, _intPtrGame))
                        {
                            _curFishState = EFishState.SaleFish;
                            continue;
                        }

                        Point? toEnterLoc = FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.EnterToFishing);
                        if (toEnterLoc != null)
                        {
                            _curFishState = EFishState.GameIdle;
                            continue;
                        }

                        //DateTime nowTime = DateTime.Now;
                        //if (nowTime.Hour == 5 && nowTime.Minute <= 59)
                        //{
                        //}
                        // 处理月卡，并非整点结算月卡
                        Point? moonCardLoc = FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.MoonCard);
                        if (moonCardLoc != null)
                        {
                            _curFishState = EFishState.MoonCard;
                            continue;
                        }

                        // 超过40秒没有任何操作，主动回退到游戏界面
                        if (DateTimeOffset.Now.ToUnixTimeSeconds() - lastOperationTime >= 40)
                        {
                            _curFishState = EFishState.Back;
                            continue;
                        }

                        // 被标记为待机状态，或者超过10秒没有任何操作
                        if (_curFishState == EFishState.Idle
                            || DateTimeOffset.Now.ToUnixTimeSeconds() - lastOperationTime >= 10)
                        {
                            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                            lostFishingCount = 0;
                            _curFishState = EFishState.Fishing;
                        }

                        continue;
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
                MessageBox.Show(
                    $"{e.Message}\n请关闭工具后重新打开",
                    "不温馨提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Console.WriteLine("任务 [FishingLoop] 已停止");
            }
        }

        private void HandleMoonCard(Bitmap windowImg)
        {
            Point? moonCardLoc = FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.MoonCard);
            if (moonCardLoc == null)
            {
                // 没有月卡，无事发生
                return;
            }

            SimulateEventHandler.MouseClick(moonCardLoc.Value);
            Thread.Sleep(4000);

            windowImg = ImageHandler.CaptureWindow(_intPtrGame);

            Point? closeTipsLoc = FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.ClickToClose);
            if (closeTipsLoc == null)
            {
                throw new Exception("【HandleMoonCard】关闭月卡领取提示失败");
            }

            SimulateEventHandler.MouseClick(closeTipsLoc.Value);
            Thread.Sleep(3000);
        }

        private void BackToGameIdle()
        {
            bool isInnerPage = true;

            while (isInnerPage)
            {
                Bitmap windowImg = ImageHandler.CaptureWindow(_intPtrGame);

                // 回退到能够看到【钓鱼】按钮时，停止
                Point? toEnterLoc = FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.EnterToFishing);
                if (toEnterLoc != null)
                {
                    isInnerPage = false;
                    _curFishState = EFishState.GameIdle;
                    continue;
                }

                SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_ESCAPE);
                Thread.Sleep(2000);
            }
        }

        private void GoToFishingIdle(Bitmap windowImg)
        {
            Point? toEnterLoc = FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.EnterToFishing);
            if (toEnterLoc == null)
            {
                throw new Exception("【GoToFishingIdle】未找到“钓鱼”按钮");
            }
            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
            Thread.Sleep(2000);

            windowImg = ImageHandler.CaptureWindow(_intPtrGame);

            // 检查是否缺少鱼饵
            Point? selBaitLoc = FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.SelectBait);
            if (selBaitLoc != null)
            {
                SimulateEventHandler.MouseClick(selBaitLoc.Value);
                Thread.Sleep(1500);

                windowImg = ImageHandler.CaptureWindow(_intPtrGame);
                if (!Transaction.HandleChangeBait(windowImg, _intPtrGame))
                {
                    // 购买鱼饵
                    Transaction.HandleBuyBait(_intPtrGame);

                    // 更换鱼饵
                    SimulateEventHandler.MouseClick(selBaitLoc.Value);
                    Thread.Sleep(1500);

                    windowImg = ImageHandler.CaptureWindow(_intPtrGame);
                    Transaction.HandleChangeBait(windowImg, _intPtrGame);
                }

                _curFishState = EFishState.GameIdle;
                windowImg = ImageHandler.CaptureWindow(_intPtrGame);
            }

            Point? startFishingLoc = FishScene.MathTemplateImgByName(windowImg, _intPtrGame, EGameImage.ClickToStart);
            if (startFishingLoc == null)
            {
                throw new Exception("【GoToFishingIdle】未找到“开始钓鱼”按钮");
            }
            SimulateEventHandler.MouseClick(startFishingLoc.Value);
            Thread.Sleep(2000);

            _curFishState = EFishState.Idle;
        }
    }
}
