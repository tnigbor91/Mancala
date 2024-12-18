using SS.Mancala.PL.Entities;
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SS.Mancala.PL.Test
{
    [TestClass]
    public class utGame : utBase<tblGame>
    {
        private Guid _insertedGameId;

        //[TestMethod]
        //public void LoadTest()
        //{
        //    int expected =1;
        //    var games = base.LoadTest();
        //    Assert.AreEqual(expected, games.Count());
        //}

        [TestMethod]
        public void InsertTest()
        {
            _insertedGameId = Guid.NewGuid();

            int rowsAffected = InsertTest(new tblGame
            {
                Id = _insertedGameId,
                Player1Id = Guid.NewGuid(),
                Player2Id = Guid.NewGuid(),
                CurrentTurn = Guid.NewGuid(),
                Status = "Over",
                DateCreated = DateTime.Now,
            });

            Assert.AreEqual(1, rowsAffected);
        }

        [TestMethod]
        public void UpdateTest()
        {
            tblGame row = base.LoadTest().FirstOrDefault();
            if (row != null)
            {
                row.CurrentTurn = Guid.NewGuid();
                row.Status = "End";
                row.DateCreated = DateTime.Now;

                int rowsAffected = UpdateTest(row);
                Assert.AreEqual(1, rowsAffected);
            }
        }

        [TestMethod]
        public void DeleteTest()
        {
            tblGame row = base.LoadTest().FirstOrDefault(x => x.Status == "Other");
            if (row != null)
            {
                int rowsAffected = DeleteTest(row);
                Assert.IsTrue(rowsAffected == 1);
            }

        }


    
    }
}
