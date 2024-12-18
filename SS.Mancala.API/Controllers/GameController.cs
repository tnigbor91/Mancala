using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SS.Mancala.API.Controllers;
using SS.Mancala.BL;
using SS.Mancala.BL.Models;
using SS.Mancala.PL.Data;
using System.Diagnostics;

[Route("api/[controller]")]
[ApiController]
public class GameController : GenericController<Game, GameManager, MancalaEntities>
{
    private readonly MoveManager moveManager;
    private readonly DbContextOptions<MancalaEntities> _options;
    public GameController(ILogger<GameController> logger, DbContextOptions<MancalaEntities> options)
        : base(logger, options)
    {
        this.options = options;
        this.moveManager = new MoveManager();
    }





    [HttpPost("{id}/move/{pitPosition}")]
    public async Task<ActionResult> MakeMove(Guid id, int pitPosition)
    {
        try
        {
            Console.WriteLine($"Making move for game {id}, pit {pitPosition}");
            var gameManager = (GameManager)manager;
            var game = await gameManager.LoadByIdAsync(id);

            if (game == null)
            {
                return NotFound($"Game with ID {id} not found.");
            }

            Console.WriteLine($"Before move: {game.Pits[pitPosition].Stones} stones");
            var isExtraTurn = await moveManager.ApplyMove(game, pitPosition);
            Console.WriteLine($"After move: {game.Pits[pitPosition].Stones} stones");

            // Add move record
            var move = new Move(game.Id, game.CurrentTurn, pitPosition, game.Pits[pitPosition].Stones, game.Moves.Count + 1)
            {
                IsExtraTurn = isExtraTurn
            };
            game.Moves.Add(move);

            // Save changes
            await gameManager.UpdateAsync(game);

            // Return updated game state
            return Ok(new
            {
                Success = true,
                ExtraTurn = isExtraTurn,
                CurrentTurn = game.CurrentTurn,
                IsGameOver = game.IsGameOver,
                Status = game.Status,
                Pits = game.Pits.Select(p => new
                {
                    PitPosition = p.PitPosition,
                    Stones = p.Stones,
                    IsMancala = p.IsMancala,
                    PlayerId = p.PlayerId
                }).ToList(),
                Moves = game.Moves.Select(m => new
                {
                    MoveNumber = m.MoveNo,
                    PlayerId = m.PlayerId,
                    PitPosition = m.SourcePit,
                    Stones = m.StonesMoved,
                    IsExtraTurn = m.IsExtraTurn
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MakeMove: {ex}");
            return StatusCode(500, ex.Message);
        }
    }




    /// <summary>
    /// Starts a new game with two players.
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<Game>> StartNewGame()
    {
        try
        {
            var player1Id = Guid.NewGuid();
            var player2Id = Guid.NewGuid();
            var gameId = await ((GameManager)manager).StartNewGame(player1Id, player2Id);

            var createdGame = await ((GameManager)manager).LoadByIdAsync(gameId);

            if (createdGame == null || createdGame.Pits == null || !createdGame.Pits.Any())
            {
                return StatusCode(500, "Game creation failed, pits were not initialized.");
            }

            return Ok(createdGame);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error creating game: {ex}");
            return StatusCode(500, $"Failed to create game: {ex.Message}");
        }
    }



    /// <summary>
    /// Gets the current state of the game board.
    /// </summary>
    [HttpGet("{id}/board")]
    public async Task<ActionResult> GetBoard(Guid id)
    {
        try
        {
            var game = await ((GameManager)manager).LoadByIdAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found.");
            }

       
            var response = new
            {
                GameId = game.Id,
                Player1Id = game.Player1Id,
                Player2Id = game.Player2Id,
                CurrentTurn = game.CurrentTurn,
                IsGameOver = game.IsGameOver,
                Status = game.Status
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetBoard error: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }



    [HttpGet("{id}/full")]
    public async Task<ActionResult> GetFullGameState(Guid id)
    {
        try
        {
            var game = await ((GameManager)manager).LoadByIdAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found.");
            }

            var pits = game.Pits.Select(p => new
            {
                PitPosition = p.PitPosition,
                Stones = p.Stones,
                IsMancala = p.IsMancala,
                PlayerId = p.PlayerId
            }).ToList();

            var response = new
            {
                GameId = game.Id,
                Player1Id = game.Player1Id,
                Player2Id = game.Player2Id,
                CurrentTurn = game.CurrentTurn,
                IsGameOver = game.IsGameOver,
                Status = game.Status,
                Pits = pits
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }


    /// <summary>
    /// Retrieves the pits for the specified game.
    /// </summary>
    /// <param name="id">The ID of the game.</param>
    /// <returns>A list of pits for the game, including details such as PitPosition, Stones, IsMancala, and PlayerId.</returns>
    /// <response code="200">Returns the list of pits for the game.</response>
    /// <response code="404">If the game with the specified ID is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("{id}/pits")]
    public async Task<ActionResult> GetPits(Guid id)
    {
        try
        {
            var game = await ((GameManager)manager).LoadByIdAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found.");
            }

            var pits = game.Pits.Select(p => new
            {
                PitPosition = p.PitPosition,
                Stones = p.Stones,
                IsMancala = p.IsMancala,
                PlayerId = p.PlayerId
            }).ToList();

            // Log the pits and their PlayerId for debugging
            Console.WriteLine("Pits response:");
            pits.ForEach(p => Console.WriteLine($"Pit {p.PitPosition}: PlayerId={p.PlayerId}, Stones={p.Stones}"));

            return Ok(pits);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetPits error: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }



    /// <summary>
    /// Checks the winner of the game.
    /// </summary>
    [HttpGet("{id}/winner")]
    public async Task<ActionResult> CheckWinner(Guid id)
    {
        try
        {
            var result = await manager.CheckWinner(id);
            return Ok(new { Success = true, Winner = result });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
    [HttpPost("{id}/computer-play")]
    public async Task<ActionResult> ComputerPlay(Guid id)
    {
        try
        {
            var game = await manager.LoadByIdAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found.");
            }

            if (game.CurrentTurn != game.Player2Id)
            {
                return BadRequest("It's not the computer's turn.");
            }

            Console.WriteLine($"Computer's turn: {game.Player2Id}");
            await manager.ComputerPlay(game);
            var move = new Move(game.Id, game.Player2Id, -1, 0, game.Moves.Count + 1); 
            game.Moves.Add(move);
            await manager.UpdateAsync(game);

            return Ok(new
            {
                Success = true,
                CurrentTurn = game.CurrentTurn,
                IsGameOver = game.IsGameOver,
                Status = game.Status,
                Pits = GetPitDetails(game.Pits),
                Moves = GetMoveDetails(game.Moves)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    private IEnumerable<object> GetPitDetails(IEnumerable<Pit> pits)
    {
        return pits.Select(p => new
        {
            GameId = p.GameId,
            PitPosition = p.PitPosition,
            Stones = p.Stones,
            IsMancala = p.IsMancala,
            PlayerId = p.PlayerId
        });
    }

    private IEnumerable<object> GetMoveDetails(IEnumerable<Move> moves)
    {
        return moves.Select(m => new
        {
            Id = m.Id,
            GameId = m.GameId,
            PlayerId = m.PlayerId,
            SourcePit = m.SourcePit,
            StonesMoved = m.StonesMoved,
            TimeStamp = m.TimeStamp,
            IsExtraTurn = m.IsExtraTurn,
            MoveNo = m.MoveNo
        });
    }



    [HttpPost("{id}/save")]
    public async Task<ActionResult> SaveGame(Guid id)
    {
        try
        {

            var game = await manager.LoadByIdAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found.");
            }


            await manager.UpdateAsync(game);

            return Ok(new { Success = true, Message = "Game saved successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    //error
    [HttpGet("{id}/resume")]
    public async Task<ActionResult> ResumeGame(Guid id)
    {
        try
        {

            var game = await manager.LoadByIdAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found.");
            }


            List<Pit> board = game.Pits?.ToList() ?? new List<Pit>();


            return Ok(new
            {
                GameId = game.Id,
                Player1Id = game.Player1Id,
                Player2Id = game.Player2Id,
                CurrentTurn = game.CurrentTurn,
                IsGameOver = game.IsGameOver,
                Status = game.Status,
                Board = board.Select(p => new
                {
                    PitPosition = p.PitPosition,
                    Stones = p.Stones,
                    IsMancala = p.IsMancala,
                    PlayerId = p.PlayerId
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }



    }
}






    

    









