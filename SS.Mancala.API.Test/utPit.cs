using SS.Mancala.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Mancala.API.Test
{
    [TestClass]
    public class utPit : utBase
    {
        [TestMethod]
        public async Task LoadTestAsync()
        {
            //May have to adjust number
            await base.LoadTestAsync<Pit>(1);
        }

        [TestMethod]
        public async Task InsertTestAsync()
        {

            Pit pit = new Pit(Guid.NewGuid(), 1, false, 4)
            {
                PlayerId = Guid.NewGuid(),
                Stones = 4,
                IsMancala = false,
                PitPosition = 1
            };

            await base.InsertTestAsync<Pit>(pit);
        }

        [TestMethod]
        public async Task DeleteTestAsync()
        {
            var key = new KeyValuePair<string, string>("Stones", "4");
            await base.DeleteTestAsync<Pit>(key);
        }

        [TestMethod]
        public async Task UpdateTestAsync()
        {
            Pit pit = new Pit(Guid.NewGuid(), 1, false, 4);
            await base.UpdateTestAsync<Pit>(new KeyValuePair<string, string>("Stones", "4"), pit);

        }

        [TestMethod]
        public async Task LoadByIdTestAsync()
        {
            await base.LoadByIdTestAsync<Pit>(new KeyValuePair<string, string>("Stones", "4"));
        }
    }
}
