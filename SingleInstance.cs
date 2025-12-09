using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace twDofusOrganiser
{
    internal static class SingleInstance
    {
        private const string MessageName = "TWDoFusOrganiser_ShowInstance_v1";
        private const int HWND_BROADCAST = 0xFFFF;

        public static int ShowMessageId { get; private set; }

        static SingleInstance()
        {
            ShowMessageId = RegisterWindowMessage(MessageName);
        }

        public static void PostShowMessage()
        {
            if (ShowMessageId == 0)
                return;

            PostMessage((IntPtr)HWND_BROADCAST, ShowMessageId, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
}
