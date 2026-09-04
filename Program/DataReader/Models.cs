using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DataHandler
{
    public class LogEntry
    {
        public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm:ss");
        public string Level { get; set; } = "INFO"; // INFO, WARNING, ERROR
        public string Message { get; set; } = string.Empty;

        public Brush LevelBrush => Level switch
        {
            "ERROR" => new SolidColorBrush(Color.FromRgb(255, 82, 82)),
            "WARNING" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),
            _ => new SolidColorBrush(Color.FromRgb(0, 229, 255))
        };
    }

    public class HardwareBlock : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _type = string.Empty;
        private string _statusText = "OK";
        private bool _isOk = true;
        private string _details = string.Empty;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusBrush)); }
        }

        public bool IsOk
        {
            get => _isOk;
            set { _isOk = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusBrush)); OnPropertyChanged(nameof(BadgeVisibility)); }
        }

        public string Details
        {
            get => _details;
            set { _details = value; OnPropertyChanged(); }
        }

        public System.Windows.Visibility BadgeVisibility => IsOk ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        public Brush StatusBrush => IsOk ? 
            new SolidColorBrush(Color.FromRgb(0, 230, 118)) : 
            new SolidColorBrush(Color.FromRgb(255, 82, 82));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
