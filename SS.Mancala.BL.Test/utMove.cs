using SS.Mancala.BL.Models;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS.Mancala.Utility;

namespace SS.Mancala.BL.Test
{
    [TestClass]
    public class utMove : utBase
    {
        private Move _move;
        private Game _game;
        private Guid _gameId;
        private Guid _player1Id;
        private Guid _player2Id;
        private MoveManager moveManager = new MoveManager();

        [TestInitialize]
        public void Initialize()
        {
            _gameId = Guid.NewGuid();
            _player1Id = Guid.NewGuid();
            _player2Id = Guid.NewGuid();


            // Initialize the board with pits
            _game = new Game
            {
                Id = _gameId,
                Player1Id = _player1Id,
                Player2Id = _player2Id,
                CurrentTurn = _player1Id,
                Pits = new List<Pit>
        {
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 0, IsMancala = false, PlayerId = _player1Id },
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 1, IsMancala = false, PlayerId = _player1Id },
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 2, IsMancala = false, PlayerId = _player1Id },
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 3, IsMancala = false, PlayerId = _player1Id }, 
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 4, IsMancala = false, PlayerId = _player1Id },
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 5, IsMancala = false, PlayerId = _player1Id },
            new Pit { Id = Guid.NewGuid(), Stones = 0, PitPosition = 6, IsMancala = true, PlayerId = _player1Id }, // Player 1 Mancala
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 7, IsMancala = false, PlayerId = _player2Id },
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 8, IsMancala = false, PlayerId = _player2Id },
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 9, IsMancala = false, PlayerId = _player2Id },
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 10, IsMancala = false, PlayerId = _player2Id },
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 11, IsMancala = false, PlayerId = _player2Id },
            new Pit { Id = Guid.NewGuid(), Stones = 4, PitPosition = 12, IsMancala = false, PlayerId = _player2Id },
            new Pit { Id = Guid.NewGuid(), Stones = 0, PitPosition = 13, IsMancala = true, PlayerId = _player2Id } // Player 2 Mancala
        }
            };

            Console.WriteLine("Initial board state:");
            foreach (var pit in _game.Pits)
            {
                Console.WriteLine($"Pit {pit.PitPosition}: {pit.Stones} stones");
            }

        }
        [TestMethod]
        public async Task ExportDataTest()
        {
            var entities = await new MoveManager(options).LoadAsync().ConfigureAwait(false);
            string[] columns = { "Id", "GameId", "PlayerId", "SourcePit", "StonesMoved", "TimeStamp", "IsExtraTurn", "MoveNo" };
            var data = MoveManager.ConvertData(entities, columns);
            Excel.Export("moves.xlsx", data);
        }

        [TestMethod]
        public async Task ApplyMoveTest()
        {
            // Arrange
            Initialize(); // Reset board state

            // Act
            await moveManager.ApplyMove(_game, 0); // Player 1 moves from pit 0

            // Assert: Verify the state of the board after the move
            Assert.AreEqual(0, _game.Pits[0].Stones, "Source pit should be empty after move.");
            Assert.AreEqual(5, _game.Pits[1].Stones, "Pit 1 should have 5 stones after the move.");
            Assert.AreEqual(5, _game.Pits[2].Stones, "Pit 2 should have 5 stones after the move.");
            Assert.AreEqual(5, _game.Pits[3].Stones, "Pit 3 should have 5 stones after the move.");
            Assert.AreEqual(5, _game.Pits[4].Stones, "Pit 4 should have 5 stones after the move.");
        }

        [TestMethod]
        public async Task InvalidPitSelectionTest()
        {
            // Arrange
            Initialize(); // Reset board state

            // Set current turn to Player 1, but try to play from a Player 2 pit
            _game.CurrentTurn = _player1Id;
            int invalidPitPosition = 10; // Assuming pit 10 belongs to Player 2, making it invalid for Player 1

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await moveManager.ApplyMove(_game, invalidPitPosition);
            }, "Expected an InvalidOperationException when attempting to play from a pit that does not belong to the current player.");
        }




        [TestMethod]
        public async Task NotEnoughStonesTest()
        {
            // Arrange
            Initialize(); // Reset board state
            _game.Pits[0].Stones = 0; // Set pit 0 to have no stones

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await moveManager.ApplyMove(_game, 0);
            }, "Expected an InvalidOperationException when attempting to play from an empty pit.");
        }




        [TestMethod]
        public void TestApplyMove_NoExtraTurn()
        {
            // Arrange
            Initialize(); // Reset board state
            int pitToPlayFrom = 1; // This should not end in Mancala, thus no extra turn

            // Act: Apply the move using Game, Board, and pit position
            bool isExtraTurn = moveManager.ApplyMove(_game, pitToPlayFrom).Result;

            // Assert: Player should not get an extra turn if the last stone doesn’t land in their Mancala
            Assert.IsFalse(isExtraTurn, "Player should not get an extra turn if the last stone doesn't land in their Mancala.");
        }

        [TestMethod]
        public void TestApplyMove_ExtraTurn()
        {
            // Arrange
            Initialize(); // Reset board state
            int pitToPlayFrom = 2; // Assuming this ends with the last stone in Player 1's Mancala

            // Act: Apply the move
            bool isExtraTurn = moveManager.ApplyMove(_game, pitToPlayFrom).Result;

            // Assert: Player should get an extra turn if the last stone lands in their Mancala
            Assert.IsTrue(isExtraTurn, "Player should get an extra turn when the last stone lands in their Mancala.");
        }


    }
}
