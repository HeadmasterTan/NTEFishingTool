using System;
using System.Windows.Forms;

using FishingTool;

namespace NTEFishingTool
{
    public partial class Form1 : Form
    {
        private static Fishing fishingTool = Fishing.GetInstance();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnStartFishing_Click(object sender, EventArgs e)
        {
            try
            {
                fishingTool.Start();
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
            fishingTool.Stop();

            if (btnStopFishing.Enabled)
            {
                btnStartFishing.Enabled = true;
                btnStopFishing.Enabled = false;
            }
        }
    }
}
