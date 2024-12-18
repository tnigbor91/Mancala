using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace SS.Mancala.API.Hubs
{
    public class MancalaHub : Hub
    {
      
        public async Task StartNewGame()
        {
            try
            {
                await Clients.All.SendAsync("ReceiveMessage", "Server", "New game started!");
                Console.WriteLine("Notified clients of new game.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying clients about new game: {ex.Message}");
            }
        }


        public async Task SendMessage(string user, string message)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveMessage", user, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public async Task NotifyMove(Guid gameId, Guid playerId, int pitIndex)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveMove", gameId, playerId, pitIndex);
                Console.WriteLine($"Move notified: GameId={gameId}, PlayerId={playerId}, PitIndex={pitIndex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying move: {ex.Message}");
            }
        }


        public async Task UpdateBoard(int[] boardState)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveBoardUpdate", boardState);
                Console.WriteLine("Board state updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating board: {ex.Message}");
            }
        }


        public async Task SendMove(Guid gameId, Guid playerId, int pitIndex)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveMove", gameId, playerId, pitIndex);
                Console.WriteLine($"Sent move: GameId={gameId}, PlayerId={playerId}, PitIndex={pitIndex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending move: {ex.Message}");
            }
        }
    }
}
