using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using NLog;
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
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string TEMPLATE_PATH = "./Resources/Images/";
        private static int _resolutionLevel;
        public static RECT _curWindowRect;

        // 以下是游戏内的几种分辨率。
        // 2560/1600 (1.6)
        // 1920/1080 (1.7)
        // 1792/768  (2.4)
        // 3840/1080 (3.5)
        private static double _curAspectRatio = 0;

        public static void InitRatio(RECT windowRect)
        {
            _curWindowRect = windowRect;

            double width = windowRect.Right - windowRect.Left;
            double height = windowRect.Bottom - windowRect.Top;
            _curAspectRatio = width / height;

            Log.Info("【InitRatio】游戏当前分辨率：宽 = {0}, 高 = {1}", width, height);

            double ratio = GetSimilarityRatio();
            if (ratio == 0)
            {
                string msg = $"【InitRatio】游戏当前分辨率不符合要求\n请选择符合[16:9] [16:10] [24:10] [35:10]宽高比的分辨率";
                Log.Error($"【InitRatio】游戏当前分辨率不符合要求");
                throw new Exception(msg);
            }
        }

        public static double GetSimilarityRatio()
        {
            double result = Math.Abs(_curAspectRatio - 1.6);
            if (result <= 0.1) return 1.6;

            result = Math.Abs(_curAspectRatio - 1.7);
            if (result <= 0.1) return 1.7;

            result = Math.Abs(_curAspectRatio - 2.4);
            if (result <= 0.1) return 2.4;

            result = Math.Abs(_curAspectRatio - 3.5);
            if (result <= 0.1) return 3.5;

            return 0;
        }

        private static Rectangle CalculateTemplateRect(double x, double y, double width, double height, double ratioWidth = 1920.0, double ratioHeight = 1080.0)
        {
            y -= 39; // 适配标题栏高度。

            int windowWidth = _curWindowRect.Right - _curWindowRect.Left;
            int windowHeight = _curWindowRect.Bottom - _curWindowRect.Top;
            int rectX = (int)(windowWidth * (x / ratioWidth));
            int rectY = (int)(windowHeight * (y / ratioHeight));
            int rectWidth = (int)(windowWidth * (width / ratioWidth));
            int rectHeight = (int)(windowHeight * (height / ratioHeight));

            return new Rectangle(rectX, rectY, rectWidth, rectHeight);
        }

        /// <summary>
        /// 分辨率16/9
        /// </summary>
        private static Rectangle GetNormalRatioRect(ETemplateName templateName)
        {
            switch (templateName)
            {
                case ETemplateName.FishingPoint:
                    return CalculateTemplateRect(597, 102, 738, 22);
                case ETemplateName.FishHoldActive:
                    return CalculateTemplateRect(27, 351, 238, 199);
                case ETemplateName.FishHoldInactive:
                    return CalculateTemplateRect(68, 364, 178, 177);
                case ETemplateName.FishHoldQuickSellButton:
                    return CalculateTemplateRect(964, 969, 201, 73);
                case ETemplateName.EnterFKeyToFishing:
                    return CalculateTemplateRect(1100, 590, 236, 75);
                case ETemplateName.StartSceneShopIcon:
                    return CalculateTemplateRect(1226, 928, 165, 100);
                case ETemplateName.StartSceneStartButton:
                    return CalculateTemplateRect(1359, 931, 522, 100);
                case ETemplateName.StartSceneSelectBait:
                    return CalculateTemplateRect(1597, 696, 220, 227);
                case ETemplateName.ConfirmDialogButton:
                    return CalculateTemplateRect(859, 695, 434, 100);
                case ETemplateName.ConfirmDialogConfirmButton:
                    return CalculateTemplateRect(969, 695, 404, 100);
                case ETemplateName.UniversalBait:
                    return CalculateTemplateRect(27, 150, 667, 745);
                case ETemplateName.Maximum:
                    return CalculateTemplateRect(1779, 944, 100, 100);
                case ETemplateName.ShopScenePurchaseButton:
                    return CalculateTemplateRect(1343, 1015, 536, 104);
                case ETemplateName.CloseIcon:
                    return CalculateTemplateRect(1747, 39, 175, 175);
                case ETemplateName.ClickEmptyAreaToClose:
                    return CalculateTemplateRect(803, 955, 333, 84);
                case ETemplateName.ConfirmTips:
                    return CalculateTemplateRect(825, 282, 272, 241);
                case ETemplateName.EnterAKeyToLeft:
                    return CalculateTemplateRect(1761, 577, 161, 49);
                case ETemplateName.FishingSceneFKey:
                    return CalculateTemplateRect(1728, 1015, 100, 100);
                case ETemplateName.TakesTheBait:
                    return CalculateTemplateRect(454, 217, 327, 150);
                case ETemplateName.FishWeightGram:
                    return CalculateTemplateRect(867, 711, 389, 150);
                case ETemplateName.FishHoldEmpty:
                    return CalculateTemplateRect(1315, 475, 416, 290);
                case ETemplateName.SellFishShellCoin:
                    return CalculateTemplateRect(692, 848, 542, 110);
                case ETemplateName.CenterTips:
                    return CalculateTemplateRect(907, 565, 110, 28);
            }

            throw new Exception($"【GetNormalRatioRect】无法匹配当前 16/9 分辨率下的模板矩形: {templateName}");
        }

        /// <summary>
        /// 分辨率16/10
        /// </summary>
        private static Rectangle GetShortRatioRect(ETemplateName templateName)
        {
            const double ratioWidth = 2560.0;
            const double ratioHeight = 1600.0;

            switch (templateName)
            {
                case ETemplateName.FishingPoint:
                    return CalculateTemplateRect(793, 122, 986, 31, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldActive:
                    return CalculateTemplateRect(49, 566, 259, 211, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldInactive:
                    return CalculateTemplateRect(81, 560, 263, 220, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldQuickSellButton:
                    return CalculateTemplateRect(1276, 1364, 283, 88, ratioWidth, ratioHeight);
                case ETemplateName.EnterFKeyToFishing:
                    return CalculateTemplateRect(1485, 833, 225, 129, ratioWidth, ratioHeight);
                case ETemplateName.StartSceneShopIcon:
                    return CalculateTemplateRect(1641, 1295, 194, 158, ratioWidth, ratioHeight);
                case ETemplateName.StartSceneStartButton:
                    return CalculateTemplateRect(1807, 1304, 698, 137, ratioWidth, ratioHeight);
                case ETemplateName.StartSceneSelectBait:
                    return CalculateTemplateRect(2140, 1013, 285, 268, ratioWidth, ratioHeight);
                case ETemplateName.ConfirmDialogButton:
                    return CalculateTemplateRect(1205, 1001, 295, 127, ratioWidth, ratioHeight);
                case ETemplateName.ConfirmDialogConfirmButton:
                    return CalculateTemplateRect(1305, 1002, 507, 119, ratioWidth, ratioHeight);
                case ETemplateName.UniversalBait:
                    return CalculateTemplateRect(38, 159, 916, 1060, ratioWidth, ratioHeight);
                case ETemplateName.Maximum:
                    return CalculateTemplateRect(2381, 1320, 127, 134, ratioWidth, ratioHeight);
                case ETemplateName.ShopScenePurchaseButton:
                    return CalculateTemplateRect(1788, 1428, 723, 122, ratioWidth, ratioHeight);
                case ETemplateName.CloseIcon:
                    return CalculateTemplateRect(2294, 39, 267, 218, ratioWidth, ratioHeight);
                case ETemplateName.ClickEmptyAreaToClose:
                    return CalculateTemplateRect(1017, 1396, 541, 146, ratioWidth, ratioHeight);
                case ETemplateName.ConfirmTips:
                    return CalculateTemplateRect(1121, 472, 322, 250, ratioWidth, ratioHeight);
                case ETemplateName.EnterAKeyToLeft:
                    return CalculateTemplateRect(2430, 907, 132, 85, ratioWidth, ratioHeight);
                case ETemplateName.FishingSceneFKey:
                    return CalculateTemplateRect(2305, 1516, 130, 111, ratioWidth, ratioHeight);
                case ETemplateName.TakesTheBait:
                    return CalculateTemplateRect(656, 292, 219, 170, ratioWidth, ratioHeight);
                case ETemplateName.FishWeightGram:
                    return CalculateTemplateRect(1154, 1042, 493, 193, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldEmpty:
                    return CalculateTemplateRect(1720, 634, 661, 594, ratioWidth, ratioHeight);
                case ETemplateName.SellFishShellCoin:
                    return CalculateTemplateRect(906, 1183, 749, 154, ratioWidth, ratioHeight);
                case ETemplateName.CenterTips:
                    return CalculateTemplateRect(1150, 814, 244, 43, ratioWidth, ratioHeight);
            }

            throw new Exception($"【GetShortRatioRect】无法匹配当前 16/10 分辨率下的模板矩形: {templateName}");
        }

        /// <summary>
        /// 分辨率24/10
        /// </summary>
        private static Rectangle GetLongRatioRect(ETemplateName templateName)
        {
            const double ratioWidth = 1792.0;
            const double ratioHeight = 768.0;

            switch (templateName)
            {
                case ETemplateName.FishingPoint:
                    return CalculateTemplateRect(640, 81, 522, 20, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldActive:
                    return CalculateTemplateRect(241, 276, 141, 112, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldInactive:
                    return CalculateTemplateRect(257, 275, 133, 115, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldQuickSellButton:
                    return CalculateTemplateRect(900, 701, 142, 49, ratioWidth, ratioHeight);
                case ETemplateName.EnterFKeyToFishing:
                    return CalculateTemplateRect(999, 432, 131, 50, ratioWidth, ratioHeight);
                case ETemplateName.StartSceneShopIcon:
                    return CalculateTemplateRect(1317, 664, 79, 79, ratioWidth, ratioHeight);
                case ETemplateName.StartSceneStartButton:
                    return CalculateTemplateRect(1392, 673, 370, 68, ratioWidth, ratioHeight);
                case ETemplateName.StartSceneSelectBait:
                    return CalculateTemplateRect(1577, 515, 139, 139, ratioWidth, ratioHeight);
                case ETemplateName.ConfirmDialogButton:
                    return CalculateTemplateRect(860, 506, 145, 68, ratioWidth, ratioHeight);
                case ETemplateName.ConfirmDialogConfirmButton:
                    return CalculateTemplateRect(911, 507, 273, 67, ratioWidth, ratioHeight);
                case ETemplateName.UniversalBait:
                    return CalculateTemplateRect(16, 109, 486, 565, ratioWidth, ratioHeight);
                case ETemplateName.Maximum:
                    return CalculateTemplateRect(1698, 682, 60, 66, ratioWidth, ratioHeight);
                case ETemplateName.ShopScenePurchaseButton:
                    return CalculateTemplateRect(1381, 738, 385, 62, ratioWidth, ratioHeight);
                case ETemplateName.CloseIcon:
                    return CalculateTemplateRect(1673, 39, 119, 98, ratioWidth, ratioHeight);
                case ETemplateName.ClickEmptyAreaToClose:
                    return CalculateTemplateRect(753, 686, 287, 64, ratioWidth, ratioHeight);
                case ETemplateName.ConfirmTips:
                    return CalculateTemplateRect(801, 223, 194, 139, ratioWidth, ratioHeight);
                case ETemplateName.EnterAKeyToLeft:
                    return CalculateTemplateRect(1735, 418, 59, 44, ratioWidth, ratioHeight);
                case ETemplateName.FishingSceneFKey:
                    return CalculateTemplateRect(1684, 741, 86, 57, ratioWidth, ratioHeight);
                case ETemplateName.TakesTheBait:
                    return CalculateTemplateRect(567, 178, 106, 84, ratioWidth, ratioHeight);
                case ETemplateName.FishWeightGram:
                    return CalculateTemplateRect(839, 535, 250, 90, ratioWidth, ratioHeight);
                case ETemplateName.FishHoldEmpty:
                    return CalculateTemplateRect(1133, 340, 351, 292, ratioWidth, ratioHeight);
                case ETemplateName.SellFishShellCoin:
                    return CalculateTemplateRect(698, 607, 398, 85, ratioWidth, ratioHeight);
                case ETemplateName.CenterTips:
                    return CalculateTemplateRect(842, 411, 110, 22, ratioWidth, ratioHeight);
            }

            throw new Exception($"【GetLongRatioRect】无法匹配当前 24/10 分辨率下的模板矩形: {templateName}");
        }

        /// <summary>
        /// 分辨率35/10
        /// </summary>
        private static Rectangle GetUltraRatioRect(ETemplateName templateName)
        {
            const double ratioWidth = 3840.0;
            //const double ratioHeight = 1080.0;

            switch (templateName)
            {
                case ETemplateName.FishingPoint:
                    return CalculateTemplateRect(1552, 99, 740, 24, ratioWidth);
                case ETemplateName.FishHoldActive:
                    return CalculateTemplateRect(974, 371, 235, 162, ratioWidth);
                case ETemplateName.FishHoldInactive:
                    return CalculateTemplateRect(996, 375, 231, 159, ratioWidth);
                case ETemplateName.FishHoldQuickSellButton:
                    return CalculateTemplateRect(1913, 969, 221, 72, ratioWidth);
                case ETemplateName.FishHoldEmpty:
                    return CalculateTemplateRect(2245, 415, 496, 441, ratioWidth);
                case ETemplateName.EnterFKeyToFishing:
                    return CalculateTemplateRect(2028, 582, 257, 85, ratioWidth);
                case ETemplateName.StartSceneShopIcon:
                    return CalculateTemplateRect(3167, 919, 113, 113, ratioWidth);
                case ETemplateName.StartSceneStartButton:
                    return CalculateTemplateRect(3274, 929, 524, 103, ratioWidth);
                case ETemplateName.StartSceneSelectBait:
                    return CalculateTemplateRect(3519, 711, 219, 213, ratioWidth);
                case ETemplateName.ConfirmDialogButton:
                    return CalculateTemplateRect(1851, 699, 235, 91, ratioWidth);
                case ETemplateName.ConfirmDialogConfirmButton:
                    return CalculateTemplateRect(1937, 701, 386, 91, ratioWidth);
                case ETemplateName.UniversalBait:
                    return CalculateTemplateRect(0, 142, 720, 789, ratioWidth);
                case ETemplateName.Maximum:
                    return CalculateTemplateRect(3693, 949, 106, 83, ratioWidth);
                case ETemplateName.ShopScenePurchaseButton:
                    return CalculateTemplateRect(3261, 1024, 538, 84, ratioWidth);
                case ETemplateName.CloseIcon:
                    return CalculateTemplateRect(3649, 39, 191, 149, ratioWidth);
                case ETemplateName.ClickEmptyAreaToClose:
                    return CalculateTemplateRect(1594, 935, 613, 108, ratioWidth);
                case ETemplateName.ConfirmTips:
                    return CalculateTemplateRect(1807, 304, 236, 190, ratioWidth);
                case ETemplateName.EnterAKeyToLeft:
                    return CalculateTemplateRect(3715, 575, 125, 50, ratioWidth);
                case ETemplateName.FishingSceneFKey:
                    return CalculateTemplateRect(3628, 1005, 141, 106, ratioWidth);
                case ETemplateName.TakesTheBait:
                    return CalculateTemplateRect(1423, 236, 229, 113, ratioWidth);
                case ETemplateName.FishWeightGram:
                    return CalculateTemplateRect(1781, 733, 438, 125, ratioWidth);
                case ETemplateName.SellFishShellCoin:
                    return CalculateTemplateRect(1609, 832, 638, 125, ratioWidth);
                case ETemplateName.CenterTips:
                    return CalculateTemplateRect(1826, 558, 171, 39, ratioWidth);
            }

            throw new Exception($"【GetUltraRatioRect】无法匹配当前 35/10 分辨率下的模板矩形: {templateName}");
        }

        private static Rectangle MatchRatioRectangle(ETemplateName templateName)
        {
            switch (GetSimilarityRatio())
            {
                case 1.6:
                    return GetShortRatioRect(templateName);
                case 1.7:
                    return GetNormalRatioRect(templateName);
                case 2.4:
                    return GetLongRatioRect(templateName);
                case 3.5:
                    return GetUltraRatioRect(templateName);
            }

            throw new Exception("【MatchRatioRectangle】无法匹配当前分辨率\n请检查是否符合[16:9] [16:10] [24:10] [35:10]的分辨率");
        }

        public static Rectangle GetTemplateRect(ETemplateName templateName)
        {
            switch (templateName)
            {
                case ETemplateName.LoginPageAnnouncement:
                case ETemplateName.LoginPageAnnouncementLight:
                    return CalculateTemplateRect(1826, 140, 85, 85);
                case ETemplateName.MoonCard:
                    return CalculateTemplateRect(750, 260, 400, 650);

                default:
                    return MatchRatioRectangle(templateName);
            }
        }

        public static RECT GetTemplateRECT(ETemplateName templateName)
        {
            Rectangle rect;

            switch (templateName)
            {
                case ETemplateName.LoginPageAnnouncement:
                case ETemplateName.LoginPageAnnouncementLight:
                    rect = CalculateTemplateRect(1826, 140, 85, 85);
                    break;
                case ETemplateName.MoonCard:
                    rect = CalculateTemplateRect(750, 260, 400, 650);
                    break;
                default:
                    rect = MatchRatioRectangle(templateName);
                    break;
            }

            return new RECT
            {
                Left = _curWindowRect.Left + rect.X,
                Top = _curWindowRect.Top + rect.Y,
                Right = _curWindowRect.Left + rect.X + rect.Width,
                Bottom = _curWindowRect.Top + rect.Y + rect.Height
            };
        }

        private static Dictionary<string, Mat> _templateMatCache = null;

        /// <summary>
        /// 传入游戏窗口截图，程序句柄和需要比对的图片名
        /// 返回匹配结果，能够在窗口截图中找到比对图则返回比对图在整个桌面全屏模式下的绝对位置，否则返回null
        /// </summary>
        /// <param name="intPtrGame">程序句柄</param>
        /// <param name="tmplName">模板图名称</param>
        /// <param name="sourceImg">游戏截图</param>
        /// <returns>System.Drawing.Point?</returns>
        /// <exception cref="Exception"></exception>
        public static Point? MathTemplateImgByName(IntPtr intPtrGame, ETemplateName tmplName, Bitmap sourceImg = null)
        {
            string imgName = tmplName.ToString();
            double minSimilarity = 0.8;

            // 预防Mat未能加载。
            if (_templateMatCache == null)
            {
                _templateMatCache = new Dictionary<string, Mat>();
            }

            if (!_templateMatCache.ContainsKey(imgName))
            {
                _templateMatCache[imgName] = GetTemplateImage(imgName).ToMat();
            }

            using (Bitmap image = sourceImg ?? CaptureWindow(intPtrGame, tmplName))
            {
                Rectangle rect = GetTemplateRect(tmplName);
                Point? loc = FindImageLocation(image, _templateMatCache[imgName], minSimilarity);

                if (loc != null)
                {
                    RECT windowRect = _curWindowRect;

                    // 将相对坐标转换为绝对坐标
                    int absoluteX = windowRect.Left + rect.X + loc.Value.X;
                    int absoluteY = windowRect.Top + rect.Y + loc.Value.Y;

                    return new Point(absoluteX, absoluteY);
                }
            }

            // 裁剪区域，以减少干扰和避免增加性能消耗
            //using (Bitmap cropImg = CropImageByRect(windowImg, rect))
            //{
            //    Point? loc = FindImageLocation(cropImg, _templateMatCache[imgName], minSimilarity);

            //    if (loc != null)
            //    {
            //        if (!GetPureClientRect(intPtrGame, out RECT windowRect)) return null;

            //        // 将相对坐标转换为绝对坐标
            //        int absoluteX = windowRect.Left + rect.X + loc.Value.X;
            //        int absoluteY = windowRect.Top + rect.Y + loc.Value.Y;

            //        return new Point(absoluteX, absoluteY);
            //    }
            //}

            return null;
        }

        public static int GetRandomNumber(int num)
        {
            Random random = new Random();
            return random.Next(0, num + 1);
        }

        public static Point GetRectangleCenterPoint(IntPtr windowHandle, ETemplateName tmplName)
        {
            Rectangle rect = GetTemplateRect(tmplName);
            //GetPureClientRect(windowHandle, out RECT clientRect);
            RECT clientRect = _curWindowRect;

            return new Point(clientRect.Left + rect.X + rect.Width / 2, clientRect.Top + rect.Y + rect.Height / 2);
        }

        public static Point GetRectangleRandomPoint(IntPtr windowHandle, ETemplateName tmplName)
        {
            Rectangle rect = GetTemplateRect(tmplName);
            //GetPureClientRect(windowHandle, out RECT clientRect);
            RECT clientRect = _curWindowRect;

            int x = clientRect.Left + rect.X + GetRandomNumber(rect.Width);
            int y = clientRect.Top + rect.Y + GetRandomNumber(rect.Height);

            return new Point(x, y);
        }

        private static readonly Dictionary<string, Bitmap> TemplateImagesCache = new Dictionary<string, Bitmap>();

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

            //GetPureClientRect(windowHandle, out RECT rect);
            RECT rect = _curWindowRect;
            _resolutionLevel = GetResolutionLevel(rect.Bottom - rect.Top);

            string searchPattern = $"*-{_resolutionLevel}.png";
            // 获取目录下所有符合条件的文件路径
            string[] imagesFilePaths = Directory.GetFiles(TEMPLATE_PATH, searchPattern);

            Log.Info("【InitializeImages】开始初始化模板图片...");
            Log.Info($"【InitializeImages】{ string.Join(", ", imagesFilePaths) }");
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

            Log.Info("【InitializeImages】模板图片初始化完成");
        }

        public static Bitmap GetTemplateImage(string imageName, int resolutionLevel = 0)
        {
            if (resolutionLevel == 0)
            {
                resolutionLevel = _resolutionLevel;
            }

            string key = $"HTGame-{imageName}-{resolutionLevel}";

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
            Log.Info("【ClearCache】模板图片缓存已清空");

            if (_templateMatCache != null)
            {
                foreach (Mat mat in _templateMatCache.Values)
                {
                    mat?.Dispose();
                }
                _templateMatCache.Clear();
                Log.Info("【ClearCache】模板Mat缓存已清空");
            }
        }
    }
}
