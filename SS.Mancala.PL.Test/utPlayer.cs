using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SS.Mancala.PL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Mancala.PL.Test
{
    [TestClass]
    public class utPlayer : utBase<tblPlayer>
    {
        //[TestMethod]
        //public void LoadTest()
        //{
        //    int expected = 2;
        //    var Players = base.LoadTest();
        //    Assert.AreEqual(expected, Players.Count());
        //}
        [TestMethod]
        public void InsertTest()
        {
            var newUser = new tblPlayer
            {
                Id = Guid.NewGuid(),
                Username = "JoeBillings1234",
                Score = 10
            };

            int rowsAffected = InsertTest(newUser);
            Assert.AreEqual(1, rowsAffected);

            // Clean up the inserted user
            DeleteTest(newUser);
        }


        [TestMethod]
        public void UpdateTest()
        {
            tblPlayer row = base.LoadTest().FirstOrDefault();
            if (row != null)
            {
                row.Username = "Bonnie1234";
                row.Score = 12;
                int rowsAffected = UpdateTest(row);
                Assert.AreEqual(1, rowsAffected);
            }
        }

        [TestMethod]
        public int DeleteTest(tblPlayer player)
        {
            // Attach the entity if not being tracked
            if (dc.Entry(player).State == EntityState.Detached)
            {
                dc.tblPlayers.Attach(player);
            }

   
           dc.tblPlayers.Remove(player);

     
            return dc.SaveChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
           
            var players = base.LoadTest().Where(u => u.Username.StartsWith("JoeBillings"));

            foreach (var player in players)
            {
               
                int rowsAffected = base.DeleteTest(player);
                Assert.IsTrue(rowsAffected == 1, $"Failed to delete user with ID: {player.Id}");
            }
        }

    }
}
