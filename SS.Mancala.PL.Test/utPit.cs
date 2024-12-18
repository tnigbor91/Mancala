using Microsoft.AspNetCore.SignalR;
using SS.Mancala.PL.Entities;
using System;
using System.Linq;

namespace SS.Mancala.PL.Test
{
    [TestClass]
    public class utPit : utBase<tblPit>
    {
        //[TestMethod]
        //public void LoadTest()
        //{
        //    int expected = 14;
        //    var pits = base.LoadTest();
        //    Assert.AreEqual(expected, pits.Count());

        //    Console.WriteLine(base.LoadTest().FirstOrDefault().PlayerId);
        //}

        [TestMethod]
        public void InsertTest()
        {

            int rowsAffected = InsertTest(new tblPit
            {
                Id = Guid.NewGuid(),
                GameId = base.LoadTest().FirstOrDefault().GameId,
                Stones = 4,
                IsMancala = false,
                PitPosition = 2,
                PlayerId = base.LoadTest().FirstOrDefault().PlayerId,
            });
            Assert.AreEqual(1, rowsAffected);
            DeleteTest();
        }

        [TestMethod]
        public void UpdateTest()
        {
            // Update an existing pit
            tblPit row = base.LoadTest().FirstOrDefault();
            if (row != null)
            {
                row.Stones = 12;
                row.IsMancala = false;
                int rowsAffected = UpdateTest(row);
                Assert.AreEqual(1, rowsAffected);
            }
            DeleteTest();
        }

        [TestMethod]
        public void DeleteTest()
        {

            tblPit row = base.LoadTest().FirstOrDefault();
            if (row != null)
            {
                int rowsAffected = DeleteTest(row);
                Assert.IsTrue(rowsAffected == 1);
            }
        }


        [TestCleanup]
        public void Cleanup()
        {

            var insertedPit = base.LoadTest().FirstOrDefault(p => p.PitPosition == 2 && p.Stones == 4);
            if (insertedPit != null)
            {
                DeleteTest(insertedPit);
            }
        }
    }
}
