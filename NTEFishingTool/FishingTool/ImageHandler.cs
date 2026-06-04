using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace NTEFishingTool.FishingTool
{
    internal class ImageHandler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Left == r2.Left &&
                       r1.Top == r2.Top &&
                       r1.Right == r2.Right &&
                       r1.Bottom == r2.Bottom;
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !(r1 == r2);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGB
        {
            public int R;
            public int G;
            public int B;
            public RGB(int r, int g, int b)
            {
                R = r;
                G = g;
                B = b;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        /// <summary>
        /// 获取纯净的游戏内容窗口坐标ClientRect
        /// 不包含边框、标题栏、阴影等
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="rect"></param>
        public static bool GetPureClientRect(IntPtr hWnd, out RECT rect)
        {
            try
            {
                GetClientRect(hWnd, out RECT clientRect);

                POINT topLeft = new POINT { X = 0, Y = 0 };
                ClientToScreen(hWnd, ref topLeft);

                rect = new RECT
                {
                    Top = topLeft.Y,
                    Left = topLeft.X,
                    Bottom = topLeft.Y + clientRect.Bottom,
                    Right = topLeft.X + clientRect.Right
                };
            }
            catch
            {
                rect = new RECT();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取指定窗口的屏幕截图，并返回一个Bitmap对象。
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="tmplName"></param>
        /// <returns></returns>
        public static Bitmap CaptureWindow(IntPtr hWnd, ETemplateName? tmplName = null)
        {
            IntPtr foregroundWindow = ProcessHandler.GetForegroundWindow();
            if (hWnd != foregroundWindow)
            {
                Log.Warn("【CaptureWindow】游戏窗口不在前台，正在切换到前台");

                ProcessHandler.SetForegroundWindow(hWnd);
                System.Threading.Thread.Sleep(200); // 等待窗口切换完成
            }

            RECT rect;

            // 获取窗口在屏幕上的坐标和大小
            if (tmplName.HasValue)
            {
                rect = TemplateController.GetTemplateRECT(tmplName.Value);
            }
            else
            {
                rect = TemplateController._curWindowRect;
            }

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            // 创建一个与窗口大小相同的Bitmap对象，用于存储屏幕截图
            Bitmap bmp = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                // 从屏幕上指定的区域复制图像到Bitmap对象中
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
            }

            return bmp;
        }

        /// <summary>
        /// 传入窗口高度，以判断返回游戏设置的分辨率
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public static int GetResolutionLevel(int height)
        {
            if (height < 900)
            {
                return 720;
            }
            if (height < 1439)
            {
                return 1080;
            }
            if (height < 2160)
            {
                return 1440;
            }
            return 2160;

            //if (height >= 719 && height < 770)
            //{
            //    return 720;
            //}
            //if (height >= 1079 && height < 1130)
            //{
            //    return 1080;
            //}
            //if (height >= 1439 && height < 1500)
            //{
            //    return 1440;
            //}
            //return 0; // 不支持的分辨率
        }

        /// <summary>
        /// 根据传入的HSV颜色值，计算出一个范围，用于在图像处理中进行颜色过滤。
        /// </summary>
        /// <param name="hsv">输入的HSV颜色值</param>
        /// <param name="lowRange">是否使用小范围，默认计算小范围</param>
        /// <returns>返回一个包含下限和上限的元组，用于颜色过滤</returns>
        public static (Scalar lower, Scalar upper) GetHsvRange(Scalar hsv, bool lowRange = true)
        {
            int h = (int)hsv.Val0;
            int s = (int)hsv.Val1;
            int v = (int)hsv.Val2;

            // ±5度色调，±10饱和度和亮度
            int hRange = 5;
            int sRange = 10;
            int vRange = 10;

            if (!lowRange)
            {
                hRange = 7;
                sRange = 20;
                vRange = 20;
            }

            Scalar lower = new Scalar(
                Math.Max(0, h - hRange),
                Math.Max(0, s - sRange),
                Math.Max(0, v - vRange));

            Scalar upper = new Scalar(
                Math.Min(179, h + hRange),
                Math.Min(255, s + sRange),
                Math.Min(255, v + vRange));

            return (lower, upper);
        }

        private static Mat _convertHsvPixel = new Mat();
        private static Dictionary<string, Scalar> _hsvScalarCache = new Dictionary<string, Scalar>();

        /// <summary>
        /// 将RGB颜色值转换为HSV颜色值。
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static Scalar ConvertRgbToHsv(RGB rgb)
        {
            string rgbString = $"({rgb.R}, {rgb.G}, {rgb.B})";
            if (_hsvScalarCache.ContainsKey(rgbString))
            {
                return _hsvScalarCache[rgbString];
            }

            using (Mat rgbPixel = new Mat(1, 1, MatType.CV_8UC3, new Scalar(rgb.B, rgb.G, rgb.R)))
            {
                Cv2.CvtColor(rgbPixel, _convertHsvPixel, ColorConversionCodes.BGR2HSV);

                Vec3b hsv = _convertHsvPixel.At<Vec3b>(0, 0);
                _hsvScalarCache[rgbString] = new Scalar(hsv.Item0, hsv.Item1, hsv.Item2);
                return _hsvScalarCache[rgbString];
            }
        }

        private static Mat _detectHsv = new Mat();
        private static Mat _detectMask = new Mat();
        private static Mat _structuringElement =
            Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(20, 15));

        /// <summary>
        /// 根据给定的截图，以及rgb值，检测图像中是否存在符合条件的颜色区域，并返回该区域的中心点坐标。
        /// </summary>
        /// <param name="screenFrame">传入的截图</param>
        /// <param name="rgb">RGB颜色值</param>
        /// <param name="lowRange">是否使用小范围</param>
        /// <returns>返回检测到的颜色区域的中心点坐标，如果未检测到则返回null</returns>
        public static Point? DetectAreaByRgb(Bitmap screenFrame, RGB? rgb = null, bool lowRange = true)
        {
            using (Mat frame = screenFrame.ToMat())
            {
                Cv2.CvtColor(frame, _detectHsv, ColorConversionCodes.BGR2HSV);

                Scalar lowerColor;
                Scalar upperColor;

                if (rgb.HasValue)
                {
                    (lowerColor, upperColor) = GetHsvRange(ConvertRgbToHsv(rgb.Value), lowRange);
                }
                else
                {
                    (lowerColor, upperColor) = GetHsvRange(ConvertRgbToHsv(new RGB(36, 206, 170)));
                }

                Cv2.InRange(_detectHsv, lowerColor, upperColor, _detectMask);

                // 去噪声，使用开运算（先腐蚀后膨胀）来去除小的噪点
                // 改用闭运算（先膨胀后腐蚀），加上一个矩形内核用作替代光标，连接两节钓鱼条。
                Cv2.MorphologyEx(
                    _detectMask,
                    _detectMask,
                    MorphTypes.Close,
                    _structuringElement);

                // 查找轮廓
                Cv2.FindContours(
                    _detectMask,
                    out OpenCvSharp.Point[][] contours,
                    out HierarchyIndex[] hierachy,
                    RetrievalModes.External,
                    ContourApproximationModes.ApproxSimple);

                if (contours.Length > 0)
                {
                    //_detectMask.SaveImage($"./Resources/{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.png");

                    var largestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
                    var rec = Cv2.BoundingRect(largestContour);

                    return new Point(rec.X + rec.Width / 2, rec.Y + rec.Height / 2);
                }
            }

            return null;
        }

        private static Mat _matchResizedTplMat = new Mat();
        private static Mat _matchTmplResult = new Mat();

        private static void MatchImageTemplate(Mat refMat, Mat tplMat, double scale, ref double bestMaxVal, ref OpenCvSharp.Point bestLoc, ref double bestScale)
        {
            // 根据当前的缩放比例调整模板图像的大小
            Cv2.Resize(tplMat, _matchResizedTplMat, new OpenCvSharp.Size(tplMat.Cols * scale, tplMat.Rows * scale));

            if (_matchResizedTplMat.Cols > refMat.Cols || _matchResizedTplMat.Rows > refMat.Rows)
            {
                return; // 跳过模板图像大于屏幕截图的情况
            }

            // 使用归一化相关系数匹配方法进行模板匹配，并获取匹配结果
            Cv2.MatchTemplate(refMat, _matchResizedTplMat, _matchTmplResult, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(_matchTmplResult, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

            if (maxVal > bestMaxVal)
            {
                bestMaxVal = maxVal;
                bestLoc = maxLoc;
                bestScale = scale;
            }
        }

        public static Point? FindImageLocation(Bitmap screenSource, Mat templateImg, double minSimilarity = 0.8)
        {
            Mat tplMat = templateImg;
            using (Mat refMat = screenSource.ToMat())
            {
                double bestMaxVal = 0;
                OpenCvSharp.Point bestLoc = new OpenCvSharp.Point();
                double bestScale = 1.0;

                // 直接匹配原始大小的模板图像
                MatchImageTemplate(refMat, tplMat, 1.0, ref bestMaxVal, ref bestLoc, ref bestScale);

                if (bestMaxVal <= minSimilarity)
                {
                    // 尝试从0.5倍到1.5倍的缩放比例，步长为0.1，来匹配模板图像
                    for (double scale = 0.5; scale <= 1.5; scale += 0.1)
                    {
                        if (scale == 1.0) continue; // 已经匹配过原始大小的模板图像，跳过

                        MatchImageTemplate(refMat, tplMat, scale, ref bestMaxVal, ref bestLoc, ref bestScale);
                        if (bestMaxVal > minSimilarity)
                        {
                            break; // 找到匹配度足够高的结果，跳出循环
                        }
                    }
                }

                if (bestMaxVal > minSimilarity)
                {
                    int centerX = bestLoc.X + (int)(tplMat.Cols * bestScale / 2);
                    int centerY = bestLoc.Y + (int)(tplMat.Rows * bestScale / 2);
                    return new Point(centerX, centerY);
                }
            }

            return null;
        }

        /// <summary>
        /// 裁剪图片，返回一个新的Bitmap对象，包含原图中cropRect指定的区域。
        /// </summary>
        /// <param name="sourceImg">原始图像</param>
        /// <param name="cropRect">裁剪区域</param>
        /// <returns>裁剪后的图像</returns>
        public static Bitmap CropImageByRect(Bitmap sourceImg, Rectangle cropRect)
        {
            // 创建一个新的 Bitmap 对象来存储裁剪后的图像
            Bitmap croppedImg = new Bitmap(cropRect.Width, cropRect.Height);

            // 使用 Graphics 对象从原始图像中裁剪指定区域并绘制到新的 Bitmap 上
            using (Graphics g = Graphics.FromImage(croppedImg))
            {
                g.DrawImage(sourceImg, new Rectangle(0, 0, croppedImg.Width, croppedImg.Height), cropRect, GraphicsUnit.Pixel);
            }

            return croppedImg;
        }
    }
}
