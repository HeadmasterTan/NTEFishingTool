using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace FishingTool
{
    enum E_FishState
    {
        Idle, // 待机中
        Fishing, // 钓鱼中
        BaitEmpty, // 鱼饵空了
        Full, // 鱼获满了
    }

    internal class Fishing
    {
        private const string GAME_PROCESS_NAME = "HTGame"; // 异环的程序名
        private const int FISHING_INTERVAL_MS = 1000 / 60; // 默认60帧率

        private Process prcGame; // 游戏进程对象
        private IntPtr intPtrGame; // 游戏窗口句柄
        private E_FishState curFishState = E_FishState.Idle; // 当前的钓鱼状态

        private TaskController tasks;
        private string curRunningTaskName;

        private Task fishTask;
        private Task shoppingTask;

        private ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true); // 用于暂停/继续任务
        private CancellationTokenSource cts = new CancellationTokenSource(); // 用于关闭线程

        private static Fishing uniqueInstance;

        public E_FishState CurFishState
        {
            get => curFishState;
        }

        private Fishing() { }

        public static Fishing GetInstance()
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new Fishing();
            }
            return uniqueInstance;
        }

        private void PauseTask() => pauseEvent.Reset();
        private void ContinueTask() => pauseEvent.Set();
        private void StopTask() => cts.Cancel();

        public void Start()
        {
            if (tasks == null)
            {
                prcGame = ProcessHandler.GetProcess(GAME_PROCESS_NAME);

                if (prcGame == null)
                {
                    throw new Exception($"未找到运行中的游戏【异环({GAME_PROCESS_NAME})】");
                }

                intPtrGame = prcGame.MainWindowHandle;
                ProcessHandler.SetForegroundWindow(intPtrGame);
                Thread.Sleep(200);

                // 游戏进程和句柄均已记录，开始执行钓鱼任务
                //fishTask = Task.Run(() => FishingLoop(), cts.Token);
                curRunningTaskName = "FishingLoop";
                tasks.StartTask("FishingLoop", FishingLoop);
                tasks.StartTask("ShoppingLoop", ShoppingLoop);
            }
            else if (pauseEvent.IsSet == false)
            {
                //ContinueTask();
                tasks.ResumeTasks(curRunningTaskName);
            }
        }

        public void Stop()
        {
            if (tasks != null && pauseEvent.IsSet)
            {
                //PauseTask();
                tasks.PauseTask(curRunningTaskName);
            }
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

        private void FishingLoop()
        {
            int lostFishingCount = 0; // 连续丢失钓鱼绿条或光标次数
            byte curKeyScanCode = 0; // 当前按下的键的扫描码

            long lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            while (!cts.Token.IsCancellationRequested)
            {
                pauseEvent.Wait(cts.Token); // 等待继续信号

                // 循环捕获游戏窗口图像并处理
                Bitmap windowImg = ImageHandler.CaptureWindow(intPtrGame);

                (OpenCvSharp.Point? locBar, OpenCvSharp.Point? locPoint) = FishScene.GetFishBarAndPoint(windowImg);

                if (locBar == null || locPoint == null)
                {
                    lostFishingCount++;

                    // 判断是否鱼溜走了
                    Point? fishingFailLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.FishingFail);
                    if (fishingFailLoc != null)
                    {
                        Thread.Sleep(3000); // 等待提示消失，以免再进入此判断
                        SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                        lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        continue;
                    }

                    // 处理空鱼饵
                    if (HandleBaitEmpty(windowImg))
                    {
                        continue;
                    }

                    // 处理鱼舱已满
                    if (HandleFishFull(windowImg))
                    {
                        continue;
                    }

                    // 处理月卡
                    if (HandleMoonCard(windowImg))
                    {
                        continue;
                    }
                }
                else
                {
                    lostFishingCount = 0;
                }

                // 判断是否进入溜鱼逻辑
                if (locBar != null && locPoint != null)
                {
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
                else if (lostFishingCount >= 10) // 连续多次未找到钓鱼绿条或光标，可能是溜鱼结束了
                {
                    if (curKeyScanCode != 0)
                    {
                        ToggleFishingPressKey(curKeyScanCode, 0);
                        curKeyScanCode = 0;
                    }

                    // 判断是否进入上钩逻辑
                    Point? clickToFishingLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.ClickToFishing);
                    if (clickToFishingLoc != null)
                    {
                        SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                        lostFishingCount = 0;
                        lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                        continue;
                    }

                    // 判断是否进入收杆逻辑
                    Point? closeTipsLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.ClickToClose);
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

                    // 被标记为待机状态，或者超过10秒没有任何操作
                    if (curFishState == E_FishState.Idle
                        || DateTimeOffset.Now.ToUnixTimeSeconds() - lastOperationTime >= 10)
                    {
                        SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
                        lostFishingCount = 0;
                        curFishState = E_FishState.Fishing;
                        lastOperationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }

                    continue;
                }

                Thread.Sleep(FISHING_INTERVAL_MS);
            }
        }

        private void ShoppingLoop()
        {
            //if (curFishState == E_FishState.BaitEmpty)
            //{
            //    switch (loopStep)
            //    {
            //        default:
            //            break;
            //    }
            //}

            Thread.Sleep(FISHING_INTERVAL_MS * 5); // 这个任务不需要太高的频率
        }

        /// <summary>
        /// 判断鱼饵是否钓完，并切换/购买万能鱼饵。
        /// 目前设计是线性流程，所以万一中途中断了那就寄了。
        /// </summary>
        /// <param name="windowImg"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool HandleBaitEmpty(Bitmap windowImg)
        {
            // 是否提示鱼饵没了
            Point? emptyTipsLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.BaitEmpty);
            if (emptyTipsLoc == null)
            {
                return false; // 还有鱼饵
            }

            // 打开鱼饵切换页面，等待提示信息自己消失
            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_E);
            Thread.Sleep(4000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 确认是否可以直接更换，如果可以那么更换后直接跳出，并将钓鱼状态置为待机，继续钓鱼
            Point? changeBtnLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.Change);
            if (changeBtnLoc != null)
            {
                SimulateEventHandler.MouseClick(changeBtnLoc.Value);
                Thread.Sleep(1000);
                curFishState = E_FishState.Idle;
                return true;
            }

            // 确认是否可以购买
            Point? toBuyLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.ToBuy);
            if (toBuyLoc == null)
            {
                throw new Exception("【HandleBaitEmpty】未能在切换鱼饵页面点击购买");
            }
            SimulateEventHandler.MouseClick(toBuyLoc.Value);
            Thread.Sleep(1000);

            //（新截图）
            // 进入购买页面
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 选中万能鱼饵
            Point? baitLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.BaitUniversal);
            if (baitLoc == null)
            {
                throw new Exception("【HandleBaitEmpty】未能在商店界面选中万能鱼饵");
            }
            SimulateEventHandler.MouseClick(baitLoc.Value);
            Thread.Sleep(1000);

            // 选中拉满鱼饵
            Point? shopingMaxLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.ShopingMax);
            if (shopingMaxLoc == null) {
                throw new Exception("【HandleBaitEmpty】未能在商店界面选中最大值");
            }
            SimulateEventHandler.MouseClick(shopingMaxLoc.Value);
            Thread.Sleep(1000);

            // 点击购买
            Point? buyLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.Buy);
            if (buyLoc == null)
            {
                throw new Exception("【HandleBaitEmpty】未能在商店界面选中购买");
            }
            SimulateEventHandler.MouseClick(buyLoc.Value);
            Thread.Sleep(1000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 确认购买
            Point? buyConfirmLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.BuyConfirm);
            if (buyConfirmLoc == null)
            {
                throw new Exception("【HandleBaitEmpty】未能在商店界面确认购买");
            }
            SimulateEventHandler.MouseClick(buyConfirmLoc.Value);
            Thread.Sleep(3000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 关闭提示页
            Point? closeTipsLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.ClickToClose);
            if (closeTipsLoc == null)
            {
                throw new Exception("【HandleBaitEmpty】未能关闭提示");
            }
            SimulateEventHandler.MouseClick(closeTipsLoc.Value);
            Thread.Sleep(2000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 关闭商店页
            Point? pageCloseLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.PageClose);
            if (pageCloseLoc == null)
            {
                throw new Exception("【HandleBaitEmpty】未能关闭商店界面");
            }
            SimulateEventHandler.MouseClick(pageCloseLoc.Value);
            Thread.Sleep(1000);

            // 操作时长过长，跳出逻辑后会继续钓鱼
            return true;
        }

        private bool HandleMoonCard(Bitmap windowImg)
        {
            // 在本地时间凌晨四点时，持续检测3分钟是否可领月卡。
            DateTime nowTime = DateTime.Now;
            if (!(nowTime.Hour == 4 && nowTime.Minute <= 3))
            {
                return false;
            }

            Point? moonCardLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.MoonCard);
            if (moonCardLoc == null)
            {
                return false;
            }
            SimulateEventHandler.MouseClick(moonCardLoc.Value);
            Thread.Sleep(4000);

            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            Point? closeTipsLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.ClickToClose);
            if (closeTipsLoc == null)
            {
                throw new Exception("【HandleMoonCard】关闭月卡领取提示失败");
            }
            SimulateEventHandler.MouseClick(closeTipsLoc.Value);
            Thread.Sleep(3000);

            return true;
        }

        /// <summary>
        /// 清空仓库，但是起始界面必须是钓鱼待机界面。
        /// 目前设计是线性流程，所以万一中途中断了那就寄了。
        /// </summary>
        /// <param name="windowImg"></param>
        /// <exception cref="Exception"></exception>
        private bool HandleFishFull(Bitmap windowImg)
        {
            // 检测当前鱼舱是否已满
            Point? fishFullLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.FishFull);
            if (fishFullLoc == null)
            {
                return false; // 未满
            }

            //// 检测当前界面是否可打开仓库，不能则直接退出此逻辑
            //Point? iconLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.FishStorageIcon);
            //if (iconLoc == null)
            //{
            //    return;
            //}

            // 打开仓库
            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_Q);
            Thread.Sleep(2000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 切换到鱼舱
            Point? storageLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.FishStorage);
            if (storageLoc == null)
            {
                throw new Exception("【HandleFishFull】未能切换到鱼舱");
            }
            SimulateEventHandler.MouseClick(storageLoc.Value);
            Thread.Sleep(1000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 售卖
            Point? saleLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.FishSale);
            if (saleLoc == null)
            {
                throw new Exception("【HandleFishFull】未能找到售卖鱼获入口");
            }
            SimulateEventHandler.MouseClick(saleLoc.Value);
            Thread.Sleep(1000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 确认售卖
            Point? confirmLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.Confirm);
            if (confirmLoc == null)
            {
                throw new Exception("【HandleFishFull】未能确认售出鱼获");
            }
            SimulateEventHandler.MouseClick(confirmLoc.Value);
            Thread.Sleep(5000); // 休眠时间略长一点，让鱼卖一会。

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 关闭提示页
            Point? closeTipsLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.ClickToClose);
            if (closeTipsLoc == null)
            {
                throw new Exception("【HandleFishFull】售卖鱼获后未能关闭提示页");
            }
            SimulateEventHandler.MouseClick(confirmLoc.Value);
            Thread.Sleep(2000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtrGame);

            // 关闭页面，回到垂钓待机界面
            Point? pageCloseLoc = FishScene.MathTemplateImgByName(windowImg, intPtrGame, E_GameImage.PageClose);
            if (pageCloseLoc == null)
            {
                throw new Exception("【HandleFishFull】未能从鱼仓库回到垂钓界面");
            }
            SimulateEventHandler.MouseClick(pageCloseLoc.Value);
            Thread.Sleep(1500);

            return true;
        }
    }
}
