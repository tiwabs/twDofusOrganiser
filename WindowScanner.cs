using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace twDofusOrganiser
{
    public class WindowScanner
    {
        // Delegate used by EnumWindows
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

        [DllImport("user32.dll")]
        private static extern void GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        public List<WindowInfo> GetDofusWindows()
        {
            Console.WriteLine("Scanning for Dofus windows...");

            var results = new List<WindowInfo>();

            EnumWindows((hWnd, lParam) =>
            {
                int len = GetWindowTextLength(hWnd);
                if (len == 0)
                    return true;

                var titleBuilder = new StringBuilder(len + 1);
                GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);

                var classBuilder = new StringBuilder(256);
                GetClassName(hWnd, classBuilder, classBuilder.Capacity);

                GetWindowThreadProcessId(hWnd, out int pid);

                // Filter on Unity windows (Dofus client)
                if (!classBuilder.ToString().StartsWith("UnityWndClass"))
                    return true;

                Console.WriteLine($"Found Dofus window: {titleBuilder} (PID: {pid})");

                results.Add(new WindowInfo
                {
                    Handle = hWnd,
                    Title = titleBuilder.ToString(), // Will parse CharacterName and ClassName
                    ProcessId = pid,
                    IsEnabled = true
                });

                return true;
            }, IntPtr.Zero);

            return results;
        }
    }
}