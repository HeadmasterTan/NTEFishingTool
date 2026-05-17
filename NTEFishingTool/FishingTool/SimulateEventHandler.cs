using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NTEFishingTool.FishingTool
{
    internal class SimulateEventHandler
    {
        public const uint MOUSEEVENTF_LEFTDOWN = 0x02; // 鼠标左键按下
        public const uint MOUSEEVENTF_LEFTUP = 0x04; // 鼠标左键抬起

        public const uint KEYEVENTF_KEYDOWN = 0x00; // 键盘按键按下
        public const uint KEYEVENTF_KEYUP = 0x02; // 键盘按键抬起
        public const uint KEYEVENTF_SCANCODE = 0x08; // 使用扫描码

        public const byte SCAN_ESCAPE = 0x01; // ESC键
        public const byte SCAN_A = 0x1E; // A键
        public const byte SCAN_D = 0x20; // D键
        public const byte SCAN_F = 0x21; // F键
        public const byte SCAN_Q = 0x10; // Q键
        public const byte SCAN_W = 0x11; // W键
        public const byte SCAN_E = 0x12; // E键
        public const byte SCAN_R = 0x13; // R键

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public static void MouseClick(Point point, int delayMs = 50)
        {
            Cursor.Position = point;
            Thread.Sleep(100);

            mouse_event(MOUSEEVENTF_LEFTDOWN, point.X, point.Y, 0, 0);
            Thread.Sleep(delayMs);
            mouse_event(MOUSEEVENTF_LEFTUP, point.X, point.Y, 0, 0);
        }

        public static void SendScanCodeKeyDown(byte scanCode)
        {
            keybd_event(0, scanCode, KEYEVENTF_SCANCODE | KEYEVENTF_KEYDOWN, 0);
        }

        public static void SendScanCodeKeyUp(byte scanCode)
        {
            keybd_event(0, scanCode, KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP, 0);
        }

        public static void SendScanCodeKeyPress(byte scanCode)
        {
            SendScanCodeKeyDown(scanCode);
            Thread.Sleep(50);
            SendScanCodeKeyUp(scanCode);
        }
    }
}
