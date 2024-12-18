using Microsoft.AspNetCore.SignalR.Client;

public class SignalRConnection
{
    private string hubAddress;
    HubConnection _connection;

    public string User { get; set; }
    public string Message { get; set; }
    string owner;

    public SignalRConnection(string hubAddress)
    {
        this.hubAddress = hubAddress;
    }
    public SignalRConnection(string hubAddress, string owner)
    {
        this.hubAddress = hubAddress;
        this.owner = owner;
    }

    public HubConnection HubConnection
    {
        get { return _connection; }
    }

    public void Start()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubAddress)
            .Build();

        _connection.On<string, string>("ReceiveMessage", (s1, s2) => OnSend(s1, s2));

        _connection.StartAsync();
    }

    public void Stop()
    {
        _connection.StopAsync();
    }
    private void OnSend(string user, string message)
    {
        Console.WriteLine(user + ": " + message);
        this.User = user;
        this.Message = message;
    }

    public void SendMessageToChannel(string user, string message)
    {

        try
        {
            Console.WriteLine("Sending message " + message + " from " + user);
            _connection.InvokeAsync("SendMessage", user, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }


    }

    public void ConnectToChannel(string user)
    {
        Start();
        string message = user + " Connected";

        try
        {
            _connection.InvokeAsync("SendMessage", "System", message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

  
}