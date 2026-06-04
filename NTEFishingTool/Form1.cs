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
        private static string[] Languages = { "简体中文", "繁体中文", "English" };
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
                default:
                    SetChineseSimplified();
                    break;
            }
        }
    }
}
