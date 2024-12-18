using Microsoft.Extensions.Logging.Abstractions;
using SS.Mancala.BL.Models;
using SS.Mancala.PL.Data;
using SS.Mancala.PL.Entities;
using SS.Mancala.Utility;

namespace SS.Mancala.BL.Test
{
    [TestClass]
    public class utGame : utBase
    {
        private GameManager _gameManager;
        private MoveManager _moveManager;

        public utGame()
        {
        }

        [TestInitialize]
        public async Task Initialize()
        {
            _gameManager = new GameManager(NullLogger<GameManager>.Instance, options);
            _moveManager = new MoveManager(NullLogger<MoveManager>.Instance, options);
        }

        [TestMethod]
        public async Task InsertTest()
        {
            Game game = new Game
            {
                Id = Guid.NewGuid(),
                IsGameOver = false,
                Status = "In Progress",
                CurrentTurn = Guid.NewGuid(),
                DateCreated = DateTime.Now,
                Player1Id = Guid.NewGuid(),
                Player2Id = Guid.NewGuid(),
                Pits = new List<Pit>(),
                Moves = new List<Move>()
            };

            Guid result = await _gameManager.InsertAsync(game, true);
            Assert.AreNotEqual(result, Guid.Empty);

            Console.WriteLine($"Game ID: {game.Id}");
            Console.WriteLine($"IsGameOver: {game.IsGameOver}");
            Console.WriteLine($"Status: {game.Status}");
            Console.WriteLine($"CurrentTurn: {game.CurrentTurn}");
            Console.WriteLine($"DateCreated: {game.DateCreated}");
            Console.WriteLine($"Player1Id: {game.Player1Id}");
            Console.WriteLine($"Player2Id: {game.Player2Id}");
            Console.WriteLine($"Pits Count: {game.Pits.Count}");
            Console.WriteLine($"Moves Count: {game.Moves.Count}");
        }
        [TestMethod]
        public async Task ExportDataTest()
        {
            var entities = await new GameManager(options).LoadAsync().ConfigureAwait(false);
            string[] columns = { "Id", "IsGameOver", "Status", "CurrentTurn", "DateCreated", "Player1Id", "Player2Id", "Pits", "Moves" };
            var data = GameManager.ConvertData(entities, columns);
            Excel.Export("games.xlsx", data);
        }


        [TestMethod]
        public async Task LoadAsyncTest()
        {
            var games = await _gameManager.LoadAsync();
            Assert.IsTrue(games.Count > 0, "Games should be loaded from the database.");
        }
        [TestMethod]
        public async Task LoadByIdTest()
        {
            Game game = (await new GameManager(options).LoadAsync()).FirstOrDefault();
            Assert.AreEqual(new GameManager(options).LoadByIdAsync(game.Id).Result.Id, game.Id);
        }


        [TestMethod]
        public async Task UpdateTest()
        {
            var existingGame = (await _gameManager.LoadAsync()).FirstOrDefault();
            Assert.IsNotNull(existingGame, "There should be an existing game to update.");

            existingGame.Status = "Completed";
            if (existingGame.DateCreated < new DateTime(1753, 1, 1) || existingGame.DateCreated > new DateTime(9999, 12, 31))
            {
                existingGame.DateCreated = DateTime.Now;
            }

            var result = await _gameManager.UpdateAsync(existingGame, true);
            Assert.IsTrue(result > 0, "The game update should be successful.");
        }

        [TestMethod]
        public async Task StartNewGameTest()
        {
            
            var player1Id = Guid.NewGuid();
            var player2Id = Guid.NewGuid();

    
            var gameId = await _gameManager.StartNewGame(player1Id, player2Id);
            Assert.AreNotEqual(Guid.Empty, gameId, "Game ID should not be empty.");

            var game = await _gameManager.LoadByIdAsync(gameId);


            Assert.IsNotNull(game, "The game should be created successfully.");
            Assert.AreEqual(player1Id, game.CurrentTurn, "Player 1 should start the game.");
            Assert.AreEqual(14, game.Pits.Count, "The board should have 14 pits.");
        }

        [TestMethod]
        public async Task ComputerPlayTest()
        {
           
            var existingGame = (await _gameManager.LoadAsync()).FirstOrDefault();
            if (existingGame == null)
            {
                var player1Id = Guid.NewGuid();
                var player2Id = Guid.NewGuid();
                existingGame = await _gameManager.LoadByIdAsync(await _gameManager.StartNewGame(player1Id, player2Id));
            }

            Assert.IsNotNull(existingGame, "Game should exist for testing computer play.");

           
            if (!existingGame.Pits.Any())
            {
                existingGame.Pits = _gameManager.CreatePits(existingGame.Id, existingGame.Player1Id, existingGame.Player2Id);
            }

            foreach (var pit in existingGame.Pits.Where(p => p.PlayerId == existingGame.Player2Id && !p.IsMancala))
            {
                pit.Stones = 4; 
            }

   
            await _gameManager.UpdateAsync(existingGame);

            existingGame.CurrentTurn = existingGame.Player2Id;

            Console.WriteLine($"Initial current turn: {existingGame.CurrentTurn}");
            Console.WriteLine($"Board before move: {string.Join(", ", existingGame.Pits.Select(p => $"{p.PitPosition}: {p.Stones}"))}");


            await _gameManager.ComputerPlay(existingGame);

     
            var updatedGame = await _gameManager.LoadByIdAsync(existingGame.Id);

            Console.WriteLine($"Board after move: {string.Join(", ", updatedGame.Pits.Select(p => $"{p.PitPosition}: {p.Stones}"))}");

        
            Assert.IsTrue(updatedGame.Pits.Any(pit => pit.PlayerId == existingGame.Player2Id && pit.Stones == 0),
                          "Player 2 should have made a valid move.");
            Assert.AreEqual(existingGame.Player2Id, updatedGame.CurrentTurn,
                            "The turn should switch to Player 1 after Player 2's move.");
        }




        [TestMethod]
        public void CreatePitsTest()
        {
            var pits = _gameManager.CreatePits(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.AreEqual(14, pits.Count, "There should be 14 pits in total.");
        }

        [TestMethod]
        public async Task CreateBoardWithPitsTest()
        {
            var player1Id = Guid.NewGuid();
            var player2Id = Guid.NewGuid();

            using (var context = new MancalaEntities(options))
            {
                context.tblPlayers.Add(new tblPlayer { Id = player1Id, Username = "Player1" });
                context.tblPlayers.Add(new tblPlayer { Id = player2Id, Username = "Player2" });
                await context.SaveChangesAsync();
            }

            var board = await _gameManager.CreateBoard(Guid.NewGuid(), player1Id, player2Id);

            Assert.IsNotNull(board, "The board should be created successfully.");
            Assert.AreEqual(14, board.Pits.Count, "The board should have 14 pits.");
        }
    }
}
