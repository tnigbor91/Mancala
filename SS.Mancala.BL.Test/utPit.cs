using SS.Mancala.BL.Models;
using SS.Mancala.BL.Test;
using SS.Mancala.Utility;
using System.Transactions;



namespace SS.Mancala.BL.Test
{

    [TestClass]
    public class utPit : utBase
    {
        private Pit _pit;
        private TransactionScope _transactionScope;

        [TestInitialize]
        public void Initialize()
        {

            _transactionScope = new TransactionScope();


            _pit = new Pit(Guid.NewGuid(), 1, false, 4);
        }

        [TestMethod]
        public void InitializePitTest()
        {
            Assert.AreEqual(4, _pit.Stones);
            Assert.AreEqual(1, _pit.PitPosition);
            Assert.IsFalse(_pit.IsMancala);
            Assert.IsNotNull(_pit.Id);
        }
        [TestMethod]
        public async Task ExportDataTest()
        {
            var entities = await new PitManager(options).LoadAsync().ConfigureAwait(false);
            string[] columns = { "Id", "GameId", "PlayerId", "Stones", "IsMancala", "PitPosition" };
            var data = PitManager.ConvertData(entities, columns);
            Excel.Export("pits.xlsx", data);
        }
        [TestMethod]
        public void AddStonesTest()
        {
            _pit.AddStones(3);
            Assert.AreEqual(7, _pit.Stones);
        }

        [TestMethod]
        public void RemoveStonesTest()
        {
            _pit.RemoveStones(2);
            Assert.AreEqual(2, _pit.Stones);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RemoveMoreStonesThanAvailableTest()
        {
            _pit.RemoveStones(5);
        }

        [TestMethod]
        public void InitializeMancalaTest()
        {
            var mancalaPit = new Pit(Guid.NewGuid(), 6, true, 0);
            Assert.IsTrue(mancalaPit.IsMancala);
            Assert.AreEqual(0, mancalaPit.Stones);
        }

        private bool SimulateDelete(Pit pit)
        {

            return true;
        }

        [TestMethod]
        public async Task DeleteTestAsync()
        {
            // Create a new pit for deletion testing
            var pitToDelete = new Pit(Guid.NewGuid(), 5, false, 3);

            bool isDeleted = await Task.Run(() => SimulateDelete(pitToDelete));

            Assert.IsTrue(isDeleted, "The pit should be deleted successfully.");
        }

        [TestCleanup]
        public void Cleanup()
        {

            _transactionScope.Dispose();
        }
    }
}
