using SS.Mancala.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Mancala.API.Test
{
    [TestClass]
    public class utMove : utBase
    {
        [TestMethod]
        public async Task LoadTestAsync()
        {
            //May have to adjust number
            await base.LoadTestAsync<Move>(1);
        }

        [TestMethod]
        public async Task InsertTestAsync()
        {

            Move move = new Move(Guid.NewGuid(), Guid.NewGuid(), 1, 4, 1)
            {
                GameId = Guid.NewGuid(),
           //     PlayerId = Guid.NewGuid(),
                SourcePit = 1,
                StonesMoved = 4,
                MoveNo = 1,
                TimeStamp = DateTime.Now,
                IsExtraTurn = false
            };

            await base.InsertTestAsync<Move>(move);
        }

        [TestMethod]
        public async Task DeleteTestAsync()
        {
            var key = new KeyValuePair<string, string>("TimeStamp", "9/11/2017 12:00:00 AM");
            await base.DeleteTestAsync<Game>(key);
        }

        [TestMethod]
        public async Task UpdateTestAsync()
        {
            Move move = new Move(Guid.NewGuid(), Guid.NewGuid(), 1, 4, 1);
            await base.UpdateTestAsync<Move>(new KeyValuePair<string, string>("SourcePit", "1"), move);

        }

        [TestMethod]
        public async Task LoadByIdTestAsync()
        {
            await base.LoadByIdTestAsync<Move>(new KeyValuePair<string, string>("SourcePit", "1"));
        }
    }
}
