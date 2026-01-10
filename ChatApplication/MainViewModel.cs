using ChatApplication.Business;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApplication
{
    class MainViewModel : INotifyPropertyChanged
    {
        private ChatService _chatService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> Messages { get; set; }

        public ObservableCollection<string> Emojis { get; } =
        new ObservableCollection<string>
        {
            "😀","😂","😍","❤️","🔥","👍","🎉","😢","😎","🤔"
        };
        public ICommand AddEmojiCommand { get; set; }

        // 🔥 FIX INPUTMESSAGE
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

        // ✅ Tự sinh username – KHÔNG BAO GIỜ TRÙNG
        public string UserName { get; set; }

        public string GroupName { get; set; } = "General";

        public MainViewModel()
        {
            Messages = new ObservableCollection<string>();

            UserName = $"User-{Guid.NewGuid().ToString("N")[..6]}";

            _chatService = new ChatService();
            _chatService.Connect(UserName, GroupName);

            _chatService.MessageReceived += msg =>
            {
                if (msg.Type == "MESSAGE")
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add($"{msg.User}: {msg.Message}");
                    });
                }
            };

            SendCommand = new RelayCommand(() =>
            {
                if (!string.IsNullOrWhiteSpace(InputMessage))
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
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
