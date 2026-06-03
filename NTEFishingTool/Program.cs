using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace NTEFishingTool
{
    internal static class Program
    {
        /// <summary>
        /// 重定向兼容DLL加载，解决在某些环境下缺少System.Runtime.CompilerServices.Unsafe.dll导致的运行时错误
        /// </summary>
        private static void RedirectCompatibleDll()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if (args.Name.Contains("System.Runtime.CompilerServices.Unsafe"))
                {
                    string folderPath = AppDomain.CurrentDomain.BaseDirectory;
                    string assemblyPath = Path.Combine(folderPath, "System.Runtime.CompilerServices.Unsafe.dll");

                    if (File.Exists(assemblyPath))
                    {
                        return Assembly.LoadFrom(assemblyPath);
                    }
                }

                return null;
            };
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            RedirectCompatibleDll();

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
