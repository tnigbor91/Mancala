using Microsoft.EntityFrameworkCore;
using SS.Mancala.BL.Models;
using SS.Mancala.PL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SS.Mancala.BL
{
    public class PlayerManager : GenericManager<tblPlayer>
    {
        public PlayerManager(ILogger logger, DbContextOptions<MancalaEntities> options) : base(logger, options) { }
        public PlayerManager(DbContextOptions<MancalaEntities> options) : base(options) { }
        public PlayerManager() : base() { }

        public async Task<tblPlayer> CreateUserAsync(string username)
        {
            using (var context = new MancalaEntities(options))
            {
                var user = new tblPlayer
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    Score = 0
                };

                context.tblPlayers.Add(user);
                await context.SaveChangesAsync();
                return user;
            }
        }

        public async Task<Guid> InsertAsync(Player user, bool rollback = false)
        {
            try
            {
                Guid result = Guid.Empty;
                using (var context = new MancalaEntities(options))
                {
                    bool inUse = await context.tblPlayers.AnyAsync(u => u.Username.Trim().ToUpper() == user.Username.Trim().ToUpper());

                    if (inUse && !rollback)
                    {
                        throw new Exception("This Username already exists.");
                    }
                    else
                    {
                        var transaction = rollback ? await context.Database.BeginTransactionAsync() : null;

                        tblPlayer newUser = MapUserToTblUser(user);

                        context.tblPlayers.Add(newUser);
                        await context.SaveChangesAsync();

                        user.Id = newUser.Id;

                        if (rollback && transaction != null)
                            await transaction.RollbackAsync();

                        result = newUser.Id;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting user: " + ex.Message);
            }
        }

        public async Task<Player> GetUserByIdAsync(Guid playerId)
        {
            try
            {
                using (var context = new MancalaEntities(options))
                {
                    var tblPlayer = await context.tblPlayers.FirstOrDefaultAsync(u => u.Id == playerId);
                    if (tblPlayer == null)
                    {
                        return null;
                    }

                    return MapTblUserToUser(tblPlayer);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting user by ID: " + ex.Message);
            }
        }

        public async Task<IEnumerable<Player>> GetAllUsersAsync()
        {
            try
            {
                using (var context = new MancalaEntities(options))
                {
                    var tblPlayers = await context.tblPlayers.ToListAsync();
                    return tblPlayers.Select(MapTblUserToUser).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all users: " + ex.Message);
            }
        }

        public async Task<bool> DeleteUserAsync(Guid playerId)
        {
            try
            {
                using (var context = new MancalaEntities(options))
                {
                    var player = await context.tblPlayers.FirstOrDefaultAsync(u => u.Id == playerId);
                    if (player == null)
                    {
                        return false;
                    }

                    context.tblPlayers.Remove(player);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting user: " + ex.Message);
            }
        }

        public async Task<List<Player>> LoadAsync()
        {
            try
            {
                var tblPlayers = await base.LoadAsync();
                return tblPlayers.Select(MapTblUserToUser).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading users: " + ex.Message);
            }
        }

        private tblPlayer MapUserToTblUser(Player player)
        {
            return new tblPlayer
            {
                Id = player.Id,
                Username = player.Username,
                Score = player.Score
            };
        }

        private Player MapTblUserToUser(tblPlayer tblUser)
        {
            return new Player
            {
                Id = tblUser.Id,
                Username = tblUser.Username,
                Score = tblUser.Score
            };
        }

        public static string GetHash(string password)
        {
            using (var hasher = new System.Security.Cryptography.SHA1Managed())
            {
                var hashbytes = System.Text.Encoding.UTF8.GetBytes(password);
                return Convert.ToBase64String(hasher.ComputeHash(hashbytes));
            }
        }

        public async Task Seed()
        {
            var players = await LoadAsync();

            if (players.Count == 0)
            {
                await InsertAsync(new Player("Player"));
            }
        }

        public async Task<bool> UpdateUserAsync(tblPlayer user)
        {
            try
            {
                using (var context = new MancalaEntities(options))
                {
                    var existingPlayer = await context.tblPlayers.FirstOrDefaultAsync(u => u.Id == user.Id);
                    if (existingPlayer == null)
                    {
                        return false;
                    }

                    existingPlayer.Username = user.Username;
                    existingPlayer.Score = user.Score;

                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating user: " + ex.Message);
            }
        }
  
    }
}
