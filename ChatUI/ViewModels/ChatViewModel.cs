using ChatUI.Models;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace ChatUI
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private string _login;
        private string _messageText;
        private bool _isConnected;
        private int _userCount;
        private readonly ChatService _chatService;

        public ObservableCollection<MessageModel> Messages { get; } = new();

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public string MessageText
        {
            get => _messageText;
            set { _messageText = value; OnPropertyChanged(); }
        }

        public int UserCount
        {
            get => _userCount;
            set { _userCount = value; OnPropertyChanged(); }
        }

        public ICommand ConnectCommand { get; }
        public ICommand SendMessageCommand { get; }

        public ChatViewModel()
        {
            _chatService = new ChatService();

            ConnectCommand = new RelayCommand(_ => Connect(), _ => !_isConnected);
            SendMessageCommand = new RelayCommand(_ => SendMessage(), _ => !string.IsNullOrWhiteSpace(MessageText));
        }

        private async void Connect()
        {
            if (_isConnected) return;

            _chatService.MessageReceived -= OnMessageReceived;
            _chatService.MessageReceived += OnMessageReceived;

            _chatService.UserCountUpdated -= OnUserCountUpdated;
            _chatService.UserCountUpdated += OnUserCountUpdated;

            try
            {
                using var client = new HttpClient();
                var response = await client.PostAsync("http://localhost:5000/auth/login",
                    new StringContent($"{{ \"userName\": \"{Login}\" }}", Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    Messages.Add(new MessageModel
                    {
                        User = "System",
                        Text = $"Не вдалося увійти: {response.StatusCode}",
                        Timestamp = DateTime.Now,
                        IsMine = false
                    });
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var jwt = JObject.Parse(json)["token"]?.ToString();

                if (string.IsNullOrWhiteSpace(jwt))
                {
                    Messages.Add(new MessageModel
                    {
                        User = "System",
                        Text = $"Сервер не повернув токен.",
                        Timestamp = DateTime.Now,
                        IsMine = false
                    });
                    return;
                }

                await _chatService.Connect(Login, jwt);
                _isConnected = true;
            }
            catch (Exception ex)
            {
                Messages.Add(new MessageModel
                {
                    User = "System",
                    Text = $"Помилка: {ex.Message}",
                    Timestamp = DateTime.Now,
                    IsMine = false
                });
            }
        }


        private async void SendMessage()
        {
            await _chatService.SendMessage(Login, MessageText);
            Messages.Add(new MessageModel
            {
                User = Login,
                Text = MessageText,
                Timestamp = DateTime.Now,
                IsMine = true
            });
            MessageText = string.Empty;
        }

        private void OnMessageReceived(string user, string message)
        {
            if (user == Login) return;

            App.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(new MessageModel
                {
                    User = user,
                    Text = message,
                    Timestamp = DateTime.Now,
                    IsMine = false
                });
            });
        }

        private void OnUserCountUpdated(int count)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                UserCount = count;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
