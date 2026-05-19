using System.Drawing;

namespace NTEFishingTool.FishingTool
{
    enum EGameImage
    {
        BaitEmpty,
        BaitUniversal,
        Buy,
        BuyConfirm,
        Change,
        ChangeBaitUniversal,
        ClickToClose,
        ClickToFishing,
        ClickToStart,
        Confirm,
        EnterToFishing,
        FishFull,
        FishingFail,
        FishSale,
        FishStorage,
        FishStorageIcon,
        MoonCard,
        PageClose,
        SelectBait,
        ShopingMax,
        ToBuy,
    }

    internal class ImageManager
    {
        private const string IMAGE_PATH = ".\\Resources\\Images\\";

        private const string IMAGE_BAITEMPTY = "HTGame-BaitEmpty";
        private static readonly Bitmap img720_BaitEmpty = new Bitmap($"{IMAGE_PATH}{IMAGE_BAITEMPTY}-720.png");
        private static readonly Bitmap img1080_BaitEmpty = new Bitmap($"{IMAGE_PATH}{IMAGE_BAITEMPTY}-1080.png");
        private static readonly Bitmap img1440_BaitEmpty = new Bitmap($"{IMAGE_PATH}{IMAGE_BAITEMPTY}-1440.png");

        private const string IMAGE_BAITUNIVERSAL = "HTGame-BaitUniversal";
        private static readonly Bitmap img720_BaitUniversal = new Bitmap($"{IMAGE_PATH}{IMAGE_BAITUNIVERSAL}-720.png");
        private static readonly Bitmap img1080_BaitUniversal = new Bitmap($"{IMAGE_PATH}{IMAGE_BAITUNIVERSAL}-1080.png");
        private static readonly Bitmap img1440_BaitUniversal = new Bitmap($"{IMAGE_PATH}{IMAGE_BAITUNIVERSAL}-1440.png");

        private const string IMAGE_BUY = "HTGame-Buy";
        private static readonly Bitmap img720_Buy = new Bitmap($"{IMAGE_PATH}{IMAGE_BUY}-720.png");
        private static readonly Bitmap img1080_Buy = new Bitmap($"{IMAGE_PATH}{IMAGE_BUY}-1080.png");
        private static readonly Bitmap img1440_Buy = new Bitmap($"{IMAGE_PATH}{IMAGE_BUY}-1440.png");

        private const string IMAGE_BUYCONFIRM = "HTGame-BuyConfirm";
        private static readonly Bitmap img720_BuyConfirm = new Bitmap($"{IMAGE_PATH}{IMAGE_BUYCONFIRM}-720.png");
        private static readonly Bitmap img1080_BuyConfirm = new Bitmap($"{IMAGE_PATH}{IMAGE_BUYCONFIRM}-1080.png");
        private static readonly Bitmap img1440_BuyConfirm = new Bitmap($"{IMAGE_PATH}{IMAGE_BUYCONFIRM}-1440.png");

        private const string IMAGE_CHANGE = "HTGame-Change";
        private static readonly Bitmap img720_Change = new Bitmap($"{IMAGE_PATH}{IMAGE_CHANGE}-720.png");
        private static readonly Bitmap img1080_Change = new Bitmap($"{IMAGE_PATH}{IMAGE_CHANGE}-1080.png");
        private static readonly Bitmap img1440_Change = new Bitmap($"{IMAGE_PATH}{IMAGE_CHANGE}-1440.png");

        private const string IMAGE_CHANGEBAITUNIVERSAL = "HTGame-ChangeBaitUniversal";
        private static readonly Bitmap img720_ChangeBaitUniversal = new Bitmap($"{IMAGE_PATH}{IMAGE_CHANGEBAITUNIVERSAL}-720.png");
        private static readonly Bitmap img1080_ChangeBaitUniversal = new Bitmap($"{IMAGE_PATH}{IMAGE_CHANGEBAITUNIVERSAL}-1080.png");
        private static readonly Bitmap img1440_ChangeBaitUniversal = new Bitmap($"{IMAGE_PATH}{IMAGE_CHANGEBAITUNIVERSAL}-1440.png");

