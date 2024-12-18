using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SS.Mancala.Utility;
using Microsoft.AspNetCore.SignalR.Client;
using static System.Net.WebRequestMethods;
using SS.Mancala.UIConsole;

internal class Program
{
    private static string DrawMenu()
    {
        Console.WriteLine("Which operation do you wish to perform?");
        Console.WriteLine("Authenticate success (b)");
        Console.WriteLine("Connect to a channel (c)");
        Console.WriteLine("Get the secret (g)");
        Console.WriteLine("Send a message to the channel (s)");
      //  Console.WriteLine("Notify game start (n)");
     //   Console.WriteLine("Notify move (m)");
        Console.WriteLine("Exit (x)");
        return Console.ReadLine();
    }

    public static async Task<string> GetSecret(string secretName)
    {
        try
        {
            var keyVaultName = "kv-101521081-500122975";
            var kvUri = $"https://{keyVaultName}.vault.azure.net";

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(secretName);
            Console.WriteLine(secret.Value.Value.ToString());
            return secret.Value.Value.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    private static async Task Main(string[] args)
    {
        string user = "Brittany";
        string hubAddress = "https://bigprojectapi-500122975.azurewebsites.net/MancalaHub";
        //  string hubAddress = "https://fvtcdp.azurewebsites.net/MancalaHub";
        String apiAddress = "https://localhost:7113/api/";
        apiAddress = "https://bigprojectapi-500122975.azurewebsites.net/api/";
        ApiClient apiClient = new ApiClient(apiAddress);
        var signalRClient = new SignalRClient(hubAddress);



        bool listening = false;

        string operation = DrawMenu();

        while (operation != "x")
        {
            switch (operation.ToLower())
            {
                case "b":
                    try
                    {
                        var result = apiClient.Authenticate("bfoote", "maple");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine();
                        Console.WriteLine($"Authenticate: {result}");
                        Console.WriteLine($"Token: {apiClient.Token}");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine();
                        Console.WriteLine($"Authenticate failed: {ex.Message}");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;
                case "c":
                    signalRClient.ConnectToChannel(user);
                    string user1 = signalRClient.User;
                    string message1 = signalRClient.Message;
                    Console.WriteLine(user + ": " + message1);
                    break;
                case "g":
                    await GetSecret("Mancala-ConnectionString");
                    break;
                case "s":
                    Console.WriteLine("Message?");
                    string message = Console.ReadLine();
                    signalRClient.SendMessageToChannel(user, message);
                    break;
                //case "n":
                //    Console.WriteLine("Enter game ID:");
                //    string gameId = Console.ReadLine();
                //    signalRClient.NotifyGameStart(gameId);
                //    break;
                //case "m":
                //    Console.WriteLine("Enter pit position:");
                //    if (int.TryParse(Console.ReadLine(), out int pitPosition))
                //    {
                //        signalRClient.NotifyMove(user, pitPosition);
                //    }
                //    break;
            }
            operation = DrawMenu();
        }


        signalRClient.Stop();
        Console.WriteLine("Disconnected.");
    }
}
