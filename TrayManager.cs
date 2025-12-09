using System;
using System.Drawing;
using Forms = System.Windows.Forms;

namespace twDofusOrganiser
{
    /// <summary>
    /// Manages the system tray icon and organizer active state.
    /// </summary>
    public class TrayManager : IDisposable
    {
        private readonly Forms.NotifyIcon trayIcon;
        private bool organizerActive;

        public bool IsOrganizerActive => organizerActive;

        /// <summary>
        /// Raised when the user clicks "Exit" in the tray menu.
        /// </summary>
        public event Action? ExitRequested;

        /// <summary>
        /// Raised when the user left-clicks the tray icon while the organizer is active (to show the main window).
        /// </summary>
        public event Action? ShowRequested;

        public TrayManager()
        {
            trayIcon = new Forms.NotifyIcon();

            try
            {
                var uri = new Uri("pack://application:,,,/Images/Icon.ico", UriKind.Absolute);
                var streamResourceInfo = System.Windows.Application.GetResourceStream(uri);

                if (streamResourceInfo != null)
                    trayIcon.Icon = new Icon(streamResourceInfo.Stream);
                else
                    trayIcon.Icon = SystemIcons.Application;
            }
            catch
            {
                trayIcon.Icon = SystemIcons.Application;
            }

            trayIcon.Visible = false; // visible only when organizer is active
            trayIcon.Text = "Organizer inactive";

            var menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Quit", null, (s, e) => ExitRequested?.Invoke());
            trayIcon.ContextMenuStrip = menu;

            trayIcon.MouseClick += TrayIcon_MouseClick;
        }

        private void TrayIcon_MouseClick(object? sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left && organizerActive)
            {
                ShowRequested?.Invoke();
            }
        }

        /// <summary>
        /// Sets the organizer active state and updates icon visibility and tooltip.
        /// </summary>
        public void SetOrganizerActive(bool isActive)
        {
            organizerActive = isActive;

            if (organizerActive)
            {
                trayIcon.Visible = true;
                trayIcon.Text = "Organizer actif";
            }
            else
            {
                trayIcon.Visible = false;
                trayIcon.Text = "Organizer inactif";
            }
        }

        public void Dispose()
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }
    }
}


