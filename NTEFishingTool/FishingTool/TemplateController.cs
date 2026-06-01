using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Point = System.Drawing.Point;
using static NTEFishingTool.FishingTool.ImageHandler;

namespace NTEFishingTool.FishingTool
{
    // 带文字的模板更新为无需匹配，直接获取中心点。
    enum ETemplateName
    {
        LoginPageAnnouncement, // 登录界面公告图标
        LoginPageAnnouncementLight, // 登录界面公告图标

        EnterFKeyToFishing, // 按F钓鱼
        StartSceneShopIcon, // 开始界面商店图标
        StartSceneSelectBait, // 开始界面选择鱼饵按钮图标。
        StartSceneStartButton, // 开始界面开始钓鱼按钮，无需模板匹配。

        UniversalBait, // 通用鱼饵图标。
        Maximum, // 最大值图标。
        CloseIcon, // 关闭图标。
        ShopScenePurchaseButton, // 商店界面购买按钮，无需模板匹配。

        ConfirmDialogButton, // 确认对话框按钮。
        ConfirmDialogConfirmButton, // 确认对话框右边的确认按钮，无需模板匹配。

        FishingPoint, // 钓鱼光标。
        SellFishShellCoin, // 卖鱼结算界面贝壳币图标。
        ClickEmptyAreaToClose, // 点击空白处关闭，无需模板匹配。

        FishHoldActive, // 鱼舱激活图标。
        FishHoldInactive, // 鱼舱未激活图标。
        FishHoldEmpty, // 鱼舱空图标。
        FishHoldQuickSellButton, // 鱼舱快速出售按钮，无需模板匹配。

        TakesTheBait, // 上钩图标。
        FishWeightGram, // 鱼重克图标。
        ConfirmTips, // 确认提示图标。
        EnterAKeyToLeft, // 按A向左
        FishingSceneFKey, // 钓鱼界面按F提示图标。
        CenterTips, // 屏幕中间提示，无需模板匹配。

        MoonCard, // 月卡图标。
    }

    internal class TemplateController
    {
        private const string TEMPLATE_PATH = "./Resources/Images/";
        private static readonly Dictionary<string, Mat> TemplateMatCache = new Dictionary<string, Mat>();
        private static readonly Dictionary<string, Bitmap> TemplateImagesCache = new Dictionary<string, Bitmap>();
        private static int _resolutionLevel;

        // 以下是游戏内的几种分辨率。
        // 2560/1600 (1.6)
        // 1920/1080 (1.7)
        // 1792/768  (2.4)
        // 3840/1080 (3.5)
        private static double _curAspectRatio = 0;

        public static void InitRatio(RECT windowRect)
        {
            double width = windowRect.Right - windowRect.Left;
            double height = windowRect.Bottom - windowRect.Top;
            _curAspectRatio = width / height;
        }

        public static double GetSimilarityRatio()
        {
            double result = Math.Abs(_curAspectRatio - 1.6);
            if (result < 0.1) return 1.6;

            result = Math.Abs(_curAspectRatio - 1.7);
            if (result < 0.1) return 1.7;

            result = Math.Abs(_curAspectRatio - 2.4);
            if (result < 0.1) return 2.4;

            result = Math.Abs(_curAspectRatio - 3.5);
            if (result < 0.1) return 3.5;

            return 0;
        }

        private static Rectangle CalculateTemplateRect(Bitmap windowImg, double x, double y, double width, double height, double ratioWidth = 1920.0, double ratioHeight = 1080.0)
        {
            y -= 39; // 适配标题栏高度。

            int windowWidth = windowImg.Width;
            int windowHeight = windowImg.Height;
            int rectX = (int)(windowWidth * (x / ratioWidth));
            int rectY = (int)(windowHeight * (y / ratioHeight));
            int rectWidth = (int)(windowWidth * (width / ratioWidth));
            int rectHeight = (int)(windowHeight * (height / ratioHeight));

            return new Rectangle(rectX, rectY, rectWidth, rectHeight);
        }

        /// <summary>
        /// 分辨率16/9
        /// </summary>
        private static Rectangle GetNormalRatioRect(Bitmap windowImg, ETemplateName templateName)
        {
            switch (templateName)
            {
                case ETemplateName.FishingPoint:
                    return CalculateTemplateRect(windowImg, 597, 102, 738, 22);
                case ETemplateName.FishHoldActive:
                    return CalculateTemplateRect(windowImg, 27, 351, 238, 199);
                case ETemplateName.FishHoldInactive:
                    return CalculateTemplateRect(windowImg, 68, 364, 178, 177);
                case ETemplateName.FishHoldQuickSellButton:
                    return CalculateTemplateRect(windowImg, 964, 969, 201, 73);
            }

            throw new Exception($"【GetNormalRatioRect】无法匹配当前 16/9 分辨率下的模板矩形: {templateName}");
        }

