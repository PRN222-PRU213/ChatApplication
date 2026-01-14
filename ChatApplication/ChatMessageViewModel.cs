using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ChatApplication
{
    public class ChatMessageViewModel : INotifyPropertyChanged
    {
        public string User { get; set; }

        private string _message;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public bool IsMine { get; set; }
        public string Alignment => IsMine ? "Right" : "Left";
        public string BackgroundColor => IsMine ? "#DCF8C6" : "#FFFFFF";
        public string Margin => IsMine ? "100,5,10,5" : "10,5,100,5";

        // === FILE PROPERTIES ===
        public bool IsFile { get; set; } = false;

        public string FileId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }

        // 🔥 FileData cần notify khi thay đổi
        private byte[] _fileData;

        public byte[] FileData
        {
            get => _fileData;
            set
            {
                _fileData = value;
                OnPropertyChanged(nameof(FileData));
                OnPropertyChanged(nameof(CanDownload));
                OnPropertyChanged(nameof(IsReadyToDownload));

                // 🔥 Tự động tạo ImageSource nếu là ảnh
                if (value != null && IsImageFile)
                {
                    CreateImageSource(value);
                }
            }
        }

        // 🔥 IMAGE PROPERTIES
        private BitmapImage _imageSource;

        public BitmapImage ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
                OnPropertyChanged(nameof(HasImage));
            }
        }

        // 🔥 Kiểm tra có phải file ảnh không
        public bool IsImageFile
        {
            get
            {
                if (string.IsNullOrEmpty(FileName)) return false;
                string ext = Path.GetExtension(FileName).ToLower();
                return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp";
            }
        }

        // 🔥 Có ảnh để hiển thị không
        public bool HasImage => ImageSource != null && IsImageFile;

        // 🔥 Hiển thị nút download cho file KHÔNG phải ảnh
        public bool ShowDownloadButton => IsReadyToDownload && !IsImageFile;

        // Trạng thái file: đã tải hay chưa
        private bool _isDownloaded = false;

        public bool IsDownloaded
        {
            get => _isDownloaded;
            set
            {
                _isDownloaded = value;
                OnPropertyChanged(nameof(IsDownloaded));
                OnPropertyChanged(nameof(CanDownload));
                OnPropertyChanged(nameof(IsReadyToDownload));
                OnPropertyChanged(nameof(ShowDownloadButton));
            }
        }

        // Có thể tải khi: là file + có data + chưa tải
        public bool CanDownload => IsFile && FileData != null && !IsDownloaded;

        private int _progress = 0;

        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(IsReceiving));
                OnPropertyChanged(nameof(IsReadyToDownload));
                OnPropertyChanged(nameof(ShowDownloadButton));
            }
        }

        // Đang nhận file (progress > 0 và < 100)
        public bool IsReceiving => IsFile && Progress > 0 && Progress < 100;

        // 🔥 Đã nhận xong và có thể tải
        public bool IsReadyToDownload => IsFile && Progress == 100 && FileData != null && !IsDownloaded;

        public string ProgressText => IsFile ? $"{Progress}%" : "";

        // 🔥 Command tải file - cần notify
        private ICommand _downloadCommand;

        public ICommand DownloadCommand
        {
            get => _downloadCommand;
            set
            {
                _downloadCommand = value;
                OnPropertyChanged(nameof(DownloadCommand));
                OnPropertyChanged(nameof(IsReadyToDownload));
                OnPropertyChanged(nameof(ShowDownloadButton));
            }
        }

        // 🔥 Tạo BitmapImage từ byte[]
        private void CreateImageSource(byte[] imageData)
        {
            try
            {
                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(imageData))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Cho phép sử dụng cross-thread
                }
                ImageSource = bitmap;
            }
            catch
            {
                ImageSource = null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}