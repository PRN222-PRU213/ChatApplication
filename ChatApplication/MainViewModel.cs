using ChatApplication.Business;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        public ICommand SendFileCommand { get; set; }

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

        private string _serverIP = "172.16.16.1";

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
                    OnPropertyChanged(nameof(CanSendFile));
                }
            }
        }

        // 🔥 Trạng thái đang gửi file
        private bool _isSendingFiles = false;

        public bool IsSendingFiles
        {
            get => _isSendingFiles;
            set
            {
                if (_isSendingFiles != value)
                {
                    _isSendingFiles = value;
                    OnPropertyChanged(nameof(IsSendingFiles));
                    OnPropertyChanged(nameof(CanSendFile));
                }
            }
        }

        public bool CanSendFile => IsConnected && !IsSendingFiles;
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

            // 🔥 Gửi nhiều file
            SendFileCommand = new RelayCommand(async () => await SelectAndSendMultipleFiles());
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

                // 🔥 Xử lý file đã nhận xong - KHÔNG TỰ ĐỘNG LƯU
                _chatService.FileReceived += (fileId, fileName, data) =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (_receivingFiles.TryGetValue(fileId, out var fileMsg))
                        {
                            // Cập nhật thông tin file
                            fileMsg.Message = $"📎 {fileName} ({FormatFileSize(data.Length)})";
                            fileMsg.Progress = 100;
                            fileMsg.FileData = data;  // Lưu data trong memory
                            fileMsg.FileName = fileName;

                            // 🔥 Gán Command tải file - CHỈ TẢI KHI CLICK
                            fileMsg.DownloadCommand = new RelayCommand(() => DownloadFile(fileMsg));

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

        /// <summary>
        /// Tải file khi người dùng click vào nút Download
        /// </summary>
        private void DownloadFile(ChatMessageViewModel fileMsg)
        {
            if (fileMsg.FileData == null || fileMsg.IsDownloaded)
                return;

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Lưu file",
                    FileName = fileMsg.FileName,
                    Filter = GetFileFilter(fileMsg.FileName)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveDialog.FileName, fileMsg.FileData);
                    fileMsg.IsDownloaded = true;
                    fileMsg.Message = $"✅ Đã tải: {fileMsg.FileName}";
                    AddSystemMessage($"💾 Đã lưu file: {saveDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                AddSystemMessage($"[LỖI] Không thể lưu file: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔥 Chọn và gửi NHIỀU FILE cùng lúc
        /// </summary>
        /// <summary>
        /// 🔥 Chọn và gửi NHIỀU FILE cùng lúc
        /// </summary>
        private async Task SelectAndSendMultipleFiles()
        {
            if (!IsConnected || IsSendingFiles) return;

            var dialog = new OpenFileDialog
            {
                Title = "Chọn file để gửi (giữ Ctrl để chọn nhiều file)",
                Filter = "Tất cả file (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true && dialog.FileNames.Length > 0)
            {
                string[] filePaths = dialog.FileNames;
                int totalFiles = filePaths.Length;

                long totalSize = filePaths.Sum(f => new FileInfo(f).Length);
                AddSystemMessage($"📤 Chuẩn bị gửi {totalFiles} file ({FormatFileSize(totalSize)})...");

                IsSendingFiles = true;

                try
                {
                    int sentCount = 0;

                    foreach (string filePath in filePaths)
                    {
                        var fileInfo = new FileInfo(filePath);
                        sentCount++;

                        // 🔥 Đọc file data để hiển thị preview (nếu là ảnh)
                        byte[] fileData = File.ReadAllBytes(filePath);

                        var sendingMsg = new ChatMessageViewModel
                        {
                            User = UserName,  // 🔥 Đổi từ "UPLOAD" sang UserName
                            Message = $"📤 [{sentCount}/{totalFiles}] Đang gửi: {fileInfo.Name}",
                            IsMine = true,
                            IsFile = true,
                            FileName = fileInfo.Name,
                            Progress = 0
                        };

                        // 🔥 Gán FileData để hiển thị ảnh preview
                        sendingMsg.FileData = fileData;
                        sendingMsg.DownloadCommand = new RelayCommand(() => DownloadFile(sendingMsg));

                        Messages.Add(sendingMsg);

                        // Gửi file và đợi hoàn thành
                        await _chatService.SendFileAsync(filePath, progress =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                sendingMsg.Progress = progress;
                                sendingMsg.Message = $"📤 [{sentCount}/{totalFiles}] {fileInfo.Name} ({progress}%)";
                            });
                        });

                        // Cập nhật hoàn thành
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            sendingMsg.Progress = 100;
                            sendingMsg.Message = $"✅ Đã gửi: {fileInfo.Name} ({FormatFileSize(fileData.Length)})";
                        });

                        // 🔥 TĂNG DELAY giữa các file để server và client kịp xử lý
                        await Task.Delay(500);
                    }

                    //AddSystemMessage($"✅ Đã gửi xong {totalFiles} file!");
                }
                catch (Exception ex)
                {
                    AddSystemMessage($"[LỖI] Gửi file thất bại: {ex.Message}");
                }
                finally
                {
                    IsSendingFiles = false;
                }
            }
        }

        private string GetFileFilter(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();

            return ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => $"Image files (*{ext})|*{ext}|All files (*.*)|*.*",
                ".pdf" => "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                ".doc" or ".docx" => "Word documents (*.doc;*.docx)|*.doc;*.docx|All files (*.*)|*.*",
                ".xls" or ".xlsx" => "Excel files (*.xls;*.xlsx)|*.xls;*.xlsx|All files (*.*)|*.*",
                ".zip" or ".rar" or ".7z" => "Archive files (*.zip;*.rar;*.7z)|*.zip;*.rar;*.7z|All files (*.*)|*.*",
                _ => $"Files (*{ext})|*{ext}|All files (*.*)|*.*"
            };
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