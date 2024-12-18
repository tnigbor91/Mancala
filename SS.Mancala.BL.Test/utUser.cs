using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS.Mancala.BL.Models;
using SS.Mancala.BL;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SS.Mancala.PL.Data;
using SS.Mancala.Utility;

namespace SS.Mancala.BL.Test
{
    [TestClass]
    public class utUser:utBase
    {
        private PlayerManager _playerManager;
        private DbContextOptions<MancalaEntities> options;
        private MancalaEntities _context;






        [TestInitialize]
        public async Task Initialize()
        {
          
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

     
            var connectionString = configuration.GetConnectionString("MancalaConnection");

            options = new DbContextOptionsBuilder<MancalaEntities>()
                .UseSqlServer(connectionString) 
                .Options;

    
            _playerManager = new PlayerManager(options);

    
            await _playerManager.Seed();
        }
        [TestMethod]
        public async Task ExportDataTest()
        {
            var entities = await new UserManager(options).LoadAsync().ConfigureAwait(false);
            string[] columns = { "FirstName", "LastName", "UserId", "Password" };
            var data = UserManager.ConvertData(entities, columns);
            Excel.Export("users.xlsx", data);
        }
        [TestMethod]
        public async Task CreateUserTest()
        {
            // Arrange
            string username = "PlayerOne";

            // Act
            var user = await _playerManager.CreateUserAsync(username);

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Username);

            await _playerManager.DeleteUserAsync(user.Id);
        }

        [TestMethod]
        public async Task UpdateUserTest()
        {
            // Arrange
            string originalUsername = "PlayerOne";
            var user = await _playerManager.CreateUserAsync(originalUsername);
            string updatedUsername = "UpdatedPlayerOne";

            try
            {
                // Act
                user.Username = updatedUsername;
                bool isUpdated = await _playerManager.UpdateUserAsync(user);

                // Assert: Check if the update was successful
                Assert.IsTrue(isUpdated);
                var updatedUser = await _playerManager.GetUserByIdAsync(user.Id);
                Assert.AreEqual(updatedUsername, updatedUser.Username);
            }
            finally
            {
                // Cleanup: Revert the username to the original
                user.Username = originalUsername;
                await _playerManager.UpdateUserAsync(user);

              
                 await _playerManager.DeleteUserAsync(user.Id);
            }
        }


        [TestMethod]
        public async Task GetUserByIdTest()
        {
            // Arrange
            var user = await _playerManager.CreateUserAsync("PlayerTwo");

            // Act
            var fetchedUser = await _playerManager.GetUserByIdAsync(user.Id);

            // Assert
            Assert.IsNotNull(fetchedUser);
            Assert.AreEqual(user.Id, fetchedUser.Id);
            await _playerManager.DeleteUserAsync(user.Id);
        }

        [TestMethod]
        public async Task DeleteUserTest()
        {
            // Arrange
            var user = await _playerManager.CreateUserAsync("PlayerToDelete");

            // Act
            bool isDeleted = await _playerManager.DeleteUserAsync(user.Id);

            // Assert
            Assert.IsTrue(isDeleted);
            var deletedUser = await _playerManager.GetUserByIdAsync(user.Id);
            Assert.IsNull(deletedUser);
        }

        [TestMethod]
        public async Task GetAllUsersTest()
        {
            // Arrange
            await _playerManager.CreateUserAsync("PlayerOne");
            await _playerManager.CreateUserAsync("PlayerTwo");

            // Act
            var users = await _playerManager.GetAllUsersAsync();

            // Assert
            Assert.IsTrue(users.Count() >= 2);
            var userOne = users.FirstOrDefault(u => u.Username == "PlayerOne");
            var userTwo = users.FirstOrDefault(u => u.Username == "PlayerTwo");
            if (userOne != null)
            {
                await _playerManager.DeleteUserAsync(userOne.Id);
            }

            if (userTwo != null)
            {
                await _playerManager.DeleteUserAsync(userTwo.Id);
            }
        }
        [TestCleanup]
        public void Cleanup()
        {
            if (_context != null)
            {
              

                var testUsers = _context.tblPlayers;

                if (testUsers != null && testUsers.Any())
                {
                    _context.tblPlayers.RemoveRange(testUsers);
                    _context.SaveChanges();
                }
            }
        }


    }
}
