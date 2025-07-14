using Microsoft.AspNetCore.SignalR.Client;

public class ChatService
{
    private HubConnection _connection;
    private string _systemUserName = "System";

    public event Action<string, string> MessageReceived;
    public event Action<int> UserCountUpdated;

    public async Task Connect(string username, string jwtToken)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/chathub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(jwtToken);
            })
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
        if (_connection?.State == HubConnectionState.Connected)
            await _connection.InvokeAsync("SendMessage", _systemUserName, message);
        else
            MessageReceived?.Invoke(_systemUserName, message);
    }
}

