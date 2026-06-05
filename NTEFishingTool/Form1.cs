using System;
using System.Windows.Forms;
using NLog;
using NTEFishingTool.FishingTool;

namespace NTEFishingTool
{
    public partial class Form1 : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly Fishing _fishingTool = Fishing.GetInstance();
        private static string[] Languages = { "简体中文", "繁体中文", "English", "日本語", "한국어" };
        private static string _hotKeyText = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 注册全局热键 F11
            bool hotkeyRegistered = SimulateEventHandler.RegisterHotKey(
                this.Handle,
                SimulateEventHandler.HOTKEY_ID,
                SimulateEventHandler.MOD_NONE,
                SimulateEventHandler.VK_F11);

            if (hotkeyRegistered)
            {
                _hotKeyText = "[F11]\n";
            }

            selLanguage.DataSource = Languages;
            selLanguage.SelectedIndex = 0;
            SetChineseSimplified();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312; // 热键消息常量

            if (m.Msg == WM_HOTKEY)
            {
                if (m.WParam.ToInt32() == SimulateEventHandler.HOTKEY_ID)
                {
                    HandleHotKey();
                }
            }

            base.WndProc(ref m);
        }

        private void HandleHotKey()
        {
            if (btnStartFishing.Enabled)
            {
                HandleStart();
            }
            else if (btnStopFishing.Enabled)
            {
                HandlePause();
            }
        }

        private void HandleStart()
        {
            try
            {
                _fishingTool.Start();
                if (btnStartFishing.Enabled)
                {
                    btnStartFishing.Enabled = false;
                    btnStopFishing.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【HandleStart】工具启动发生异常");
            }
        }

        private void btnStartFishing_Click(object sender, EventArgs e)
        {
            HandleStart();
        }

        private void HandlePause()
        {
            try
            {
                _fishingTool.Pause();

                if (btnStopFishing.Enabled)
                {
                    btnStartFishing.Enabled = true;
                    btnStopFishing.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【HandlePause】工具暂停发生异常");
            }
        }

        private void btnStopFishing_Click(object sender, EventArgs e)
        {
            HandlePause();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _fishingTool.Stop();
            SimulateEventHandler.UnregisterHotKey(this.Handle, SimulateEventHandler.HOTKEY_ID);
        }

        private void SetChineseSimplified()
        {
            this.Text = "安魂曲钓鱼工具";
            label1.Text = "使用说明：";
            label2.Text = "1. 请保持游戏界面完整暴露在桌面";
            label3.Text = "2. 开始自动钓鱼后请勿再操作电脑（除非停止钓鱼）";
            label4.Text = "3. 支持[16:9] [16:10] [24:10] [35:10]等分辨率";
            label5.Text = "4. 内置自动购买万能鱼饵和自动售出（不帮忙购买鱼竿）";
            label6.Text = "确认已在游戏中点击【开始钓鱼】后使用";
            btnStartFishing.Text = _hotKeyText + "自动钓鱼";
            btnStopFishing.Text = _hotKeyText + "停止钓鱼";
            labelHelp.Text = "遇到问题？";

            string helpText = "• 适当降低游戏画质或分辨率。\n" +
                              "• 关闭色彩增强、滤镜等功能。\n" +
                              "• 尝试换个钓鱼点。\n" +
                              "• 关闭AMD显卡帧生成";
            helpTips.SetToolTip(labelHelp, helpText);
        }

        private void SetChineseTraditional()
        {
            this.Text = "安魂曲釣魚工具";
            label1.Text = "使用說明：";
            label2.Text = "1. 請保持遊戲畫面完整顯露於桌面";
            label3.Text = "2. 開始自動釣魚後請勿操作電腦（除非停止釣魚）";
            label4.Text = "3. 支援 [16:9] [16:10] [24:10] [35:10] 等解析度";
            label5.Text = "4. 內建自動購買萬能魚餌與自動販售（不提供自動購買魚竿）";
            label6.Text = "確認已在遊戲中點擊【開始釣魚】後使用";
            btnStartFishing.Text = _hotKeyText + "自動釣魚";
            btnStopFishing.Text = _hotKeyText + "停止釣魚";
            labelHelp.Text = "遇到問題？";

            string helpText = "• 適當降低遊戲畫質或解析度。\n" +
                              "• 關閉色彩增強、濾鏡等功能。\n" +
                              "• 嘗試換個釣魚點。\n" +
                              "• 關閉AMD顯卡幀生成";
            helpTips.SetToolTip(labelHelp, helpText);
        }

        private void SetEnglish()
        {
            this.Text = "LacrimosaFishingTool";
            label1.Text = "Instructions:";
            label2.Text = "1. Keep the game window fully visible on desktop.";
            label3.Text = "2. Do not use PC after auto-fishing starts (until stopped).";
            label4.Text = "3. Supports [16:9], [16:10], [24:10], [35:10] resolutions.";
            label5.Text = "4. Auto-buys bait & auto-sells catch (No auto-buy rod).";
            label6.Text = "Click [Start Fishing] in-game before using.";
            btnStartFishing.Text = _hotKeyText + "AutoFishing";
            btnStopFishing.Text = _hotKeyText + "StopFishing";
            labelHelp.Text = "Help";

            string helpText = "• Lower game graphics or resolution.\n" +
                              "• Disable color enhancements & filters.\n" +
                              "• Try another fishing spot.\n" +
                              "• Turn off AMD Frame Generation.";
            helpTips.SetToolTip(labelHelp, helpText);
        }

        private void SetJapanese()
        {
            this.Text = "レクイエム自動釣りツール";
            label1.Text = "使い方：";
            label2.Text = "1. ゲーム画面を隠さず、デスクトップにすべて表示してください。";
            label3.Text = "2. 自動釣り開始後は、PCの操作を控えてください（停止時を除く）。";
            label4.Text = "3. [16:9] [16:10] [24:10] [35:10] などの解像度に対応。";
            label5.Text = "4. 万能釣り餌の自動購入・自動売却機能付き（釣竿は購入しません）。";
            label6.Text = "ゲーム内で【釣りを開始】したことを確認してから起動してください。";
            btnStartFishing.Text = _hotKeyText + "自動釣り";
            btnStopFishing.Text = _hotKeyText + "釣り停止";
            labelHelp.Text = "ヘルプ";

            string helpText = "• ゲームの画質や解像度を下げる。\n" +
                              "• カラー強調やフィルターをオフにする。\n" +
                              "• 別の釣りスポットを試す。\n" +
                              "• AMD製グラボのフレーム生成をオフにする。";
            helpTips.SetToolTip(labelHelp, helpText);
        }

        private void SetKorean()
        {
            this.Text = "라크리모사 자동 낚시 툴";
            label1.Text = "사용 방법:";
            label2.Text = "1. 게임 화면이 가려지지 않게 데스크톱에 온전히 띄워주세요.";
            label3.Text = "2. 자동 낚시 시작 후에는 컴퓨터 조작을 삼가세요 (중지 시 제외).";
            label4.Text = "3. [16:9], [16:10], [24:10], [35:10] 등 해상도 지원.";
            label5.Text = "4. 만능 미끼 자동 구매 및 자동 판매 내장 (낚싯대는 구매 안 함).";
            label6.Text = "게임 내에서 【낚시 시작】을 누른 후 실행해 주세요.";
            btnStartFishing.Text = _hotKeyText + "자동 낚시";
            btnStopFishing.Text = _hotKeyText + "낚시 중지";
            labelHelp.Text = "돕다";

            string helpText = "• 게임 화질이나 해상도 낮추기.\n" +
                              "• 색상 강화, 필터 등 기능 끄기.\n" +
                              "• 다른 낚시터로 이동해 보기.\n" +
                              "• AMD 그래픽카드 프레임 생성 기능 끄기.";
            helpTips.SetToolTip(labelHelp, helpText);
        }

        private void selLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = selLanguage.SelectedItem.ToString();
            switch (item)
            {
                case "简体中文":
                    SetChineseSimplified();
                    break;
                case "繁体中文":
                    SetChineseTraditional();
                    break;
                case "English":
                    SetEnglish();
                    break;
                case "日本語":
                    SetJapanese();
                    break;
                case "한국어":
                    SetKorean();
                    break;
                default:
                    SetChineseSimplified();
                    break;
            }
        }
    }
}
