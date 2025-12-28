using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace twDofusOrganiser
{
    /// <summary>
    /// Represents a Dofus game window with parsed metadata.
    /// </summary>
    public class WindowInfo : INotifyPropertyChanged
    {
        public IntPtr Handle { get; set; }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    UpdateFromTitle();
                    OnPropertyChanged();
                }
            }
        }

        private string className = string.Empty;
        public string ClassName
        {
            get => className;
            set
            {
                if (className != value)
                {
                    className = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ClassIconPath));
                }
            }
        }

        public int ProcessId { get; set; }

        private string characterName = string.Empty;
        public string CharacterName
        {
            get => characterName;
            private set
            {
                if (characterName != value)
                {
                    characterName = value;
                    OnPropertyChanged();
                }
            }
        }

        // Icon path using pack URI (Images folder at project root)
        public string ClassIconPath => $"pack://application:,,,/Images/{ClassName}.png";

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Parse "<Name> - <Class> - ..."
        private void UpdateFromTitle()
        {
            string raw = title ?? string.Empty;

            string parsedName = string.Empty;
            string parsedClass = string.Empty;

            if (!string.IsNullOrWhiteSpace(raw))
            {
                // Split only on the exact separator " - " to avoid false positives (e.g. Jean-Paul)
                string[] parts = raw.Split(" - ", StringSplitOptions.None);

                if (parts.Length >= 1)
                    parsedName = parts[0].Trim();

                if (parts.Length >= 2)
                    parsedClass = parts[1].Trim();
            }

            // Fallback: if name is still empty, use the raw title
            if (string.IsNullOrWhiteSpace(parsedName))
                parsedName = raw.Trim();

            CharacterName = parsedName;
            ClassName = parsedClass;
        }
    }
}


