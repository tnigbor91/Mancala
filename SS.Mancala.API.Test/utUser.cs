using SS.Mancala.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Mancala.API.Test
{
    [TestClass]
    public class utUser : utBase
    {
        [TestMethod]
        public async Task LoadTestAsync()
        {
            //May have to adjust number
            await base.LoadTestAsync<Player>(2);
        }

        [TestMethod]
        public async Task InsertTestAsync()
        {

            Player player = new Player
            {
                Username = "Test",
                Score = 99,
                FirstName = "Test",
                LastName = "Test",
                Password = "Test",
            };

            await base.InsertTestAsync<Player>(player);
        }

        [TestMethod]
        public async Task DeleteTestAsync()
        {
            await base.DeleteTestAsync<User>(new KeyValuePair<string, string>("Password", "Deleting"));
        }

        [TestMethod]
        public async Task UpdateTestAsync()
        {
            Player player = new Player { Password = "Updating" };
            await base.UpdateTestAsync<Player>(new KeyValuePair<string, string>("Password", "Updating"), player);

        }

        [TestMethod]
        public async Task LoadByIdTestAsync()
        {
            await base.LoadByIdTestAsync<Game>(new KeyValuePair<string, string>("Password", "Loading"));
        }
    }
}
