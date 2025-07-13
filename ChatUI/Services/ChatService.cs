using Microsoft.AspNetCore.SignalR.Client;

public class ChatService
{
    private HubConnection _connection;

    public event Action<string, string> MessageReceived;
    public event Action<int> UserCountUpdated;

    private readonly string _systemUserName = "System";

    public async Task Connect(string username)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"http://localhost:5000/chathub?user={username}")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            MessageReceived?.Invoke(user, message);
        });

        _connection.On<int>("UserCountUpdated", count =>
        {
            UserCountUpdated?.Invoke(count);
        });

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            MessageReceived?.Invoke(_systemUserName, $"Помилка підключення: {ex.Message}");
        }
    }

    public async Task SendMessage(string user, string message)
    {
        try
        {
            await _connection.InvokeAsync("SendMessage", user, message);
        }
        catch (Exception ex)
        {
            await SendSystemMessage($"Помилка надсилання повідомлення: {ex.Message}");
        }
    }

    public async Task SendSystemMessage(string message)
    {
        if (_connection != null && _connection.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("SendMessage", _systemUserName, message);
        }
        else
        {
            MessageReceived?.Invoke(_systemUserName, message);
        }
    }
}
