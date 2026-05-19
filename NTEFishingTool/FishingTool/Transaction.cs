using System;
using System.Drawing;
using System.Threading;

namespace NTEFishingTool.FishingTool
{
    internal class Transaction
    {
        private static readonly Fishing _fishingTool = Fishing.GetInstance();

        public static bool CheckIsBaitEmpty(Bitmap windowImg, IntPtr intPtr)
        {
            Point? emptyTipsLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.BaitEmpty);

            return emptyTipsLoc != null;
        }

        public static bool HandleChangeBait(Bitmap windowImg, IntPtr intPtr)
        {
            Point? changeBtnLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.Change);
            if (changeBtnLoc != null)
            {
                SimulateEventHandler.MouseClick(changeBtnLoc.Value);
                Thread.Sleep(2000);
                _fishingTool.CurFishState = EFishState.Idle;
                return true;
            }

            return false;
        }

        public static bool HandleCloseTips(Bitmap windowImg, IntPtr intPtr)
        {
            // 关闭提示页
            Point? closeTipsLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.ClickToClose);
            if (closeTipsLoc == null)
            {
                //throw new Exception("【HandleBuyOrChangeBait】未能关闭提示");
                return false;
            }
            SimulateEventHandler.MouseClick(closeTipsLoc.Value);
            Thread.Sleep(2000);

            return true;
        }

        /// <summary>
        /// 切换/购买万能鱼饵。
        /// 如果可以直接更换，那就更换结束后继续钓鱼
        /// 如果不能更换，那么在购买之前，执行售出鱼获逻辑。
        /// 目前设计是线性流程，所以万一中途中断了那就捕获异常，回到垂钓界面重新再来
        /// </summary>
        /// <param name="intPtr"></param>
        /// <param name="isSaleFish"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static void HandleBuyOrChangeBait(IntPtr intPtr, bool isSaleFish = true)
        {
            // 打开鱼饵切换页面，等待提示信息自己消失
            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_E);
            Thread.Sleep(4000);

            //（新截图）
            Bitmap windowImg = ImageHandler.CaptureWindow(intPtr);

            // 确认是否可以直接更换，如果可以那么更换后直接跳出，并将钓鱼状态置为待机，继续钓鱼
            if (HandleChangeBait(windowImg, intPtr))
            {
                return;
            }

            if (isSaleFish)
            {
                // 退回到垂钓待机界面
                SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_ESCAPE);
                Thread.Sleep(1500);

                // 先出售鱼获，以防不够钱买鱼饵
                HandleSaleFish(intPtr);

                // 鱼获出售完毕后会回到钓鱼待机界面
                SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_E);
                Thread.Sleep(2000);
            }

            HandleBuyBait(intPtr);

            // 操作时间已经很长了，跳出逻辑后会继续钓鱼
        }

        public static void HandleBuyBait(IntPtr intPtr)
        {
            Bitmap windowImg = ImageHandler.CaptureWindow(intPtr);

            // 确认是否可以购买
            Point? toBuyLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.ToBuy);
            if (toBuyLoc == null)
            {
                throw new Exception("【HandleBuyBait】未能在切换鱼饵页面点击购买");
            }
            SimulateEventHandler.MouseClick(toBuyLoc.Value);
            Thread.Sleep(2000);

            //（新截图）
            // 进入购买页面
            windowImg = ImageHandler.CaptureWindow(intPtr);

            // 选中万能鱼饵
            Point? baitLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.BaitUniversal);
            if (baitLoc == null)
            {
                throw new Exception("【HandleBuyBait】未能在商店界面选中万能鱼饵");
            }
            SimulateEventHandler.MouseClick(baitLoc.Value);
            Thread.Sleep(1000);

            // 选中拉满鱼饵
            Point? shopingMaxLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.ShopingMax);
            if (shopingMaxLoc == null)
            {
                throw new Exception("【HandleBuyBait】未能在商店界面选中最大值");
            }
            SimulateEventHandler.MouseClick(shopingMaxLoc.Value);
            Thread.Sleep(1000);

            // 点击购买
            Point? buyLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.Buy);
            if (buyLoc == null)
            {
                throw new Exception("【HandleBuyBait】未能在商店界面选中购买");
            }
            SimulateEventHandler.MouseClick(buyLoc.Value);
            Thread.Sleep(3500);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtr);

            // 购买分两种情况
            // 一种直接购买成功，然后关闭购买提示
            // 另一种弹出确认购买，需要确认然后关闭提示
            if (!HandleCloseTips(windowImg, intPtr))
            {
                // 确认购买
                Point? buyConfirmLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.BuyConfirm);
                if (buyConfirmLoc == null)
                {
                    throw new Exception("【HandleBuyBait】未能在商店界面确认购买");
                }
                SimulateEventHandler.MouseClick(buyConfirmLoc.Value);
                Thread.Sleep(3000);

                windowImg = ImageHandler.CaptureWindow(intPtr);
                if (!HandleCloseTips(windowImg, intPtr))
                {
                    throw new Exception("【HandleBuyBait】未能关闭提示");
                }
            }

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtr);

            // 关闭商店页
            Point? pageCloseLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.PageClose);
            if (pageCloseLoc == null)
            {
                throw new Exception("【HandleBuyBait】未能关闭商店界面");
            }
            SimulateEventHandler.MouseClick(pageCloseLoc.Value);
            Thread.Sleep(1000);
        }

        public static bool IsFishHoldFull(Bitmap windowImg, IntPtr intPtr)
        {
            // 检测当前鱼舱是否已满
            var fishFullLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.FishFull);

            return fishFullLoc != null;
        }

        /// <summary>
        /// 出售鱼获，但是起始界面必须是钓鱼待机界面。
        /// 目前设计是线性流程，所以万一中途中断了那就捕获异常，回到垂钓界面重新再来
        /// </summary>
        /// <param name="intPtr">程序句柄</param>
        /// <exception cref="Exception"></exception>
        public static void HandleSaleFish(IntPtr intPtr)
        {
            // 打开仓库
            SimulateEventHandler.SendScanCodeKeyPress(SimulateEventHandler.SCAN_Q);
            Thread.Sleep(2000);

            //（新截图）
            Bitmap windowImg = ImageHandler.CaptureWindow(intPtr);

            // 是否可切换到鱼舱
            Point? storageLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.FishStorage);
            if (storageLoc == null)
            {
                throw new Exception("【HandleSaleFish】未能切换到鱼舱");
            }
            SimulateEventHandler.MouseClick(storageLoc.Value);
            Thread.Sleep(1000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtr);

            // 是否可售卖
            Point? saleLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.FishSale);
            if (saleLoc == null)
            {
                throw new Exception("【HandleSaleFish】未能找到售卖鱼获入口");
            }
            SimulateEventHandler.MouseClick(saleLoc.Value);
            Thread.Sleep(1000);

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtr);

            // 是否可确认售卖
            Point? confirmLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.Confirm);
            if (confirmLoc == null)
            {
                throw new Exception("【HandleSaleFish】未能确认售出鱼获");
            }
            SimulateEventHandler.MouseClick(confirmLoc.Value);
            Thread.Sleep(5000); // 休眠时间稍微长一点，让鱼卖一会。

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtr);

            // 是否可关闭提示页
            if (!HandleCloseTips(windowImg, intPtr))
            {
                throw new Exception("【HandleSaleFish】售卖鱼获后未能关闭提示页");
            }

            //（新截图）
            windowImg = ImageHandler.CaptureWindow(intPtr);

            // 关闭页面，回到垂钓待机界面
            Point? pageCloseLoc = FishScene.MathTemplateImgByName(windowImg, intPtr, EGameImage.PageClose);
            if (pageCloseLoc == null)
            {
                throw new Exception("【HandleSaleFish】未能从鱼仓库回到垂钓界面");
            }
            SimulateEventHandler.MouseClick(pageCloseLoc.Value);
            Thread.Sleep(1500);
        }
    }
}