        private const string IMAGE_CLICKTOCLOSE = "HTGame-ClickToClose";
        private static readonly Bitmap img720_ClickToClose = new Bitmap($"{IMAGE_PATH}{IMAGE_CLICKTOCLOSE}-720.png");
        private static readonly Bitmap img1080_ClickToClose = new Bitmap($"{IMAGE_PATH}{IMAGE_CLICKTOCLOSE}-1080.png");
        private static readonly Bitmap img1440_ClickToClose = new Bitmap($"{IMAGE_PATH}{IMAGE_CLICKTOCLOSE}-1440.png");

        private const string IMAGE_CLICKTOFISHING = "HTGame-ClickToFishing";
        private static readonly Bitmap img720_ClickToFishing = new Bitmap($"{IMAGE_PATH}{IMAGE_CLICKTOFISHING}-720.png");
        private static readonly Bitmap img1080_ClickToFishing = new Bitmap($"{IMAGE_PATH}{IMAGE_CLICKTOFISHING}-1080.png");
        private static readonly Bitmap img1440_ClickToFishing = new Bitmap($"{IMAGE_PATH}{IMAGE_CLICKTOFISHING}-1440.png");

        private const string IMAGE_CLICKTOSTART = "HTGame-ClickToStart";
        private static readonly Bitmap img720_ClickToStart = new Bitmap($"{IMAGE_PATH}{IMAGE_CLICKTOSTART}-720.png");
        private static readonly Bitmap img1080_ClickToStart = new Bitmap($"{IMAGE_PATH}{IMAGE_CLICKTOSTART}-1080.png");
        private static readonly Bitmap img1440_ClickToStart = new Bitmap($"{IMAGE_PATH}{IMAGE_CLICKTOSTART}-1440.png");

        private const string IMAGE_CONFIRM = "HTGame-Confirm";
        private static readonly Bitmap img720_Confirm = new Bitmap($"{IMAGE_PATH}{IMAGE_CONFIRM}-720.png");
        private static readonly Bitmap img1080_Confirm = new Bitmap($"{IMAGE_PATH}{IMAGE_CONFIRM}-1080.png");
        private static readonly Bitmap img1440_Confirm = new Bitmap($"{IMAGE_PATH}{IMAGE_CONFIRM}-1440.png");

        private const string IMAGE_ENTERTOFISHING = "HTGame-EnterToFishing";
        private static readonly Bitmap img720_EnterToFishing = new Bitmap($"{IMAGE_PATH}{IMAGE_ENTERTOFISHING}-720.png");
        private static readonly Bitmap img1080_EnterToFishing = new Bitmap($"{IMAGE_PATH}{IMAGE_ENTERTOFISHING}-1080.png");
        private static readonly Bitmap img1440_EnterToFishing = new Bitmap($"{IMAGE_PATH}{IMAGE_ENTERTOFISHING}-1440.png");

