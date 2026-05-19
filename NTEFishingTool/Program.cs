using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NTEFishingTool
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 使程序适应高DPI显示屏
            //if (Environment.OSVersion.Version.Major >= 6)
            //{
            //    SetProcessDPIAware();
            //}

            Application.Run(new Form1());
        }

        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //public static extern bool SetProcessDPIAware();
    }
}
