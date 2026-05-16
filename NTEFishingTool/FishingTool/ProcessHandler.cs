using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NTEFishingTool.FishingTool
{
    internal class ProcessHandler
    {
        /// <summary>
        /// 将指定窗口设置为前台窗口，使其获得用户的输入焦点。
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 获取当前桌面聚焦的窗口的句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public static Process GetProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            return processes.Length > 0 ? processes[0] : null;
        }
    }
}
