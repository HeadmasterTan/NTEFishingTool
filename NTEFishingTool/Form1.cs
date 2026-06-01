using System;
using System.Windows.Forms;

using NTEFishingTool.FishingTool;

namespace NTEFishingTool
{
    public partial class Form1 : Form
    {
        private static readonly Fishing _fishingTool = Fishing.GetInstance();

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
    }
}
