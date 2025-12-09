using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace twDofusOrganiser
{
    /// <summary>
    /// Handles navigation between Dofus windows (next / previous) and activation.
    /// </summary>
    public class WindowNavigator
    {
        private readonly IList<WindowInfo> windows;
        private int currentIndex = -1;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        public WindowNavigator(IList<WindowInfo> windows)
        {
            this.windows = windows ?? throw new ArgumentNullException(nameof(windows));
        }

        /// <summary>
        /// Re-initializes the current index based on the current windows collection.
        /// </summary>
        public void ResetIndex()
        {
            currentIndex = windows.Count > 0 ? 0 : -1;
        }

        /// <summary>
        /// Sets the current index to the given window if it exists in the list.
        /// </summary>
        public void SetCurrentItem(WindowInfo window)
        {
            if (window == null)
                return;

            int index = windows.IndexOf(window);
            if (index >= 0)
                currentIndex = index;
        }

        /// <summary>
        /// Moves to the next enabled window, activates it and returns it with its index.
        /// Returns (null, -1) if no enabled window is found.
        /// </summary>
        public (WindowInfo? window, int index) MoveNextEnabled()
        {
            if (windows.Count == 0)
                return (null, -1);

            if (currentIndex < 0 || currentIndex >= windows.Count)
                currentIndex = 0;

            for (int i = 0; i < windows.Count; i++)
            {
                currentIndex = (currentIndex + 1) % windows.Count;

                var win = windows[currentIndex];
                if (win.IsEnabled)
                {
                    ActivateGameWindow(win.Handle);
                    return (win, currentIndex);
                }
            }

            return (null, -1);
        }

        /// <summary>
        /// Moves to the previous enabled window, activates it and returns it with its index.
        /// Returns (null, -1) if no enabled window is found.
        /// </summary>
        public (WindowInfo? window, int index) MovePreviousEnabled()
        {
            if (windows.Count == 0)
                return (null, -1);

            if (currentIndex < 0 || currentIndex >= windows.Count)
                currentIndex = 0;

            for (int i = 0; i < windows.Count; i++)
            {
                currentIndex = (currentIndex - 1 + windows.Count) % windows.Count;

                var win = windows[currentIndex];
                if (win.IsEnabled)
                {
                    ActivateGameWindow(win.Handle);
                    return (win, currentIndex);
                }
            }

            return (null, -1);
        }

        private void ActivateGameWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return;

            // Only restore if the window is minimized, otherwise just bring it to foreground
            // This prevents repositioning windows that are already visible
            if (IsIconic(handle))
            {
                ShowWindow(handle, SW_RESTORE);
            }
            
            SetForegroundWindow(handle);
        }
    }
}


