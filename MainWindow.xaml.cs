using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace twDofusOrganiser
{
    public partial class MainWindow : Window
    {
        private readonly WindowScanner scanner;
        private AppConfig config;
        private readonly WindowNavigator navigator;
        private readonly TrayManager trayManager;
        private readonly HotkeyManager hotkeyManager;

        // Observable collection for binding to the ListBox
        public ObservableCollection<WindowInfo> Windows { get; }
            = new ObservableCollection<WindowInfo>();

        public string PreviousHotkeyText { get; private set; } = string.Empty;
        public string NextHotkeyText { get; private set; } = string.Empty;

        private IntPtr windowHandle;
        private HwndSource? hwndSource;

        // Organizer / closing state
        private bool forceClose = false;

        // Drag and drop fields (WPF Point)
        private System.Windows.Point dragStartPoint;
        private bool isDragging = false;

        // Hotkey capture state
        private bool isCapturingHotkey;
        private HotkeyTarget currentCaptureTarget = HotkeyTarget.None;

        private int showMessageId = 0;

        // Win32 imports for window activation are handled by WindowNavigator.

        public MainWindow()
        {
            InitializeComponent();

            scanner = new WindowScanner();
            navigator = new WindowNavigator(Windows);
            trayManager = new TrayManager();
            hotkeyManager = new HotkeyManager();
            DataContext = this;

            // Wire tray events
            trayManager.ExitRequested += ExitApp;
            trayManager.ShowRequested += DeactivateOrganizerAndShow;

            // Wire hotkey events
            hotkeyManager.PreviousHotkeyPressed += FocusPreviousWindow;
            hotkeyManager.NextHotkeyPressed += FocusNextWindow;

            // Load persisted config
            config = ConfigManager.Load();

            // Load windows and apply order + IsEnabled
            LoadWindows();

            // Restore hotkeys from config (without saving to avoid overwriting during initialization)
            if (!string.IsNullOrWhiteSpace(config.PreviousHotkey) &&
                Hotkey.TryParse(config.PreviousHotkey, out var prev))
            {
                UpdatePreviousHotkey(prev, saveConfig: false);
            }
            else
            {
                UpdatePreviousHotkey(null, saveConfig: false);
            }

            if (!string.IsNullOrWhiteSpace(config.NextHotkey) &&
                Hotkey.TryParse(config.NextHotkey, out var next))
            {
                UpdateNextHotkey(next, saveConfig: false);
            }
            else
            {
                UpdateNextHotkey(null, saveConfig: false);
            }

            // Ensure labels show correct text at startup
            PreviousHotkeyLabel.Text = string.IsNullOrEmpty(PreviousHotkeyText) ? "None" : PreviousHotkeyText;
            NextHotkeyLabel.Text = string.IsNullOrEmpty(NextHotkeyText) ? "None" : NextHotkeyText;

            // Hook for Win32 messages
            SourceInitialized += MainWindow_SourceInitialized;

            // Organizer/tray is inactive at startup by design
            trayManager.SetOrganizerActive(false);
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            windowHandle = helper.Handle;
            hotkeyManager.SetWindowHandle(windowHandle);

            hwndSource = HwndSource.FromHwnd(windowHandle);
            if (hwndSource != null)
            {
                hwndSource.AddHook(WndProc);
            }

            // Get registered show message id
            showMessageId = SingleInstance.ShowMessageId;

            // Registration of hotkeys depends on tray manager state
            ApplyHotkeysRegistration();
        }

        private void ProcessShowMessage()
        {
            // If organizer is active (app in tray and hotkeys active), deactivate and show
            // Note: trayManager.IsOrganizerActive is public field in provided signature, use property if available
            bool isActive = false;
            try
            {
                isActive = trayManager.IsOrganizerActive;
            }
            catch
            {
                // fallback
            }

            if (isActive)
            {
                // Deactivate and show (same as tray show request)
                DeactivateOrganizerAndShow();
            }
            else
            {
                // Just show and bring to foreground
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle custom show message
            if (msg == showMessageId && showMessageId != 0)
            {
                ProcessShowMessage();
                handled = true;
                return IntPtr.Zero;
            }

            hotkeyManager.HandleWindowMessage(msg, wParam, lParam, ref handled);

            return IntPtr.Zero;
        }

        private void ExitApp()
        {
            forceClose = true;
            Close();
        }

        private void ActivateOrganizer()
        {
            trayManager.SetOrganizerActive(true);
            ApplyHotkeysRegistration();
        }

        private void DeactivateOrganizerAndShow()
        {
            trayManager.SetOrganizerActive(false);
            ApplyHotkeysRegistration();

            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void HideToTrayAndActivate()
        {
            Hide();
            ActivateOrganizer();
        }

        // Ouverture du menu quand on clique sur le bouton
        private void TrayMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Menu "Activer" -> passer en mode tray + hotkeys actifs
        private void ActivateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            HideToTrayAndActivate();
        }

        // Menu "Quitter" -> fermer complètement l'application
        private void QuitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            forceClose = true;
            Close();
        }

        private void ApplyHotkeysRegistration()
        {
            hotkeyManager.ApplyRegistration(trayManager.IsOrganizerActive);
        }

        private void LoadWindows()
        {
            Windows.Clear();

            var windows = scanner.GetDofusWindows();

            // Apply saved order if available
            if (config.WindowOrder.Count > 0)
            {
                windows.Sort((a, b) =>
                {
                    int ia = config.WindowOrder.IndexOf(a.CharacterName);
                    int ib = config.WindowOrder.IndexOf(b.CharacterName);

                    if (ia == -1 && ib == -1) return 0;
                    if (ia == -1) return 1;
                    if (ib == -1) return -1;
                    return ia.CompareTo(ib);
                });
            }

            foreach (var win in windows)
            {
                if (config.Enabled.TryGetValue(win.CharacterName, out bool enabled))
                    win.IsEnabled = enabled;

                win.PropertyChanged += Window_PropertyChanged;
                Windows.Add(win);
            }

            navigator.ResetIndex();

            if (Windows.Count > 0)
                WindowsList.SelectedIndex = 0;
        }

        private void Window_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WindowInfo.IsEnabled))
            {
                SaveConfig();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadWindows();
        }

        // -------- Drag & Drop for reordering --------

        private void WindowsList_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(null);
            isDragging = false;
        }

        private void WindowsList_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var currentPos = e.GetPosition(null);
            var diff = currentPos - dragStartPoint;

            if (!isDragging &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                isDragging = true;

                var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (listBoxItem == null)
                    return;

                var draggedItem = (WindowInfo)listBoxItem.DataContext;

                System.Windows.DragDrop.DoDragDrop(
                    listBoxItem,
                    draggedItem,
                    System.Windows.DragDropEffects.Move);
            }
        }

        private void WindowsList_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(WindowInfo)))
                return;

            var droppedData = (WindowInfo)e.Data.GetData(typeof(WindowInfo))!;

            var target = GetItemUnderMouse(e);
            if (target == null || target == droppedData)
                return;

            int oldIndex = Windows.IndexOf(droppedData);
            int newIndex = Windows.IndexOf(target);

            if (oldIndex == -1 || newIndex == -1 || oldIndex == newIndex)
                return;

            Windows.Move(oldIndex, newIndex);
            WindowsList.SelectedItem = droppedData;
            navigator.SetCurrentItem(droppedData);

            SaveConfig();
        }

        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T typed)
                    return typed;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private WindowInfo? GetItemUnderMouse(System.Windows.DragEventArgs e)
        {
            var pos = e.GetPosition(WindowsList);
            var element = WindowsList.InputHitTest(pos) as DependencyObject;
            if (element == null)
                return null;

            var listBoxItem = FindAncestor<ListBoxItem>(element);
            return listBoxItem?.DataContext as WindowInfo;
        }

        // -------- Hotkey registration / update --------

        private void UpdatePreviousHotkey(Hotkey? hotkey, bool saveConfig = true)
        {
            hotkeyManager.SetPreviousHotkey(hotkey);
            PreviousHotkeyText = hotkey?.ToString() ?? string.Empty;
            if (PreviousHotkeyLabel != null)
                PreviousHotkeyLabel.Text = string.IsNullOrEmpty(PreviousHotkeyText) ? "None" : PreviousHotkeyText;

            ApplyHotkeysRegistration();
            if (saveConfig)
                SaveConfig();
        }

        private void UpdateNextHotkey(Hotkey? hotkey, bool saveConfig = true)
        {
            hotkeyManager.SetNextHotkey(hotkey);
            NextHotkeyText = hotkey?.ToString() ?? string.Empty;
            if (NextHotkeyLabel != null)
                NextHotkeyLabel.Text = string.IsNullOrEmpty(NextHotkeyText) ? "None" : NextHotkeyText;

            ApplyHotkeysRegistration();
            if (saveConfig)
                SaveConfig();
        }

        // -------- Hotkey capture using buttons --------

        private enum HotkeyTarget
        {
            None,
            Previous,
            Next
        }

        private void PreviousHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            isCapturingHotkey = true;
            currentCaptureTarget = HotkeyTarget.Previous;
            PreviousHotkeyLabel.Text = "Press shortcut...";
            NextHotkeyLabel.Text = string.IsNullOrEmpty(NextHotkeyText) ? "None" : NextHotkeyText;

            Focus();
        }

        private void NextHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            isCapturingHotkey = true;
            currentCaptureTarget = HotkeyTarget.Next;
            NextHotkeyLabel.Text = "Press shortcut...";
            PreviousHotkeyLabel.Text = string.IsNullOrEmpty(PreviousHotkeyText) ? "None" : PreviousHotkeyText;

            Focus();
        }

        private void PreviousHotkeyButton_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            UpdatePreviousHotkey(null);
        }

        private void NextHotkeyButton_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            UpdateNextHotkey(null);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!isCapturingHotkey || currentCaptureTarget == HotkeyTarget.None)
                return;

            e.Handled = true;

            ModifierKeys modifiers = Keyboard.Modifiers;
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            // Ignore pure modifier keys
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            var newHotkey = new Hotkey(modifiers, key);

            if (currentCaptureTarget == HotkeyTarget.Previous)
            {
                UpdatePreviousHotkey(newHotkey);
            }
            else if (currentCaptureTarget == HotkeyTarget.Next)
            {
                UpdateNextHotkey(newHotkey);
            }

            isCapturingHotkey = false;
            currentCaptureTarget = HotkeyTarget.None;
        }

        // -------- Navigation Previous / Next --------

        private void FocusNextWindow()
        {
            var (window, index) = navigator.MoveNextEnabled();
            if (window == null || index < 0)
                return;

            WindowsList.SelectedIndex = index;
            WindowsList.ScrollIntoView(window);
        }

        private void FocusPreviousWindow()
        {
            var (window, index) = navigator.MovePreviousEnabled();
            if (window == null || index < 0)
                return;

            WindowsList.SelectedIndex = index;
            WindowsList.ScrollIntoView(window);
        }

        // -------- Config persistence --------

        private void SaveConfig()
        {
            config.WindowOrder = Windows.Select(w => w.CharacterName).ToList();
            config.Enabled = Windows.ToDictionary(w => w.CharacterName, w => w.IsEnabled);
            config.PreviousHotkey = PreviousHotkeyText;
            config.NextHotkey = NextHotkeyText;

            ConfigManager.Save(config);
        }

        // -------- Window lifecycle --------

        protected override void OnClosing(CancelEventArgs e)
        {
            // Si on vient du menu "Quitter" ou du menu du tray
            if (forceClose)
            {
                SaveConfig();
                base.OnClosing(e);
                return;
            }

            // Fermeture normale via le X système :
            // on sauvegarde juste, pas de tray automatique
            SaveConfig();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Ensure hotkeys are unregistered when window is closed
            hotkeyManager.ApplyRegistration(false);

            if (hwndSource != null)
            {
                hwndSource.RemoveHook(WndProc);
                hwndSource = null;
            }

            trayManager.Dispose();
        }

        // -------- Custom header drag --------

        private void HeaderGrid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Allow dragging the window by clicking on the custom header
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                DragMove();
            }
        }

    }
}
