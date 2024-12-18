using Microsoft.EntityFrameworkCore;
using SS.Mancala.PL.Entities;
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SS.Mancala.PL.Test
{
    [TestClass]
    public class utMove : utBase<tblMove>
    {
        private Guid _insertedMoveId;
        private MancalaEntities _context;

        [TestInitialize]
        public void Initialize()
        {
           
            var optionsBuilder = new DbContextOptionsBuilder<MancalaEntities>();
            optionsBuilder.UseSqlServer("Server=server-101521081-500122975.database.windows.net;Database=bigprojectdb;User ID=500122975db;Password=Test123!"); 

            _context = new MancalaEntities(optionsBuilder.Options);
        }

        private tblGame LoadFirstGame()
        {
            return _context.tblGames.FirstOrDefault();
        }

        private tblPlayer LoadFirstUser()
        {
            return _context.tblPlayers.FirstOrDefault();
        }

        [TestMethod]
        public void LoadTest()
        {
            int expected = 197;
            var moves = _context.tblMoves.ToList(); 

            Assert.AreEqual(expected, moves.Count());
        }

        [TestMethod]
        public void InsertTest()
        {
            var game = LoadFirstGame();
            var user = LoadFirstUser();

            if (game == null || user == null)
            {
                Assert.Fail("No game or user found to associate with the move.");
                return;
            }

            _insertedMoveId = Guid.NewGuid(); // Store the Id for later deletion

            _context.tblMoves.Add(new tblMove
            {
                Id = _insertedMoveId,
                GameId = game.Id,   // Use the existing game's Id
                PlayerId = user.Id,   // Use the existing user's Id
                MoveNo = 2,
                SourcePit = 6,
                StonesMoved = 7,
                TimeStamp = DateTime.Now
            });

            int rowsAffected = _context.SaveChanges();

            // Assert: Verify that 1 row was inserted
            Assert.AreEqual(1, rowsAffected);
        }

        [TestMethod]
        public void UpdateTest()
        {
            tblMove row = _context.tblMoves.FirstOrDefault();
            if (row != null)
            {
                // Modify and update the move
                row.MoveNo = 10;
                row.SourcePit = 12;
                row.StonesMoved = 3;
                row.TimeStamp = DateTime.Now;

                _context.tblMoves.Update(row);
                int rowsAffected = _context.SaveChanges();
                Assert.AreEqual(1, rowsAffected);
            }
        }

        [TestMethod]
        public void DeleteTest()
        {
            tblMove row = _context.tblMoves.FirstOrDefault();
            if (row != null)
            {
                _context.tblMoves.Remove(row);
                int rowsAffected = _context.SaveChanges();
                Assert.IsTrue(rowsAffected == 1);
            }
        }

        //[TestCleanup]
        //public void TestCleanup()
        //{
        //    if (_insertedMoveId != Guid.Empty)
        //    {
        //        var moveToDelete = _context.tblMoves.FirstOrDefault(m => m.Id == _insertedMoveId);
        //        if (moveToDelete != null)
        //        {
        //            _context.tblMoves.Remove(moveToDelete);
        //            _context.SaveChanges();
        //        }
        //    }
        //}
    }
}
