using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using SS.Mancala.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SS.Mancala.BL
{
    public class GameManager : GenericManager<tblGame>
    {
        public GameManager(ILogger logger, DbContextOptions<MancalaEntities> options) : base(logger, options) { }
        public GameManager(DbContextOptions<MancalaEntities> options) : base(options) { }
        public GameManager() : base() { }

        public async Task<Guid> InsertAsync(Game game, bool rollback = false)
        {
            try
            {
                game.Id = Guid.NewGuid();
                tblGame row = Map<Game, tblGame>(game);

                return await base.InsertAsync(row, e => e.Id == game.Id, rollback);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting game: {ex.Message}", ex);
            }
        }

        //public async Task<Game> LoadByIdAsync(Guid id)
        //{
        //    try
        //    {
        //        using (var context = new MancalaEntities(options))
        //        {
        //            var tblGame = await context.tblGames
        //                .Include(g => g.Pits)
        //                .Include(g => g.Moves)
        //                .FirstOrDefaultAsync(g => g.Id == id);

        //            if (tblGame == null)
        //                throw new Exception($"Game with ID {id} not found.");

        //            var game = Map<tblGame, Game>(tblGame);

        //            // Map pits
        //            game.Pits = tblGame.Pits.Select(p => new Pit
        //            {
        //                Id = p.Id,
        //                GameId = p.GameId,
        //                PlayerId = p.PlayerId,
        //                Stones = p.Stones,
        //                IsMancala = p.IsMancala,
        //                PitPosition = p.PitPosition
        //            }).ToList();

        //            return game;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error loading game by ID: {ex.Message}", ex);
        //    }
        //}
        public async Task<Game> LoadByIdAsync(Guid id)
        {
            try
            {
                using (var context = new MancalaEntities(options))
                {
                    var tblGame = await context.tblGames
                        .Include(g => g.Pits)
                        .Include(g => g.Moves)
                        .FirstOrDefaultAsync(g => g.Id == id);

                    if (tblGame == null)
                        throw new Exception($"Game with ID {id} not found.");

                    var game = Map<tblGame, Game>(tblGame);

                  
                    game.Pits = tblGame.Pits
                        .Select(p => new Pit
                        {
                            Id = p.Id,
                            GameId = p.GameId,
                            PlayerId = p.PlayerId,  
                            Stones = p.Stones,
                            IsMancala = p.IsMancala,
                            PitPosition = p.PitPosition
                        })
                        .OrderBy(p => p.PitPosition)
                        .ToList();

     
                    Console.WriteLine("\nVerifying pit ownership on load:");
                    foreach (var pit in game.Pits)
                    {
                        var expectedOwner = pit.PitPosition <= 6 ? game.Player1Id : game.Player2Id;
                        if (pit.PlayerId != expectedOwner)
                        {
                            Console.WriteLine($"WARNING: Pit {pit.PitPosition} has incorrect ownership. " +
                                            $"Expected: {expectedOwner}, Actual: {pit.PlayerId}");
                        }
                    }

                    return game;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading game by ID: {ex.Message}", ex);
            }
        }
        public async Task<int> UpdateAsync(Game game, bool rollback = false)
        {
            try
            {
                using (var context = new MancalaEntities(options))
                {
                    // Update game state
                    var tblGame = await context.tblGames
                        .Include(g => g.Pits)
                        .Include(g => g.Moves)
                        .FirstOrDefaultAsync(g => g.Id == game.Id);

                    if (tblGame == null)
                        throw new Exception($"Game with ID {game.Id} not found.");

                    // Update game properties
                    tblGame.CurrentTurn = game.CurrentTurn;
                    tblGame.IsGameOver = game.IsGameOver;
                    tblGame.Status = game.Status;

                    // Update pit states
                    foreach (var pit in game.Pits)
                    {
                        var tblPit = tblGame.Pits.First(p => p.PitPosition == pit.PitPosition);
                        tblPit.Stones = pit.Stones;
                        tblPit.PlayerId = pit.PlayerId;
                    }

                    // Add new moves
                    foreach (var move in game.Moves.Where(m => !tblGame.Moves.Any(tm => tm.Id == m.Id)))
                    {
                        tblGame.Moves.Add(Map<Move, tblMove>(move));
                    }

                    await context.SaveChangesAsync();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating game: {ex.Message}", ex);
            }
        }

        public async Task<List<Game>> LoadAsync()
        {
            try
            {
                var tblGames = await base.LoadAsync();

                return tblGames.Select(tblGame =>
                {
                    var game = Map<tblGame, Game>(tblGame);
                    game.Pits = tblGame.Pits.Select(Map<tblPit, Pit>).ToList();
                    game.Moves = tblGame.Moves.Select(Map<tblMove, Move>).ToList();
                    return game;
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading games: {ex.Message}", ex);
            }
        }

        //public async Task<Guid> StartNewGame(Guid player1Id, Guid player2Id)
        //{
        //    try
        //    {
        //        using (var context = new MancalaEntities(options))
        //        {
        //            var player1 = await context.tblPlayers.FindAsync(player1Id);
        //            if (player1 == null)
        //            {
        //                player1 = new tblPlayer { Id = player1Id, Username = "Player1" };
        //                context.tblPlayers.Add(player1);
        //            }

        //            var player2 = await context.tblPlayers.FindAsync(player2Id);
        //            if (player2 == null)
        //            {
        //                player2 = new tblPlayer { Id = player2Id, Username = "Player2" };
        //                context.tblPlayers.Add(player2);
        //            }

        //            await context.SaveChangesAsync();

        //            var gameId = Guid.NewGuid();
        //            var tblGame = new tblGame
        //            {
        //                Id = gameId,
        //                Player1Id = player1Id,
        //                Player2Id = player2Id,
        //                CurrentTurn = player1Id,
        //                IsGameOver = false,
        //                Status = "Active",
        //                DateCreated = DateTime.Now
        //            };

        //            context.tblGames.Add(tblGame);

        //            var pits = CreatePits(gameId, player1Id, player2Id);
        //            var tblPits = pits.Select(p => new tblPit
        //            {
        //                Id = p.Id,
        //                GameId = p.GameId,
        //                PlayerId = p.PlayerId,
        //                Stones = p.Stones,
        //                IsMancala = p.IsMancala,
        //                PitPosition = p.PitPosition
        //            }).ToList();

        //            context.tblPits.AddRange(tblPits);
        //            await context.SaveChangesAsync();

        //            return tblGame.Id;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error starting new game: {ex.Message}", ex);
        //    }
        //}
        public async Task<Guid> StartNewGame(Guid player1Id, Guid player2Id)
        {
            using (var context = new MancalaEntities(options))
            {
                try
                {
                    Console.WriteLine($"\nStarting new game");
                    Console.WriteLine($"Player 1 ID: {player1Id}");
                    Console.WriteLine($"Player 2 ID: {player2Id}");

                    // Create and save players
                    var player1 = await context.tblPlayers.FindAsync(player1Id)
                                  ?? new tblPlayer { Id = player1Id, Username = "Player 1" };
                    var player2 = await context.tblPlayers.FindAsync(player2Id)
                                  ?? new tblPlayer { Id = player2Id, Username = "Player 2" };

                    context.tblPlayers.AddRange(player1, player2);
                    await context.SaveChangesAsync();

                    // Create game with correct initial setup
                    var gameId = Guid.NewGuid();
                    var game = new tblGame
                    {
                        Id = gameId,
                        Player1Id = player1Id,  // Ensure Player 1 ID is set correctly
                        Player2Id = player2Id,  // Ensure Player 2 ID is set correctly
                        CurrentTurn = player1Id,
                        IsGameOver = false,
                        Status = "Active",
                        DateCreated = DateTime.UtcNow
                    };

                    context.tblGames.Add(game);

                    // Create pits with explicit ownership
                    var pits = new List<tblPit>();

                    // Player 1's regular pits (0-5)
                    for (int i = 0; i < 6; i++)
                    {
                        var pit = new tblPit
                        {
                            Id = Guid.NewGuid(),
                            GameId = gameId,
                            PitPosition = i,
                            Stones = 4,
                            IsMancala = false,
                            PlayerId = player1Id  // Explicitly set to Player 1
                        };
                        pits.Add(pit);
                        Console.WriteLine($"Created Player 1 pit {i} with ID {player1Id}");
                    }

                    // Player 1's Mancala (6)
                    pits.Add(new tblPit
                    {
                        Id = Guid.NewGuid(),
                        GameId = gameId,
                        PitPosition = 6,
                        Stones = 0,
                        IsMancala = true,
                        PlayerId = player1Id  // Explicitly set to Player 1
                    });

                    // Player 2's regular pits (7-12)
                    for (int i = 7; i < 13; i++)
                    {
                        var pit = new tblPit
                        {
                            Id = Guid.NewGuid(),
                            GameId = gameId,
                            PitPosition = i,
                            Stones = 4,
                            IsMancala = false,
                            PlayerId = player2Id  // Explicitly set to Player 2
                        };
                        pits.Add(pit);
                        Console.WriteLine($"Created Player 2 pit {i} with ID {player2Id}");
                    }

                    // Player 2's Mancala (13)
                    pits.Add(new tblPit
                    {
                        Id = Guid.NewGuid(),
                        GameId = gameId,
                        PitPosition = 13,
                        Stones = 0,
                        IsMancala = true,
                        PlayerId = player2Id  // Explicitly set to Player 2
                    });

                    context.tblPits.AddRange(pits);
                    await context.SaveChangesAsync();

                    // Log final setup
                    Console.WriteLine($"\nGame created with ID: {gameId}");
                    Console.WriteLine($"Total pits created: {pits.Count}");
                    foreach (var pit in pits.OrderBy(p => p.PitPosition))
                    {
                        Console.WriteLine($"Pit {pit.PitPosition}: Owner={pit.PlayerId}, IsMancala={pit.IsMancala}");
                    }

                    return gameId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in StartNewGame: {ex.Message}");
                    throw;
                }
            }
        }
        //private void ValidateGameState(Game game)
        //{
        //    Console.WriteLine("\nValidating game state...");

        //    // Check basic game properties
        //    Console.WriteLine($"Game ID: {game.Id}");
        //    Console.WriteLine($"Player 1: {game.Player1Id}");
        //    Console.WriteLine($"Player 2: {game.Player2Id}");
        //    Console.WriteLine($"Current Turn: {game.CurrentTurn}");

        //    // Validate pit count
        //    if (game.Pits.Count != 14)
        //    {
        //        throw new Exception($"Invalid number of pits: {game.Pits.Count}");
        //    }

        //    // Validate pit ownership
        //    var player1Pits = game.Pits.Where(p => p.PlayerId == game.Player1Id).ToList();
        //    var player2Pits = game.Pits.Where(p => p.PlayerId == game.Player2Id).ToList();

        //    Console.WriteLine($"Player 1 pit count: {player1Pits.Count}");
        //    Console.WriteLine($"Player 2 pit count: {player2Pits.Count}");

        //    // Print pit details
        //    foreach (var pit in game.Pits.OrderBy(p => p.PitPosition))
        //    {
        //        Console.WriteLine($"Pit {pit.PitPosition}: Owner={pit.PlayerId}, Stones={pit.Stones}, IsMancala={pit.IsMancala}");
        //    }

        //    Console.WriteLine("Game state validation complete\n");
        //}

        public async Task ComputerPlay(Game game)
        {
            Console.WriteLine($"\nComputer Play Starting");
            Console.WriteLine($"Current Turn: {game.CurrentTurn}");
            Console.WriteLine($"Computer (Player 2) ID: {game.Player2Id}");

            try
            {
                if (game.CurrentTurn != game.Player2Id)
                {
                    throw new InvalidOperationException("It's not the computer's turn.");
                }

                // Get all valid moves and evaluate them
                var validMoves = game.Pits
                    .Where(p => p.PlayerId == game.Player2Id && p.Stones > 0 && !p.IsMancala)
                    .ToList();

                if (!validMoves.Any())
                {
                    throw new InvalidOperationException("No valid moves available for computer.");
                }

                // Evaluate each possible move and pick the best one
                Pit bestPit = null;
                int bestScore = -1;

                foreach (var pit in validMoves)
                {
                    var score = EvaluateMove(game, pit);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPit = pit;
                    }
                }

                if (bestPit == null)
                {
                    bestPit = validMoves.First(); // Fallback to first valid move
                }

                Console.WriteLine($"\nComputer selected pit {bestPit.PitPosition} with {bestPit.Stones} stones (score: {bestScore})");

                var moveManager = new MoveManager(options);
                bool isExtraTurn = await moveManager.ApplyMove(game, bestPit.PitPosition);

                if (IsGameOver(game))
                {
                    game.IsGameOver = true;
                    game.Status = "Game Over";
                    Console.WriteLine("Game is now over");
                }
                else if (!isExtraTurn)
                {
                    game.CurrentTurn = game.Player1Id;
                    Console.WriteLine("Switching turn to Player 1");
                }
                else
                {
                    Console.WriteLine("Computer gets another turn");
                }

                await UpdateAsync(game);
                Console.WriteLine("Computer play completed successfully\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during computer's turn: {ex.Message}");
                throw;
            }
        }

        private int EvaluateMove(Game game, Pit pit)
        {
            int score = 0;

            // Base score is number of stones that will be distributed
            score += pit.Stones;

            // Bonus for moves that might earn an extra turn
            if (pit.Stones == (13 - pit.PitPosition))
            {
                score += 5; // Bonus for landing in Mancala
            }

            // Bonus for moves that capture opponent's stones
            int landingPosition = (pit.PitPosition + pit.Stones) % 14;
            if (landingPosition >= 7 && landingPosition < 13) // Lands in computer's pits
            {
                var landingPit = game.Pits[landingPosition];
                if (landingPit.Stones == 0) // Empty pit
                {
                    var oppositePit = game.Pits[12 - (landingPosition - 7)];
                    if (oppositePit.Stones > 0)
                    {
                        score += oppositePit.Stones + 3; // Bonus for capture
                    }
                }
            }

            // Bonus for protecting stones from capture
            if (pit.Stones > 0 && pit.Stones <= 3)
            {
                score -= 1; // Small penalty for leaving few stones
            }

            // Bonus for moves from pits closer to Mancala
            score += (pit.PitPosition - 7); // Higher score for pits closer to Mancala

            return score;
        }




        public List<Pit> CreatePits(Guid gameId, Guid player1Id, Guid player2Id)
        {
            var pits = new List<Pit>();

            // Player 1 Pits (Pits 0 to 5)
            for (int i = 0; i < 6; i++)
            {
                pits.Add(new Pit
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    PlayerId = player1Id,
                    Stones = 4,
                    IsMancala = false,
                    PitPosition = i
                });
            }

            // Player 1 Mancala (Pit 6)
            pits.Add(new Pit
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                PlayerId = player1Id,
                Stones = 0,
                IsMancala = true,
                PitPosition = 6
            });

            // Player 2 Pits (Pits 7 to 12)
            for (int i = 7; i <= 12; i++)
            {
                pits.Add(new Pit
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    PlayerId = player2Id,
                    Stones = 4,
                    IsMancala = false,
                    PitPosition = i
                });
            }

            // Player 2 Mancala (Pit 13)
            pits.Add(new Pit
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                PlayerId = player2Id,
                Stones = 0,
                IsMancala = true,
                PitPosition = 13
            });

            return pits;
        }

        public async Task<Game> CreateBoard(Guid gameId, Guid player1Id, Guid player2Id)
        {
            try
            {
                using (var context = new MancalaEntities(options))
                {
                    var tblGame = new tblGame
                    {
                        Id = gameId,
                        Player1Id = player1Id,
                        Player2Id = player2Id,
                        IsGameOver = false,
                        Status = "In Progress",
                        CurrentTurn = player1Id,
                        DateCreated = DateTime.Now
                    };

                    context.tblGames.Add(tblGame);
                    await context.SaveChangesAsync();

                    var pits = CreatePits(gameId, player1Id, player2Id);
                    var tblPits = pits.Select(p => new tblPit
                    {
                        Id = p.Id,
                        GameId = p.GameId,
                        PlayerId = p.PlayerId,
                        Stones = p.Stones,
                        IsMancala = p.IsMancala,
                        PitPosition = p.PitPosition,
                    }).ToList();

                    context.tblPits.AddRange(tblPits);
                    await context.SaveChangesAsync();

                    var game = new Game
                    {
                        Id = tblGame.Id,
                        Player1Id = tblGame.Player1Id,
                        Player2Id = tblGame.Player2Id,
                        IsGameOver = tblGame.IsGameOver,
                        Status = tblGame.Status,
                        CurrentTurn = tblGame.CurrentTurn,
                        DateCreated = tblGame.DateCreated,
                        Pits = pits,
                        Moves = new List<Move>()
                    };

                    Console.WriteLine($"Created {game.Pits.Count} pits for game ID: {game.Id}");

                    return game;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw new Exception($"Error creating board: {ex.Message}", ex);
            }
        }

        public async Task<string> CheckWinner(Guid gameId)
        {
            try
            {
                var game = await LoadByIdAsync(gameId);

                if (IsGameOver(game))
                {
                    int player1Score = game.Pits[6].Stones + game.Pits.Take(6).Sum(p => p.Stones);
                    int player2Score = game.Pits[13].Stones + game.Pits.Skip(7).Take(6).Sum(p => p.Stones);

                    string winner = player1Score > player2Score ? "Player 1 Wins!" :
                                    player2Score > player1Score ? "Player 2 Wins!" : "It's a Tie!";

                    game.IsGameOver = true;
                    game.Status = winner;

                    await UpdateAsync(game);
                    return winner;
                }

                return "Game is still ongoing.";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking winner: {ex.Message}", ex);
            }
        }

        public bool IsGameOver(Game game)
        {
            return !game.Pits.Take(6).Any(p => p.Stones > 0) ||
                   !game.Pits.Skip(7).Take(6).Any(p => p.Stones > 0);
        }

        //public async Task ComputerPlay(Game game)
        //{
        //    Console.WriteLine($"Computer Play. Current player: {game.CurrentTurn}, Player 2 (computer): {game.Player2Id}");
        //    try
        //    {
        //        if (game.CurrentTurn != game.Player2Id)
        //        {
        //            throw new InvalidOperationException("It's not the computer's turn.");
        //        }

        //        var validPit = game.Pits
        //            .Where(p => p.PlayerId == game.Player2Id && p.Stones > 0 && !p.IsMancala)
        //            .OrderBy(p => p.PitPosition)
        //            .FirstOrDefault();
        //        Console.WriteLine("Current board state:");
        //        foreach (var pit in game.Pits)
        //        {
        //            Console.WriteLine($"Pit {pit.PitPosition}: {pit.Stones} stones, Player: {pit.PlayerId}");
        //        }

        //        if (validPit == null)
        //        {
        //            throw new InvalidOperationException("No valid move available for the computer.");
        //        }

        //        Console.WriteLine($"Valid pit found: Pit {validPit.PitPosition} with {validPit.Stones} stones");

        //        Console.WriteLine($"Board before move: {string.Join(", ", game.Pits.Select(p => $"{p.PitPosition}: {p.Stones}"))}");

        //        var moveManager = new MoveManager(options);
        //        bool isExtraTurn = await moveManager.ApplyMove(game, validPit.PitPosition);

        //        Console.WriteLine($"Board after move: {string.Join(", ", game.Pits.Select(p => $"{p.PitPosition}: {p.Stones}"))}");

        //        if (IsGameOver(game))
        //        {
        //            game.IsGameOver = true;
        //            game.Status = "Game Over";
        //            Console.WriteLine("The game is over.");
        //        }
        //        else
        //        {
        //            if (!isExtraTurn)
        //            {
        //                game.CurrentTurn = game.Player1Id;
        //            }
        //        }

        //        await UpdateAsync(game);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error during computer's turn: {ex.Message}");
        //        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        //        throw new Exception($"Error during computer's turn: {ex.Message}", ex);
        //    }
        //}
    }
}