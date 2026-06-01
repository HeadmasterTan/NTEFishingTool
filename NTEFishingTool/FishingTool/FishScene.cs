using System;
using System.Drawing;
using System.Threading;

using Point = System.Drawing.Point;
using static NTEFishingTool.FishingTool.ImageHandler;

namespace NTEFishingTool.FishingTool
{
    internal class FishScene
    {
        private static readonly RGB RgbFishBar = new RGB(49, 218, 183); // 绿条
        private static readonly RGB RgbFishPoint = new RGB(245, 246, 159); // 光标
        private static readonly RGB RgbCenterTips = new RGB(255, 255, 255); // 中间提示条
        private static readonly Fishing _fishingTool = Fishing.GetInstance();

        public static Point? GetCenterTipsPoint(Bitmap windowImg)
        {
            Rectangle rect = TemplateController.GetTemplateRect(windowImg, ETemplateName.CenterTips);

            using (Bitmap cropImage = CropImageByRect(windowImg, rect))
            {
                Point? point = DetectAreaByRgb(cropImage, RgbCenterTips);
                return point;
            }
        }

        /// <summary>
        /// 点击空白处关闭。
        /// </summary>
        public static void HandleClickToClose()
        {
            Point randomClickPoint =
                TemplateController.GetRectangleRandomPoint(_fishingTool.IntPtrGame, ETemplateName.ClickEmptyAreaToClose);
            SimulateEventHandler.MouseClick(randomClickPoint);
            Thread.Sleep(1500);
        }

        /// <summary>
        /// 是否出现了中间提示条。
        /// 用三个依据来判断是否是中间提示条：
        /// 当A键提示被遮盖找不到，F键提示还在，并且中间出现白色匹配时，说明出现了中间提示条。
        /// </summary>
        /// <param name="windowImg"></param>
        /// <returns></returns>
        public static bool CheckIsCenterTips(Bitmap windowImg)
        {
            Point? enterAKeyPoint =
                TemplateController.MathTemplateImgByName(windowImg, _fishingTool.IntPtrGame, ETemplateName.EnterAKeyToLeft);
            Point? fishSceneFKeyPoint =
                TemplateController.MathTemplateImgByName(windowImg, _fishingTool.IntPtrGame, ETemplateName.FishingSceneFKey);
            Point? centerTipsPoint = GetCenterTipsPoint(windowImg);

            return enterAKeyPoint == null && fishSceneFKeyPoint.HasValue && centerTipsPoint.HasValue;
        }

        /// <summary>
        /// 判断是否鱼溜走了。
        /// </summary>
        /// <returns></returns>
        public static bool CheckIsLostFish()
        {
            Thread.Sleep(4000); // 等待提示条消失。

            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
            Thread.Sleep(600);

            using (Bitmap windowImg = CaptureWindow(_fishingTool.IntPtrGame))
            {
                // 连续两次触发中间的提示条，说明并不是鱼溜走了。
                if (CheckIsCenterTips(windowImg))
                {
                    return false;
                }
            }

            // 否则说明只是鱼溜走了。
            return true;
        }

        /// <summary>
        /// 获取钓鱼绿条的中心点坐标。
        /// </summary>
        /// <param name="windowImg"></param>
        /// <param name="absolutePoi">是否取绝对坐标</param>
        /// <returns></returns>
        public static Point? GetFishBarPoint(Bitmap windowImg, bool absolutePoi = false)
        {
            Rectangle rect = TemplateController.GetTemplateRect(windowImg, ETemplateName.FishingPoint);

            using (Bitmap cropImage = CropImageByRect(windowImg, rect))
            {
                Point? point = DetectAreaByRgb(cropImage, RgbFishBar, false);
                if (point != null && absolutePoi)
                {
                    GetPureClientRect(_fishingTool.IntPtrGame, out RECT clientRect);
                    point = new Point(
                        clientRect.Left + rect.X + point.Value.X,
                        clientRect.Top + rect.Y + point.Value.Y);
                }
                return point;
            }
        }

