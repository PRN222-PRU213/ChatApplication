using ChatApplication.Business;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace ChatApplication
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private ChatService _chatService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ChatMessageViewModel> Messages { get; set; }

        public ObservableCollection<string> Emojis { get; } =
        new ObservableCollection<string>
        {
            "😀","😂","😍","❤️","🔥","👍","🎉","😢","😎","🤔"
        };

        public ICommand AddEmojiCommand { get; set; }
        public ICommand SendFileCommand { get; set; }  // Command mới

        private string _inputMessage;

        public string InputMessage
        {
            get => _inputMessage;
            set
            {
                if (_inputMessage != value)
                {
                    _inputMessage = value;
                    OnPropertyChanged(nameof(InputMessage));
                }
            }
        }

        public ICommand SendCommand { get; set; }

        private string _userName;

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        private string _serverIP = "127.0.0.1";

        public string ServerIP
        {
            get => _serverIP;
            set
            {
                if (_serverIP != value)
                {
                    _serverIP = value;
                    OnPropertyChanged(nameof(ServerIP));
                }
            }
        }

        private string _serverPort = "9999";

        public string ServerPort
        {
            get => _serverPort;
            set
            {
                if (_serverPort != value)
                {
                    _serverPort = value;
                    OnPropertyChanged(nameof(ServerPort));
                }
            }
        }

        private string _groupName = "General";

        public string GroupName
        {
            get => _groupName;
            set
            {
                if (_groupName != value)
                {
                    _groupName = value;
                    OnPropertyChanged(nameof(GroupName));
                }
            }
        }

        private bool _isConnected = false;

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                    OnPropertyChanged(nameof(IsNotConnected));
                    OnPropertyChanged(nameof(ConnectionStatus));
                }
            }
        }

        public bool IsNotConnected => !_isConnected;
        public string ConnectionStatus => IsConnected ? "🟢 Đã kết nối" : "🔴 Chưa kết nối";

        public ICommand ConnectCommand { get; set; }

        // Dictionary để theo dõi file đang nhận
        private Dictionary<string, ChatMessageViewModel> _receivingFiles = new();

        public MainViewModel()
        {
            Messages = new ObservableCollection<ChatMessageViewModel>();

            UserName = $"User-{Guid.NewGuid().ToString("N")[..6]}";

            ConnectCommand = new RelayCommand(() => ConnectToServer());

            SendCommand = new RelayCommand(() =>
            {
                if (IsConnected && !string.IsNullOrWhiteSpace(InputMessage))
                {
                    _chatService.SendMessage(InputMessage);
                    InputMessage = "";
                    OnPropertyChanged(nameof(InputMessage));
                }
            });

            AddEmojiCommand = new RelayCommand<string>(emoji =>
            {
                InputMessage += emoji;
                OnPropertyChanged(nameof(InputMessage));
            });

            // Command gửi file
            SendFileCommand = new RelayCommand(async () => await SelectAndSendFile());
        }

        private void ConnectToServer()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                AddSystemMessage("[LỖI] Vui lòng nhập tên người dùng!");
                return;
            }

            if (string.IsNullOrWhiteSpace(ServerIP))
            {
                AddSystemMessage("[LỖI] Vui lòng nhập địa chỉ IP!");
                return;
            }

            if (!int.TryParse(ServerPort, out int port) || port < 1 || port > 65535)
            {
                AddSystemMessage("[LỖI] Port không hợp lệ (1-65535)!");
                return;
            }

            try
            {
                AddSystemMessage($"Đang kết nối đến {ServerIP}:{ServerPort}...");

                _chatService = new ChatService
                {
                    ServerIP = ServerIP,
                    ServerPort = port
                };

                // Xử lý tin nhắn thường
                _chatService.MessageReceived += msg =>
                {
                    if (msg.Type == "MESSAGE" || msg.Type == "FILE_END")
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            bool isMine = msg.User == UserName;

                            Messages.Add(new ChatMessageViewModel
                            {
                                User = msg.User,
                                Message = msg.Message,
                                IsMine = isMine,
                                IsFile = msg.Type == "FILE_END",
                                FileName = msg.FileName
                            });
                        });
                    }
                };

                // Xử lý tiến trình file
                _chatService.FileProgressChanged += (fileId, fileName, current, total) =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (!_receivingFiles.ContainsKey(fileId))
                        {
                            var fileMsg = new ChatMessageViewModel
                            {
                                User = "FILE",
                                Message = $"📥 Đang nhận: {fileName}",
                                IsMine = false,
                                IsFile = true,
                                FileId = fileId,
                                FileName = fileName,
                                Progress = 0
                            };
                            _receivingFiles[fileId] = fileMsg;
                            Messages.Add(fileMsg);
                        }

                        _receivingFiles[fileId].Progress = current * 100 / total;
                    });
                };

                // Xử lý file đã nhận xong
                _chatService.FileReceived += (fileId, fileName, data) =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (_receivingFiles.TryGetValue(fileId, out var fileMsg))
                        {
                            fileMsg.Message = $"📎 Đã nhận: {fileName}";
                            fileMsg.Progress = 100;
                            fileMsg.FileData = data;
                            _receivingFiles.Remove(fileId);
                        }
                    });
                };

                _chatService.Connect(UserName, GroupName);

                IsConnected = true;
                AddSystemMessage($"✅ Đã kết nối thành công! Tên: {UserName} | Group: {GroupName}");
            }
            catch (Exception ex)
            {
                AddSystemMessage($"[LỖI] Không thể kết nối: {ex.Message}");
                IsConnected = false;
            }
        }

        private async Task SelectAndSendFile()
        {
            if (!IsConnected) return;

            var dialog = new OpenFileDialog
            {
                Title = "Chọn file để gửi",
                Filter = "Tất cả file (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                var fileInfo = new FileInfo(filePath);

                AddSystemMessage($"📤 Đang gửi file: {fileInfo.Name} ({FormatFileSize(fileInfo.Length)})...");

                await _chatService.SendFileAsync(filePath, progress =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        // Có thể cập nhật progress bar nếu cần
                    });
                });
            }
        }

        private void AddSystemMessage(string message)
        {
            Messages.Add(new ChatMessageViewModel
            {
                User = "SYSTEM",
                Message = message,
                IsMine = false
            });
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}