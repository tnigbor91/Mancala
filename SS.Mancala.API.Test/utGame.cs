using SS.Mancala.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Mancala.API.Test
{
    [TestClass]
    public class utGame : utBase
    {
        [TestMethod]
        public async Task LoadTestAsync()
        {
            //May have to adjust number
            await base.LoadTestAsync<Game>(2);
        }

        [TestMethod]
        public async Task InsertTestAsync()
        {

            Game game = new Game
            {
                Player1Id = Guid.NewGuid(),
                Player2Id = Guid.NewGuid(),
                IsGameOver = false,
                Status = "test",
                CurrentTurn = Guid.NewGuid(),
                DateCreated = DateTime.Now,

            };

            await base.InsertTestAsync<Game>(game);
        }

        [TestMethod]
        public async Task DeleteTestAsync()
        {
            var key = new KeyValuePair<string, string>("Status", "Deleting");
            await base.DeleteTestAsync<Game>(key);
        }

        [TestMethod]
        public async Task UpdateTestAsync()
        {
            Game game = new Game { Status = "Updating" };
            await base.UpdateTestAsync<Game>(new KeyValuePair<string, string>("Status", "Deleting"), game);

        }

        [TestMethod]
        public async Task LoadByIdTestAsync()
        {
            await base.LoadByIdTestAsync<Game>(new KeyValuePair<string, string>("Status", "Loading"));
        }
    }
}