        // 旧版本，暂时保留。
        public static (Point? locBar, Point? locPoint) GetFishBarAndPoint(Bitmap windowImg)
        {
            // 裁剪出钓鱼条的相对区域，避免干扰
            int rectX = (int)(windowImg.Width * 0.315);
            int rectY = (int)(windowImg.Height * 0.0653);
            int rectWidth = (int)(windowImg.Width * 0.375);
            int rectHeight = (int)(windowImg.Height * 0.014);
            using (Bitmap cropImage = CropImageByRect(windowImg, new Rectangle(rectX, rectY, rectWidth, rectHeight)))
            {
                // 获取绿条和光标的位置
                Point? locBar = DetectAreaByRgb(cropImage, RgbFishBar, false);
                Point? locPoint = DetectAreaByRgb(cropImage, RgbFishPoint, false);
                return (locBar, locPoint);
            }
        }

        /// <summary>
        /// 处理鱼饵用完的情况。
        /// 先去出售鱼获，然后尝试更换鱼饵，如果更换成功了就直接退出，否则进入购买鱼饵的流程。
        /// </summary>
        /// <param name="intPtr"></param>
        public static void HandleBaitEmpty(IntPtr intPtr)
        {
            _fishingTool.CurFishState = EFishState.SaleFish;
            HandleSaleFish(intPtr);

            if (HandleChangeBait(intPtr))
            {
                return;
            }

            _fishingTool.CurFishState = EFishState.BuyBait;
            HandleBuyBait(intPtr);

            _fishingTool.CurFishState = EFishState.Idle;
        }

        /// <summary>
        /// 更换鱼饵
        /// 如果更换成功，将当前状态设置为待机。
        /// 如果没更换成功，会直接进入商店页，且当前状态设置为购买鱼饵。
        /// </summary>
        /// <param name="intPtr"></param>
        /// <returns></returns>
        public static bool HandleChangeBait(IntPtr intPtr)
        {
            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_E);
            Thread.Sleep(3000);

            // 弹窗都没有，说明失败。
            using (Bitmap windowImg = CaptureWindow(intPtr))
            {
                Point? confirmDialogPoi =
                    TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.ConfirmDialogButton);
                if (confirmDialogPoi == null)
                {
                    throw new Exception("【HandleChangeBait】未能找到确认弹窗");
                }
            }

            // 不管他是更换，还是购买，点了再说。
            Point confirmBtnPoint =
                TemplateController.GetRectangleCenterPoint(intPtr, ETemplateName.ConfirmDialogConfirmButton);

            SimulateEventHandler.MouseClick(confirmBtnPoint);
            Thread.Sleep(2000);