        private const string IMAGE_FISHFULL = "HTGame-FishFull";
        private static readonly Bitmap img720_FishFull = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHFULL}-720.png");
        private static readonly Bitmap img1080_FishFull = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHFULL}-1080.png");
        private static readonly Bitmap img1440_FishFull = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHFULL}-1440.png");

        private const string IMAGE_FISHINGFAIL = "HTGame-FishingFail";
        private static readonly Bitmap img720_FishingFail = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHINGFAIL}-720.png");
        private static readonly Bitmap img1080_FishingFail = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHINGFAIL}-1080.png");
        private static readonly Bitmap img1440_FishingFail = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHINGFAIL}-1440.png");

        private const string IMAGE_FISHSALE = "HTGame-FishSale";
        private static readonly Bitmap img720_FishSale = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHSALE}-720.png");
        private static readonly Bitmap img1080_FishSale = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHSALE}-1080.png");
        private static readonly Bitmap img1440_FishSale = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHSALE}-1440.png");

        private const string IMAGE_FISHSTORAGE = "HTGame-FishStorage";
        private static readonly Bitmap img720_FishStorage = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHSTORAGE}-720.png");
        private static readonly Bitmap img1080_FishStorage = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHSTORAGE}-1080.png");
        private static readonly Bitmap img1440_FishStorage = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHSTORAGE}-1440.png");

        private const string IMAGE_FISHSTORAGEICON = "HTGame-FishStorageIcon";
        private static readonly Bitmap img720_FishStorageIcon = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHSTORAGEICON}-720.png");
        private static readonly Bitmap img1080_FishStorageIcon = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHSTORAGEICON}-1080.png");
        private static readonly Bitmap img1440_FishStorageIcon = new Bitmap($"{IMAGE_PATH}{IMAGE_FISHSTORAGEICON}-1440.png");

        private const string IMAGE_MOONCARD = "HTGame-MoonCard";
        private static readonly Bitmap img720_MoonCard = new Bitmap($"{IMAGE_PATH}{IMAGE_MOONCARD}-720.png");
        private static readonly Bitmap img1080_MoonCard = new Bitmap($"{IMAGE_PATH}{IMAGE_MOONCARD}-1080.png");
        //private static readonly Bitmap img1440_MoonCard = new Bitmap($"{IMAGE_PATH}{IMAGE_MOONCARD}-1440.png");

        private const string IMAGE_PAGECLOSE = "HTGame-PageClose";
        private static readonly Bitmap img720_PageClose = new Bitmap($"{IMAGE_PATH}{IMAGE_PAGECLOSE}-720.png");
        private static readonly Bitmap img1080_PageClose = new Bitmap($"{IMAGE_PATH}{IMAGE_PAGECLOSE}-1080.png");
        private static readonly Bitmap img1440_PageClose = new Bitmap($"{IMAGE_PATH}{IMAGE_PAGECLOSE}-1440.png");

        private const string IMAGE_SELECTBAIT = "HTGame-SelectBait";
        private static readonly Bitmap img720_SelectBait = new Bitmap($"{IMAGE_PATH}{IMAGE_SELECTBAIT}-720.png");
        private static readonly Bitmap img1080_SelectBait = new Bitmap($"{IMAGE_PATH}{IMAGE_SELECTBAIT}-1080.png");
        private static readonly Bitmap img1440_SelectBait = new Bitmap($"{IMAGE_PATH}{IMAGE_SELECTBAIT}-1440.png");

        private const string IMAGE_SHOPINGMAX = "HTGame-ShopingMax";
        private static readonly Bitmap img720_ShopingMax = new Bitmap($"{IMAGE_PATH}{IMAGE_SHOPINGMAX}-720.png");
        private static readonly Bitmap img1080_ShopingMax = new Bitmap($"{IMAGE_PATH}{IMAGE_SHOPINGMAX}-1080.png");
        private static readonly Bitmap img1440_ShopingMax = new Bitmap($"{IMAGE_PATH}{IMAGE_SHOPINGMAX}-1440.png");

        private const string IMAGE_TOBUY = "HTGame-ToBuy";
        private static readonly Bitmap img720_ToBuy = new Bitmap($"{IMAGE_PATH}{IMAGE_TOBUY}-720.png");
        private static readonly Bitmap img1080_ToBuy = new Bitmap($"{IMAGE_PATH}{IMAGE_TOBUY}-1080.png");
        private static readonly Bitmap img1440_ToBuy = new Bitmap($"{IMAGE_PATH}{IMAGE_TOBUY}-1440.png");

        public Bitmap this[string imgName]
        {
            get
            {
                switch (imgName)
                {
                    case "img720_BaitEmpty":
                        return img720_BaitEmpty;
                    case "img1080_BaitEmpty":
                        return img1080_BaitEmpty;
                    case "img1440_BaitEmpty":
                        return img1440_BaitEmpty;

                    case "img720_BaitUniversal":
                        return img720_BaitUniversal;
                    case "img1080_BaitUniversal":
                        return img1080_BaitUniversal;
                    case "img1440_BaitUniversal":
                        return img1440_BaitUniversal;

                    case "img720_Buy":
                        return img720_Buy;
                    case "img1080_Buy":
                        return img1080_Buy;
                    case "img1440_Buy":
                        return img1440_Buy;

                    case "img720_BuyConfirm":
                        return img720_BuyConfirm;
                    case "img1080_BuyConfirm":
                        return img1080_BuyConfirm;
                    case "img1440_BuyConfirm":
                        return img1440_BuyConfirm;

                    case "img720_Change":
                        return img720_Change;
                    case "img1080_Change":
                        return img1080_Change;
                    case "img1440_Change":
                        return img1440_Change;

                    case "img720_ChangeBaitUniversal":
                        return img720_ChangeBaitUniversal;
                    case "img1080_ChangeBaitUniversal":
                        return img1080_ChangeBaitUniversal;
                    case "img1440_ChangeBaitUniversal":
                        return img1440_ChangeBaitUniversal;

                    case "img720_ClickToClose":
                        return img720_ClickToClose;
                    case "img1080_ClickToClose":
                        return img1080_ClickToClose;
                    case "img1440_ClickToClose":
                        return img1440_ClickToClose;

                    case "img720_ClickToFishing":
                        return img720_ClickToFishing;
                    case "img1080_ClickToFishing":
                        return img1080_ClickToFishing;
                    case "img1440_ClickToFishing":
                        return img1440_ClickToFishing;

                    case "img720_ClickToStart":
                        return img720_ClickToStart;
                    case "img1080_ClickToStart":
                        return img1080_ClickToStart;
                    case "img1440_ClickToStart":
                        return img1440_ClickToStart;

                    case "img720_Confirm":
                        return img720_Confirm;
                    case "img1080_Confirm":
                        return img1080_Confirm;
                    case "img1440_Confirm":
                        return img1440_Confirm;

                    case "img720_EnterToFishing":
                        return img720_EnterToFishing;
                    case "img1080_EnterToFishing":
                        return img1080_EnterToFishing;
                    case "img1440_EnterToFishing":
                        return img1440_EnterToFishing;

                    case "img720_FishFull":
                        return img720_FishFull;
                    case "img1080_FishFull":
                        return img1080_FishFull;
                    case "img1440_FishFull":
                        return img1440_FishFull;

                    case "img720_FishingFail":
                        return img720_FishingFail;
                    case "img1080_FishingFail":
                        return img1080_FishingFail;
                    case "img1440_FishingFail":
                        return img1440_FishingFail;

                    case "img720_FishSale":
                        return img720_FishSale;
                    case "img1080_FishSale":
                        return img1080_FishSale;
                    case "img1440_FishSale":
                        return img1440_FishSale;

                    case "img720_FishStorage":
                        return img720_FishStorage;
                    case "img1080_FishStorage":
                        return img1080_FishStorage;
                    case "img1440_FishStorage":
                        return img1440_FishStorage;

                    case "img720_FishStorageIcon":
                        return img720_FishStorageIcon;
                    case "img1080_FishStorageIcon":
                        return img1080_FishStorageIcon;
                    case "img1440_FishStorageIcon":
                        return img1440_FishStorageIcon;

                    case "img720_MoonCard":
                        return img720_MoonCard;
                    case "img1080_MoonCard":
                    case "img1440_MoonCard":
                        return img1080_MoonCard;
                    //    return img1440_MoonCard;

                    case "img720_PageClose":
                        return img720_PageClose;
                    case "img1080_PageClose":
                        return img1080_PageClose;
                    case "img1440_PageClose":
                        return img1440_PageClose;

                    case "img720_SelectBait":
                        return img720_SelectBait;
                    case "img1080_SelectBait":
                        return img1080_SelectBait;
                    case "img1440_SelectBait":
                        return img1440_SelectBait;

                    case "img720_ShopingMax":
                        return img720_ShopingMax;
                    case "img1080_ShopingMax":
                        return img1080_ShopingMax;
                    case "img1440_ShopingMax":
                        return img1440_ShopingMax;

                    case "img720_ToBuy":
                        return img720_ToBuy;
                    case "img1080_ToBuy":
                        return img1080_ToBuy;
                    case "img1440_ToBuy":
                        return img1440_ToBuy;

                    default:
                        return null;
                }
            }
        }

        public Rectangle? GetImageRectangle(Bitmap windowImg, EGameImage gameImg)
        {
            int rectX, rectY, rectWidth, rectHeight;

            int width = windowImg.Width;
            int height = windowImg.Height;

            switch (gameImg)
            {
                case EGameImage.ClickToClose:
                    rectX = (int)(width * 0.43);
                    rectY = (int)(height * 0.833);
                    rectWidth = (int)(width * 0.156);
                    rectHeight = (int)(height * 0.139);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.ClickToFishing:
                    rectX = (int)(width * 0.391);
                    rectY = (int)(height * 0.209);
                    rectWidth = (int)(width * 0.235);
                    rectHeight = (int)(height * 0.07);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.FishStorageIcon:
                    rectX = (int)(width * 0.75);
                    rectY = (int)(height * 0.8);
                    rectWidth = (int)(width * 0.15);
                    rectHeight = (int)(height * 0.15);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.FishStorage:
                    rectX = (int)(width * 0.047);
                    rectY = (int)(height * 0.334);
                    rectWidth = (int)(width * 0.073);
                    rectHeight = (int)(height * 0.102);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.FishSale:
                    rectX = (int)(width * 0.511);
                    rectY = (int)(height * 0.871);
                    rectWidth = (int)(width * 0.084);
                    rectHeight = (int)(height * 0.047);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.Confirm:
                    rectX = (int)(width * 0.573);
                    rectY = (int)(height * 0.63);
                    rectWidth = (int)(width * 0.0625);
                    rectHeight = (int)(height * 0.056);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.PageClose:
                    rectX = (int)(width * 0.922);
                    rectY = 0;
                    rectWidth = (int)(width * 0.078);
                    rectHeight = (int)(height * 0.148);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.BaitEmpty:
                    rectX = (int)(width * 0.406);
                    rectY = (int)(height * 0.463);
                    rectWidth = (int)(width * 0.208);
                    rectHeight = (int)(height * 0.093);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.ChangeBaitUniversal:
                    rectX = (int)(width * 0.313);
                    rectY = (int)(height * 0.417);
                    rectWidth = (int)(width * 0.156);
                    rectHeight = (int)(height * 0.139);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.ToBuy:
                case EGameImage.Change:
                    rectX = (int)(width * 0.573);
                    rectY = (int)(height * 0.620);
                    rectWidth = (int)(width * 0.052);
                    rectHeight = (int)(height * 0.065);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.BaitUniversal:
                    rectX = (int)(width * 0.016);
                    rectY = (int)(height * 0.111);
                    rectWidth = (int)(width * 0.365);
                    rectHeight = (int)(height * 0.648);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.Buy:
                    rectX = (int)(width * 0.8125);
                    rectY = (int)(height * 0.926);
                    rectWidth = (int)(width * 0.052);
                    rectHeight = (int)(height * 0.056);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.BuyConfirm:
                    rectX = (int)(width * 0.573);
                    rectY = (int)(height * 0.63);
                    rectWidth = (int)(width * 0.052);
                    rectHeight = (int)(height * 0.056);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.ShopingMax:
                    rectX = (int)(width * 0.911);
                    rectY = (int)(height * 0.833);
                    rectWidth = (int)(width * 0.0625);
                    rectHeight = (int)(height * 0.093);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.MoonCard:
                    rectX = (int)(width * 0.401);
                    rectY = (int)(height * 0.278);
                    rectWidth = (int)(width * 0.209);
                    rectHeight = (int)(height * 0.371);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.FishingFail:
                    rectX = (int)(width * 0.448);
                    rectY = (int)(height * 0.463);
                    rectWidth = (int)(width * 0.104);
                    rectHeight = (int)(height * 0.074);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.FishFull:
                    rectX = (int)(width * 0.344);
                    rectY = (int)(height * 0.463);
                    rectWidth = (int)(width * 0.3125);
                    rectHeight = (int)(height * 0.084);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.ClickToStart:
                    rectX = (int)(width * 0.797);
                    rectY = (int)(height * 0.834);
                    rectWidth = (int)(width * 0.084);
                    rectHeight = (int)(height * 0.056);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.EnterToFishing:
                    rectX = (int)(width * 0.61);
                    rectY = (int)(height * 0.51);
                    rectWidth = (int)(width * 0.052);
                    rectHeight = (int)(height * 0.056);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);

                case EGameImage.SelectBait:
                    rectX = (int)(width * 0.852);
                    rectY = (int)(height * 0.653);
                    rectWidth = (int)(width * 0.086);
                    rectHeight = (int)(height * 0.125);
                    return new Rectangle(rectX, rectY, rectWidth, rectHeight);
            }

            return null;
        }
    }
}
