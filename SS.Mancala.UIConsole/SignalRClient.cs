using Microsoft.AspNetCore.SignalR.Client;
using SS.Mancala.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Mancala.UIConsole
{
    public class SignalRClient : SignalRConnection
    {
        public SignalRClient(string hubAddress) : base(hubAddress)
        {

        }
        public SignalRClient(string hubAddress, string owner) :
           base(hubAddress, owner)
        {

        }
        public void NotifyGameStart(string gameId)
        {
            try
            {
                HubConnection.InvokeAsync("SendMessage", "System", $"Game {gameId} started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying game start: {ex.Message}");
            }
        }

        public void NotifyMove(string user, int pitPosition)
        {
            try
            {
                HubConnection.InvokeAsync("SendMessage", user, $"Played pit {pitPosition}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying move: {ex.Message}");
            }
        }

    }
}
