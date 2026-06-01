using System;
using System.Windows.Forms;

using NTEFishingTool.FishingTool;

namespace NTEFishingTool
{
    public partial class Form1 : Form
    {
        private static readonly Fishing _fishingTool = Fishing.GetInstance();

        private static string[] Languages = { "简体中文", "繁体中文", "English" };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            selLanguage.DataSource = Languages;
            selLanguage.SelectedIndex = 0;
            SetChineseSimplified();
        }

        private void btnStartFishing_Click(object sender, EventArgs e)
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
            catch
            {
                return;
            }
        }

        private void btnStopFishing_Click(object sender, EventArgs e)
        {
            _fishingTool.Pause();

            if (btnStopFishing.Enabled)
            {
                btnStartFishing.Enabled = true;
                btnStopFishing.Enabled = false;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _fishingTool.Stop();
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
            btnStartFishing.Text = "自动钓鱼";
            btnStopFishing.Text = "停止钓鱼";
        }

        private void SetChineseTraditional()
        {
            this.Text = "安魂曲釣魚工具";
            label1.Text = "使用說明：";
            label2.Text = "1. 請保持遊戲界面完整暴露在桌面";
            label3.Text = "2. 開始自動釣魚後請勿再操作電腦（除非停止釣魚）";
            label4.Text = "3. 支持[16:9] [16:10] [24:10] [35:10]等分辨率";
            label5.Text = "4. 內置自動購買萬能魚餌和自動售出（不幫忙購買魚竿）";
            label6.Text = "確認已在遊戲中點擊【開始釣魚】後使用";
            btnStartFishing.Text = "自動釣魚";
            btnStopFishing.Text = "停止釣魚";
        }

        private void SetEnglish()
        {
            this.Text = "LacrimosaFishingTool";
            label1.Text = "Instructions:";
            label2.Text = "1. Keep the game window fully visible on desktop";
            label3.Text = "2. Do not use PC after auto-fishing starts (unless stopped)";
            label4.Text = "3. Supports [16:9], [16:10], [24:10], [35:10] resolutions";
            label5.Text = "4. Auto-buys bait & auto-sells fish (excludes fishing rods)";
            label6.Text = "Click [Start Fishing] in-game before using";
            btnStartFishing.Text = "AutoFish";
            btnStopFishing.Text = "StopFishing";
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