        /// <summary>
        /// 分辨率24/10
        /// </summary>
        private static Rectangle GetLongRatioRect(Bitmap windowImg, ETemplateName templateName)
        {
            const double ratioWidth = 1792.0;
            const double ratioHeight = 768.0;

            switch (templateName)
            {
                case ETemplateName.FishingPoint:
                    return CalculateTemplateRect(windowImg, 640, 82, 522, 17, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldActive:
                    return CalculateTemplateRect(windowImg, 221, 265, 192, 138, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldInactive:
                    return CalculateTemplateRect(windowImg, 246, 263, 159, 139, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldQuickSellButton:
                    return CalculateTemplateRect(windowImg, 884, 691, 173, 67, ratioWidth, ratioHeight);
            }

            throw new Exception($"【GetLongRatioRect】无法匹配当前 24/10 分辨率下的模板矩形: {templateName}");
        }

        private static Rectangle MatchRatioRectangle(Bitmap windowImg, ETemplateName templateName)
        {
            switch (GetSimilarityRatio())
            {
                case 1.6:
                    switch (templateName)
                    {
                        case ETemplateName.FishingPoint:
                            return CalculateTemplateRect(windowImg, 597, 102, 738, 22);
                    }

                    break;
                case 1.7:
                    return GetNormalRatioRect(windowImg, templateName);
                case 2.4:
                    return GetLongRatioRect(windowImg, templateName);
                case 3.5:
                    switch (templateName)
                    {
                        case ETemplateName.FishingPoint:
                            return CalculateTemplateRect(windowImg, 580, 94, 766, 56);
                    }
                    break;
            }

            throw new Exception("【MatchRatioRectangle】无法匹配当前分辨率");
        }

        public static Rectangle GetTemplateRect(Bitmap windowImg, ETemplateName templateName)
        {
            switch (templateName)
            {
                case ETemplateName.LoginPageAnnouncement:
                case ETemplateName.LoginPageAnnouncementLight:
                    return CalculateTemplateRect(windowImg, 1826, 140, 85, 85);
                case ETemplateName.EnterFKeyToFishing:
                    return CalculateTemplateRect(windowImg, 1100, 590, 236, 75);
                case ETemplateName.EnterAKeyToLeft:
                    return CalculateTemplateRect(windowImg, 1761, 577, 161, 49);
                case ETemplateName.FishingSceneFKey:
                    return CalculateTemplateRect(windowImg, 1728, 1015, 100, 100);
                case ETemplateName.StartSceneShopIcon:
                    return CalculateTemplateRect(windowImg, 1226, 928, 165, 100);
                case ETemplateName.StartSceneStartButton:
                    return CalculateTemplateRect(windowImg, 1359, 931, 522, 100);
                case ETemplateName.StartSceneSelectBait:
                    return CalculateTemplateRect(windowImg, 1597, 696, 220, 227);
                case ETemplateName.UniversalBait:
                    return CalculateTemplateRect(windowImg, 27, 150, 667, 745);
                case ETemplateName.Maximum:
                    return CalculateTemplateRect(windowImg, 1779, 944, 100, 100);
                case ETemplateName.ShopScenePurchaseButton:
                    return CalculateTemplateRect(windowImg, 1343, 1015, 536, 104);
                case ETemplateName.ConfirmDialogButton:
                    return CalculateTemplateRect(windowImg, 859, 695, 434, 100);
                case ETemplateName.ConfirmDialogConfirmButton:
                    return CalculateTemplateRect(windowImg, 969, 695, 404, 100);
                case ETemplateName.CloseIcon:
                    return CalculateTemplateRect(windowImg, 1747, 38, 175, 175);
                case ETemplateName.SellFishShellCoin:
                    return CalculateTemplateRect(windowImg, 692, 848, 542, 110);
                case ETemplateName.ClickEmptyAreaToClose:
                    return CalculateTemplateRect(windowImg, 803, 955, 333, 84);
                case ETemplateName.TakesTheBait:
                    return CalculateTemplateRect(windowImg, 454, 217, 327, 150);
                case ETemplateName.FishWeightGram:
                    return CalculateTemplateRect(windowImg, 867, 711, 389, 150);
                case ETemplateName.ConfirmTips:
                    return CalculateTemplateRect(windowImg, 825, 282, 272, 241);
                case ETemplateName.CenterTips:
                    return CalculateTemplateRect(windowImg, 907, 565, 110, 28);
                case ETemplateName.MoonCard:
                    return CalculateTemplateRect(windowImg, 770, 339, 400, 400);
                case ETemplateName.FishHoldEmpty:
                    return CalculateTemplateRect(windowImg, 1315, 475, 416, 290);

                case ETemplateName.FishingPoint:
                case ETemplateName.FishHoldActive:
                case ETemplateName.FishHoldInactive:
                case ETemplateName.FishHoldQuickSellButton:
                    return MatchRatioRectangle(windowImg, templateName);
                //case ETemplateName.FishHoldActive:
                //    return CalculateTemplateRect(windowImg, 27, 351, 238, 199);
                //case ETemplateName.FishHoldInactive:
                //    return CalculateTemplateRect(windowImg, 68, 364, 178, 177);
                //case ETemplateName.FishHoldQuickSellButton:
                //    return CalculateTemplateRect(windowImg, 964, 969, 201, 73);
            }

            throw new Exception($"【GetTemplateRect】未找到模板: {templateName}");
        }

        /// <summary>
        /// 传入游戏窗口截图，程序句柄和需要比对的图片名
        /// 返回匹配结果，能够在窗口截图中找到比对图则返回比对图在整个桌面全屏模式下的绝对位置，否则返回null
        /// </summary>
        /// <param name="windowImg">游戏窗口截图</param>
        /// <param name="intPtrGame">程序句柄</param>
        /// <param name="tmplName">模板图名称</param>
        /// <param name="minSimilarity">最小相似度</param>
        /// <returns>System.Drawing.Point?</returns>
        /// <exception cref="Exception"></exception>
        public static Point? MathTemplateImgByName(Bitmap windowImg, IntPtr intPtrGame, ETemplateName tmplName, double minSimilarity = 0.8)
        {
            string imgName = tmplName.ToString();

            if (!TemplateMatCache.ContainsKey(imgName))
            {
                TemplateMatCache[imgName] = GetTemplateImage(imgName).ToMat();
            }

            // 裁剪区域，以减少干扰和避免增加性能消耗
            Rectangle rect = GetTemplateRect(windowImg, tmplName);

            using (Bitmap cropImg = CropImageByRect(windowImg, rect))
            {
                Point? loc = FindImageLocation(cropImg, TemplateMatCache[imgName], minSimilarity);

                if (loc != null)
                {
                    if (!GetPureClientRect(intPtrGame, out RECT windowRect)) return null;

                    // 将相对坐标转换为绝对坐标
                    int absoluteX = windowRect.Left + rect.X + loc.Value.X;
                    int absoluteY = windowRect.Top + rect.Y + loc.Value.Y;

                    return new Point(absoluteX, absoluteY);
                }
            }

            return null;
        }

        public static int GetRandomNumber(int num)
        {
            Random random = new Random();
            return random.Next(0, num + 1);
        }

        public static Point GetRectangleCenterPoint(IntPtr windowHandle, ETemplateName tmplName)
        {
            Rectangle rect = GetTemplateRect(CaptureWindow(windowHandle), tmplName);
            GetPureClientRect(windowHandle, out RECT clientRect);

            return new Point(clientRect.Left + rect.X + rect.Width / 2, clientRect.Top + rect.Y + rect.Height / 2);
        }

        public static Point GetRectangleRandomPoint(IntPtr windowHandle, ETemplateName tmplName)
        {
            Rectangle rect = GetTemplateRect(CaptureWindow(windowHandle), tmplName);
            GetPureClientRect(windowHandle, out RECT clientRect);

            int x = clientRect.Left + rect.X + GetRandomNumber(rect.Width);
            int y = clientRect.Top + rect.Y + GetRandomNumber(rect.Height);

            return new Point(x, y);
        }

        private static void LoadTemplateImage(string path)
        {
            string imageName = Path.GetFileNameWithoutExtension(path);

            // 使用文件流读取，避免使用 new Bitmap(path) 可能导致文件锁定问题
            using (Stream stream = File.OpenRead(path))
            {
                Bitmap bmp = new Bitmap(stream);
                TemplateImagesCache[imageName] = bmp;
            }
        }

        public static void InitializeImages(IntPtr windowHandle)
        {
            if (!Directory.Exists(TEMPLATE_PATH))
            {
                throw new DirectoryNotFoundException($"【InitializeImages】未找到模板目录: {TEMPLATE_PATH}");
            }

            ClearCache();

            GetPureClientRect(windowHandle, out RECT rect);
            _resolutionLevel = GetResolutionLevel(rect.Bottom - rect.Top);

            if (_resolutionLevel == 0)
            {
                throw new Exception("不支持的分辨率");
            }

            string searchPattern = $"*-{_resolutionLevel}.png";
            // 获取目录下所有PNG图片文件路径
            string[] imagesFilePaths = Directory.GetFiles(TEMPLATE_PATH, searchPattern);

            foreach (var path in imagesFilePaths)
            {
                try
                {
                    LoadTemplateImage(path);
                }
                catch (Exception ex)
                {
                    throw new Exception($"【InitializeImages】加载模板图片失败: {path}. \n错误信息: {ex.Message}");
                }
            }
        }

        public static Bitmap GetTemplateImage(string imageName)
        {
            string key = $"HTGame-{imageName}-{_resolutionLevel}";

            if (TemplateImagesCache.TryGetValue(key, out Bitmap image))
            {
                return image;
            }

            string path = Path.Combine(TEMPLATE_PATH, $"{key}.png");
            if (File.Exists(path))
            {
                try
                {
                    LoadTemplateImage(path);
                    return TemplateImagesCache[imageName];
                }
                catch (Exception ex)
                {
                    throw new Exception($"【GetTemplateImage】加载模板图片失败: {path}. \n错误信息: {ex.Message}");
                }
            }

            throw new FileNotFoundException($"【GetTemplateImage】未找到模板图片: {imageName}");
        }

        public static void ClearCache()
        {
            foreach (Bitmap image in TemplateImagesCache.Values)
            {
                image?.Dispose();
            }
            TemplateImagesCache.Clear();
        }
    }
}