            // 点完之后检查是否可以查找到万能鱼饵图标，如果能找到，说明进入了商店页。如果找不到，说明更换鱼饵成功。
            using (Bitmap windowImg = CaptureWindow(intPtr))
            {
                Point? baitPoint = TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.UniversalBait);
                if (baitPoint != null)
                {
                    _fishingTool.CurFishState = EFishState.BuyBait;
                    return false;
                }
            }

            _fishingTool.CurFishState = EFishState.Idle;
            return true;
        }

        /// <summary>
        /// 购买鱼饵
        /// 目前设计是线性流程，所以万一中途中断了那就捕获异常，回到垂钓界面重新再来
        /// </summary>
        /// <param name="intPtr"></param>
        /// <exception cref="Exception"></exception>
        public static void HandleBuyBait(IntPtr intPtr)
        {
            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_R);
            Thread.Sleep(3000);

            //（新截图）
            // 进入购买页面
            using (Bitmap windowImg = CaptureWindow(intPtr))
            {
                // 选中万能鱼饵
                Point? baitLoc = TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.UniversalBait);
                if (baitLoc == null)
                {
                    throw new Exception("【HandleBuyBait】未能在商店界面选中万能鱼饵");
                }
                SimulateEventHandler.MouseClick(baitLoc.Value);
                Thread.Sleep(1500);

                // 选中拉满鱼饵
                Point? maximumLoc = TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.Maximum);
                if (maximumLoc == null)
                {
                    throw new Exception("【HandleBuyBait】未能在商店界面选中最大值");
                }
                SimulateEventHandler.MouseClick(maximumLoc.Value);
                Thread.Sleep(1500);

                // 上边都没报错，说明可以直接点击购买
                Point purchasePoint = TemplateController.GetRectangleCenterPoint(intPtr, ETemplateName.ShopScenePurchaseButton);
                SimulateEventHandler.MouseClick(purchasePoint);
                Thread.Sleep(3500);
            }

            //（新截图）
            using (Bitmap windowImg = CaptureWindow(intPtr))
            {
                // 购买分两种情况
                // 一种直接购买成功，然后关闭购买成功提示
                // 另一种弹出确认购买，需要先确认购买，然后关闭购买成功提示
                Point? confirmDialogPoi =
                    TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.ConfirmDialogButton);
                if (confirmDialogPoi != null)
                {
                    Point confirmBtnPoint = TemplateController.GetRectangleCenterPoint(intPtr, ETemplateName.ConfirmDialogConfirmButton);
                    SimulateEventHandler.MouseClick(confirmBtnPoint);
                    Thread.Sleep(2500);
                }

                HandleClickToClose();
            }

            //（新截图）
            using (Bitmap windowImg = CaptureWindow(intPtr))
            {
                // 关闭商店页
                Point? pageCloseLoc = TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.CloseIcon);
                if (pageCloseLoc == null)
                {
                    throw new Exception("【HandleBuyBait】未能关闭商店界面");
                }
                SimulateEventHandler.MouseClick(pageCloseLoc.Value);
                Thread.Sleep(1500);
            }
        }

        /// <summary>
        /// 出售鱼获，但是起始界面必须是钓鱼待机界面。
        /// 目前设计是线性流程，所以万一中途中断了那就捕获异常，回到垂钓界面重新再来
        /// </summary>
        /// <param name="intPtr">程序句柄</param>
        /// <exception cref="Exception"></exception>
        public static void HandleSaleFish(IntPtr intPtr)
        {
            // 打开仓库界面。
            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_Q);
            Thread.Sleep(2000);

            //（新截图）
            using (Bitmap windowImg = CaptureWindow(intPtr))
            {
                // 是否可切换到鱼舱
                Point? fishHoldInactivePoi =
                    TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.FishHoldInactive);
                if (fishHoldInactivePoi == null)
                {
                    throw new Exception("【HandleSaleFish】未能切换到鱼舱");
                }
                SimulateEventHandler.MouseClick(fishHoldInactivePoi.Value);
                Thread.Sleep(1500);
            }

            //（新截图）
            using (Bitmap windowImg = CaptureWindow(intPtr))
            {
                // 鱼舱是空的，直接退出。
                Point? emptyPoint = TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.FishHoldEmpty);
                if (emptyPoint != null)
                {
                    return;
                }

                // 是否可售卖
                Point? fishHoldActivePoi =
                    TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.FishHoldActive);
                if (fishHoldActivePoi == null)
                {
                    throw new Exception("【HandleSaleFish】未能找到售卖鱼获入口");
                }

                Point salePoint =
                    TemplateController.GetRectangleCenterPoint(intPtr, ETemplateName.FishHoldQuickSellButton);

                SimulateEventHandler.MouseClick(salePoint);
                Thread.Sleep(1500);
            }

            //（新截图）
            using (Bitmap windowImg = CaptureWindow(intPtr))
            {
                // 是否可确认售卖
                Point? confirmDialogPoi =
                    TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.ConfirmDialogButton);
                if (confirmDialogPoi == null)
                {
                    throw new Exception("【HandleSaleFish】未能确认售出鱼获");
                }

                Point confirmBtnPoint =
                    TemplateController.GetRectangleCenterPoint(intPtr, ETemplateName.ConfirmDialogConfirmButton);

                SimulateEventHandler.MouseClick(confirmBtnPoint);
                Thread.Sleep(6000); // 休眠时间稍微长一点，让鱼卖一会。
            }

            // 直接关闭
            HandleClickToClose();

            //（新截图）
            using (Bitmap windowImg = CaptureWindow(intPtr))
            {
                // 关闭页面，回到垂钓待机界面
                Point? pageClosePoint =
                    TemplateController.MathTemplateImgByName(windowImg, intPtr, ETemplateName.CloseIcon);
                if (pageClosePoint == null)
                {
                    throw new Exception("【HandleSaleFish】未能从鱼仓库回到垂钓界面");
                }

                SimulateEventHandler.MouseClick(pageClosePoint.Value);
                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// 处理月卡逻辑。
        /// </summary>
        /// <param name="windowImg"></param>
        public static void HandleMoonCard(Bitmap windowImg)
        {
            Point? moonCardLoc = TemplateController.MathTemplateImgByName(windowImg, _fishingTool.IntPtrGame, ETemplateName.MoonCard);
            if (moonCardLoc == null)
            {
                // 没有月卡，无事发生
                return;
            }
            SimulateEventHandler.MouseClick(moonCardLoc.Value);
            Thread.Sleep(5000);

            HandleClickToClose();
            Thread.Sleep(2000);
        }

        /// <summary>
        /// 回退到游戏待机界面，充当状态重置。
        /// </summary>
        public static void BackToGameIdle()
        {
            while (true)
            {
                using (Bitmap windowImg = CaptureWindow(_fishingTool.IntPtrGame))
                {
                    // 回退到能够看到【钓鱼】按钮时，停止
                    Point? enterFLoc = TemplateController.MathTemplateImgByName(windowImg, _fishingTool.IntPtrGame, ETemplateName.EnterFKeyToFishing);
                    if (enterFLoc != null)
                    {
                        _fishingTool.CurFishState = EFishState.GameIdle;
                        break;
                    }

                    SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_ESCAPE);
                    Thread.Sleep(2000);
                }

            }

            Thread.Sleep(2000);
        }

        /// <summary>
        /// 从游戏待机界面进入钓鱼待机界面。
        /// </summary>
        /// <param name="windowImg"></param>
        /// <exception cref="Exception"></exception>
        public static void GoToFishingIdle(Bitmap windowImg)
        {
            IntPtr intPtrGame = _fishingTool.IntPtrGame;

            Point? enterFLoc = TemplateController.MathTemplateImgByName(windowImg, intPtrGame, ETemplateName.EnterFKeyToFishing);
            if (enterFLoc == null)
            {
                throw new Exception("【GoToFishingIdle】未找到【钓鱼】按钮");
            }
            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_F);
            Thread.Sleep(2000);

            using (Bitmap captureWindow = CaptureWindow(intPtrGame))
            {
                // 检查是否缺少鱼饵
                Point? selBaitLoc =
                    TemplateController.MathTemplateImgByName(captureWindow, intPtrGame, ETemplateName.StartSceneSelectBait);
                if (selBaitLoc != null)
                {
                    SimulateEventHandler.MouseClick(selBaitLoc.Value);
                    Thread.Sleep(1500);

                    // 更换鱼饵失败，但未报错，说明现在在商店页。
                    if (!HandleChangeBait(intPtrGame))
                    {
                        // 购买鱼饵
                        HandleBuyBait(intPtrGame);

                        // 再次点击选择鱼饵
                        SimulateEventHandler.MouseClick(selBaitLoc.Value);
                        Thread.Sleep(1500);

                        // 更换鱼饵
                        HandleChangeBait(intPtrGame);
                    }
                }
            }

            // 没有必要检查模板，直接点击开始钓鱼。
            Point startFishingLoc =
                TemplateController.GetRectangleCenterPoint(intPtrGame, ETemplateName.StartSceneStartButton);

            SimulateEventHandler.MouseClick(startFishingLoc);
            Thread.Sleep(2000);

            _fishingTool.CurFishState = EFishState.Idle;
        }
    }
}
