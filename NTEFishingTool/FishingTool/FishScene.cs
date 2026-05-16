using System;
using System.Drawing;

using static NTEFishingTool.FishingTool.ImageHandler;

namespace NTEFishingTool.FishingTool
{
    internal class FishScene
    {
        private static RGB rgbFishBar = new RGB(49, 218, 183); // 绿条
        private static RGB rgbFishPoint = new RGB(245, 246, 159); // 光标
        private static ImageManager imgManager = new ImageManager();

        public static (OpenCvSharp.Point? locBar, OpenCvSharp.Point? locPoint) GetFishBarAndPoint(Bitmap windowImg)
        {
            // 裁剪出钓鱼条的相对区域，避免干扰
            int rectX = (int)(windowImg.Width * 0.315);
            int rectY = (int)(windowImg.Height * 0.059) + 40;
            int rectWidth = (int)(windowImg.Width * 0.363);
            int rectHeight = (int)(windowImg.Height * 0.021);
            Bitmap fishbarImg = CropImageByRect(windowImg, new Rectangle(rectX, rectY, rectWidth, rectHeight));

            // 获取绿条和光标的位置
            OpenCvSharp.Point? locBar = DetectAreaByRgb(fishbarImg, rgbFishBar, false);
            OpenCvSharp.Point? locPoint = DetectAreaByRgb(fishbarImg, rgbFishPoint);

            return (locBar, locPoint);
        }

        /// <summary>
        /// 传入窗口高度，以判断返回游戏设置的分辨率
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        private static int GetResolutionLevel(int height)
        {
            if (height >= 719 && height < 800)
            {
                return 720;
            }
            if (height >= 1079 && height < 1440)
            {
                return 1080;
            }
            if (height >= 1439)
            {
                return 1440;
            }

            return 0; // 不支持的分辨率
        }

        /// <summary>
        /// 传入游戏窗口截图，程序句柄和需要比对的图片名
        /// 返回匹配结果，能够在窗口截图中找到比对图则返回比对图在整个桌面全屏模式下的绝对位置，否则返回null
        /// </summary>
        /// <param name="windowImg">游戏窗口截图</param>
        /// <param name="IntPtrGame">程序句柄</param>
        /// <param name="gameImg">比对的图片名</param>
        /// <returns>System.Drawing.Point?</returns>
        /// <exception cref="Exception"></exception>
        public static Point? MathTemplateImgByName(Bitmap windowImg, IntPtr IntPtrGame, EGameImage gameImg)
        {
            Bitmap templateImg = null;

            int resolutionLevel = GetResolutionLevel(windowImg.Height);

            if (resolutionLevel == 0)
            {
                throw new Exception("不支持的分辨率");
            }

            templateImg = imgManager[$"img{resolutionLevel}_{gameImg.ToString()}"];

            if (templateImg == null)
            {
                throw new Exception("传入了错误的比对图");
            }

            // 裁剪区域，以减少干扰和避免增加性能消耗
            Rectangle? rect = imgManager.GetImageRectangle(windowImg, gameImg);
            if (rect == null)
            {
                throw new Exception("未找到比对图所需裁剪区域");
            }

            Bitmap cropImg = CropImageByRect(windowImg, rect.Value);

            Point? loc = FindImageLocation(cropImg, templateImg);

            if (loc != null)
            {
                GetWindowRect(IntPtrGame, out RECT windowRect);

                // 将相对坐标转换为绝对坐标
                int absoluteX = windowRect.Left + rect.Value.X + loc.Value.X;
                int absoluteY = windowRect.Top + rect.Value.Y + loc.Value.Y;

                return new Point(absoluteX, absoluteY);
            }

            return null;
        }
    }
}
