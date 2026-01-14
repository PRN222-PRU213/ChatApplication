using System;
using System.ComponentModel;

namespace ChatApplication
{
    public class ChatMessageViewModel : INotifyPropertyChanged
    {
        public string User { get; set; }
        public string Message { get; set; }
        public bool IsMine { get; set; }
        public string Alignment => IsMine ? "Right" : "Left";
        public string BackgroundColor => IsMine ? "#DCF8C6" : "#FFFFFF";
        public string Margin => IsMine ? "100,5,10,5" : "10,5,100,5";

        // === FILE PROPERTIES ===
        public bool IsFile { get; set; } = false;

        public string FileId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public byte[] FileData { get; set; }

        private int _progress = 0;

        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText => IsFile ? $"{Progress}%" : "";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}