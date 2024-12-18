using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using SS.Mancala.BL.Models;
using SS.Mancala.PL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SS.Mancala.BL
{

    public class MoveManager : GenericManager<tblMove>
    {
        public MoveManager(ILogger logger, DbContextOptions<MancalaEntities> options) : base(logger, options) { }
        public MoveManager(DbContextOptions<MancalaEntities> options) : base(options) { }
        public MoveManager() { }

        public async Task<bool> ApplyMove(Game game, int pitPosition)
        {
            try
            {
      
                LogGameState(game, pitPosition);

       
                ValidateMove(game, pitPosition);

       
                var selectedPit = game.Pits[pitPosition];

        
                bool isExtraTurn = DistributeStones(game, pitPosition);


                if (IsGameOver(game))
                {
                    HandleGameOver(game);
                    return false;
                }

                if (!isExtraTurn)
                {
                    SwitchTurn(game);
                }

                return isExtraTurn;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ApplyMove: {ex.Message}");
                throw;
            }
        }

        private void LogGameState(Game game, int pitPosition)
        {
            Console.WriteLine("\n=== Game State ===");
            Console.WriteLine($"Game ID: {game.Id}");
            Console.WriteLine($"Player 1: {game.Player1Id}");
            Console.WriteLine($"Player 2: {game.Player2Id}");
            Console.WriteLine($"Current Turn: {game.CurrentTurn}");
            Console.WriteLine($"Selected Pit: {pitPosition}");

            Console.WriteLine("\nCurrent Board State:");
            foreach (var pit in game.Pits.OrderBy(p => p.PitPosition))
            {
                Console.WriteLine($"Pit {pit.PitPosition}: " +
                                $"Stones={pit.Stones}, " +
                                $"Owner={pit.PlayerId}, " +
                                $"IsMancala={pit.IsMancala}, " +
                                $"BelongsToCurrentPlayer={pit.PlayerId == game.CurrentTurn}");
            }
        }

        private void ValidateMove(Game game, int pitPosition)
        {
            Console.WriteLine("\n=== Validating Move ===");
            Console.WriteLine($"Pit Position: {pitPosition}");
            Console.WriteLine($"Player 1 ID: {game.Player1Id}");
            Console.WriteLine($"Player 2 ID: {game.Player2Id}");
            Console.WriteLine($"Current Turn: {game.CurrentTurn}");

            if (pitPosition < 0 || pitPosition >= game.Pits.Count)
            {
                throw new InvalidOperationException($"Invalid pit position: {pitPosition}");
            }

            var pit = game.Pits[pitPosition];

            Console.WriteLine($"Selected Pit Details:");
            Console.WriteLine($"  Position: {pitPosition}");
            Console.WriteLine($"  Owner ID: {pit.PlayerId}");
            Console.WriteLine($"  Is Player 1's: {pit.PlayerId == game.Player1Id}");
            Console.WriteLine($"  Is Player 2's: {pit.PlayerId == game.Player2Id}");
            Console.WriteLine($"  Is Current Player's: {pit.PlayerId == game.CurrentTurn}");
            Console.WriteLine($"  Stones: {pit.Stones}");
            Console.WriteLine($"  Is Mancala: {pit.IsMancala}");

            // Check pit ownership
            bool isCurrentPlayersPit = pit.PlayerId == game.CurrentTurn;
            if (!isCurrentPlayersPit)
            {
                throw new InvalidOperationException(
                    $"Cannot play from opponent's pit. Pit belongs to {pit.PlayerId}, but it's {game.CurrentTurn}'s turn");
            }

            // Check if Mancala
            if (pit.IsMancala)
            {
                throw new InvalidOperationException("Cannot play from Mancala");
            }

            // Check if empty
            if (pit.Stones == 0)
            {
                throw new InvalidOperationException("Cannot play from empty pit");
            }

            Console.WriteLine("Move validation passed\n");
        }

        private bool DistributeStones(Game game, int startPitPosition)
        {
            var startPit = game.Pits[startPitPosition];
            int stonesToMove = startPit.Stones;
            startPit.Stones = 0;

            Console.WriteLine($"\nDistributing {stonesToMove} stones from pit {startPitPosition}");

            int currentPosition = startPitPosition;

            while (stonesToMove > 0)
            {
                currentPosition = (currentPosition + 1) % game.Pits.Count;

                // Skip opponent's Mancala
                bool isOpponentMancala =
                    (currentPosition == 6 && game.CurrentTurn == game.Player2Id) ||
                    (currentPosition == 13 && game.CurrentTurn == game.Player1Id);

                if (isOpponentMancala)
                {
                    Console.WriteLine($"Skipping opponent's Mancala at {currentPosition}");
                    continue;
                }

                game.Pits[currentPosition].Stones++;
                stonesToMove--;

                Console.WriteLine($"Added stone to pit {currentPosition} (now has {game.Pits[currentPosition].Stones})");
            }

            // Check for extra turn
            bool isExtraTurn = (game.CurrentTurn == game.Player1Id && currentPosition == 6) ||
                              (game.CurrentTurn == game.Player2Id && currentPosition == 13);

            Console.WriteLine($"Last stone in pit {currentPosition}, Extra turn: {isExtraTurn}");
            return isExtraTurn;
        }

        private void SwitchTurn(Game game)
        {
            var oldTurn = game.CurrentTurn;
            game.CurrentTurn = game.CurrentTurn == game.Player1Id ? game.Player2Id : game.Player1Id;
            Console.WriteLine($"Switching turn from {oldTurn} to {game.CurrentTurn}");
        }

        private bool IsGameOver(Game game)
        {
            var player1EmptyPits = game.Pits
                .Where(p => p.PlayerId == game.Player1Id && !p.IsMancala)
                .All(p => p.Stones == 0);

            var player2EmptyPits = game.Pits
                .Where(p => p.PlayerId == game.Player2Id && !p.IsMancala)
                .All(p => p.Stones == 0);

            return player1EmptyPits || player2EmptyPits;
        }

        private void HandleGameOver(Game game)
        {
            Console.WriteLine("\nGame Over - Collecting remaining stones");

            var player1Mancala = game.Pits.First(p => p.IsMancala && p.PlayerId == game.Player1Id);
            var player2Mancala = game.Pits.First(p => p.IsMancala && p.PlayerId == game.Player2Id);

       
            foreach (var pit in game.Pits.Where(p => !p.IsMancala))
            {
                if (pit.PlayerId == game.Player1Id)
                {
                    player1Mancala.Stones += pit.Stones;
                }
                else
                {
                    player2Mancala.Stones += pit.Stones;
                }
                pit.Stones = 0;
            }

            // Set game status
            game.IsGameOver = true;
            game.Status = player1Mancala.Stones > player2Mancala.Stones ? "Player 1 Wins!" :
                         player2Mancala.Stones > player1Mancala.Stones ? "Player 2 Wins!" :
                         "It's a Tie!";

            Console.WriteLine($"Game Over: {game.Status}");
        }

   
        public async Task<Guid> InsertAsync(Move move, bool rollback = false)
        {
            try
            {
                tblMove row = Map<Move, tblMove>(move);
                return await base.InsertAsync(row, e => e.Id == e.Id, rollback);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateAsync(Move move, bool rollback = false)
        {
            try
            {
                tblMove row = Map<Move, tblMove>(move);
                return await base.UpdateAsync(row, rollback);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating move", ex);
            }
        }

        public async Task<List<Move>> LoadAsync()
        {
            try
            {
                var rows = await base.LoadAsync();
                return rows?.Any() == true
                    ? rows.Select(e => Map<tblMove, Move>(e)).ToList()
                    : new List<Move>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading moves", ex);
            }
        }

        public async Task<Move> LoadByIdAsync(Guid id)
        {
            try
            {
                var rows = await base.LoadAsync(e => e.Id == id);
                if (!rows?.Any() == true)
                {
                    throw new Exception("Move not found.");
                }
                return Map<tblMove, Move>(rows.First());
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading move by Id", ex);
            }
        }
    }
}




