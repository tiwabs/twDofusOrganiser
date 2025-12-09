using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace twDofusOrganiser
{
    /// <summary>
    /// Represents a user-defined global hotkey (modifiers + key).
    /// </summary>
    public class Hotkey
    {
        public ModifierKeys Modifiers { get; }
        public Key Key { get; }

        public Hotkey(ModifierKeys modifiers, Key key)
        {
            Modifiers = modifiers;
            Key = key;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Modifiers.HasFlag(ModifierKeys.Control))
                sb.Append("Ctrl+");
            if (Modifiers.HasFlag(ModifierKeys.Alt))
                sb.Append("Alt+");
            if (Modifiers.HasFlag(ModifierKeys.Shift))
                sb.Append("Shift+");
            if (Modifiers.HasFlag(ModifierKeys.Windows))
                sb.Append("Win+");

            sb.Append(Key.ToString());
            return sb.ToString();
        }

        // Parse "Ctrl+Alt+Tab" into Hotkey
        public static bool TryParse(string text, out Hotkey? hotkey)
        {
            hotkey = null;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return false;

            ModifierKeys mods = ModifierKeys.None;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                string p = parts[i].Trim();
                if (p.Equals("Ctrl", StringComparison.OrdinalIgnoreCase))
                    mods |= ModifierKeys.Control;
                else if (p.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                    mods |= ModifierKeys.Alt;
                else if (p.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                    mods |= ModifierKeys.Shift;
                else if (p.Equals("Win", StringComparison.OrdinalIgnoreCase))
                    mods |= ModifierKeys.Windows;
            }

            string keyPart = parts[^1].Trim();
            if (!Enum.TryParse(keyPart, out Key key))
                return false;

            hotkey = new Hotkey(mods, key);
            return true;
        }
    }

    /// <summary>
    /// Manages registration and handling of global hotkeys for the application.
    /// </summary>
    public class HotkeyManager
    {
        private const int HOTKEY_ID_PREVIOUS = 1;
        private const int HOTKEY_ID_NEXT = 2;
        private const int WM_HOTKEY = 0x0312;

        private IntPtr windowHandle;
        private Hotkey? previousHotkey;
        private Hotkey? nextHotkey;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public event Action? PreviousHotkeyPressed;
        public event Action? NextHotkeyPressed;

        /// <summary>
        /// Sets the window handle used for RegisterHotKey/UnregisterHotKey.
        /// </summary>
        public void SetWindowHandle(IntPtr handle)
        {
            windowHandle = handle;
        }

        public void SetPreviousHotkey(Hotkey? hotkey)
        {
            previousHotkey = hotkey;
        }

        public void SetNextHotkey(Hotkey? hotkey)
        {
            nextHotkey = hotkey;
        }

        /// <summary>
        /// Registers or unregisters hotkeys depending on the organizer active state.
        /// </summary>
        public void ApplyRegistration(bool organizerActive)
        {
            if (windowHandle == IntPtr.Zero)
                return;

            // Always unregister first
            UnregisterHotKey(windowHandle, HOTKEY_ID_PREVIOUS);
            UnregisterHotKey(windowHandle, HOTKEY_ID_NEXT);

            if (!organizerActive)
                return;

            if (previousHotkey != null)
                RegisterHotkeyInternal(HOTKEY_ID_PREVIOUS, previousHotkey);

            if (nextHotkey != null)
                RegisterHotkeyInternal(HOTKEY_ID_NEXT, nextHotkey);
        }

        /// <summary>
        /// Handles window messages and raises events when hotkeys are triggered.
        /// </summary>
        public void HandleWindowMessage(int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_HOTKEY)
                return;

            int id = wParam.ToInt32();
            if (id == HOTKEY_ID_PREVIOUS)
            {
                PreviousHotkeyPressed?.Invoke();
                handled = true;
            }
            else if (id == HOTKEY_ID_NEXT)
            {
                NextHotkeyPressed?.Invoke();
                handled = true;
            }
        }

        private void RegisterHotkeyInternal(int id, Hotkey hotkey)
        {
            uint fsModifiers = 0;
            if (hotkey.Modifiers.HasFlag(ModifierKeys.Alt))
                fsModifiers |= 0x1;
            if (hotkey.Modifiers.HasFlag(ModifierKeys.Control))
                fsModifiers |= 0x2;
            if (hotkey.Modifiers.HasFlag(ModifierKeys.Shift))
                fsModifiers |= 0x4;
            if (hotkey.Modifiers.HasFlag(ModifierKeys.Windows))
                fsModifiers |= 0x8;

            Key key = hotkey.Key == Key.System ? Key.LeftAlt : hotkey.Key;
            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            bool ok = RegisterHotKey(windowHandle, id, fsModifiers, vk);
            if (!ok)
            {
                System.Windows.MessageBox.Show(
                    "Unable to register hotkey. Maybe it is already used by another application.",
                    "Hotkey error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
        }
    }
}