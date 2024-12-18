using SS.Mancala.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Mancala.BL
{
    public class LoginFailureException : Exception
    {
        public LoginFailureException() : base("Cannot log in with these credentials.  Your IP address has been saved.")
        {
        }

        public LoginFailureException(string message) : base(message)
        {
        }
    }

    public class UserManager : GenericManager<tblUser>
    {
        public UserManager(DbContextOptions<MancalaEntities> options) : base(options) { }

        public UserManager(ILogger logger, DbContextOptions<MancalaEntities> options) : base(logger, options) { }
        public UserManager() { }

        public static string GetHash(string Password)
        {
            using (var hasher = new System.Security.Cryptography.SHA1Managed())
            {
                var hashbytes = System.Text.Encoding.UTF8.GetBytes(Password);
                return Convert.ToBase64String(hasher.ComputeHash(hashbytes));
            }
        }

        public async Task Seed()
        {
            List<User> users = await LoadAsync();

            foreach (User user in users)
            {
                if (user.Password.Length != 28)
                {
                    await UpdateAsync(user);
                }
            }

            if (users.Count == 0)
            {
                // Hardcord a couple of users with hashed passwords
                await InsertAsync(new User { UserId = "bfoote", FirstName = "Brian", LastName = "Foote", Password = "maple" });
                await InsertAsync(new User { UserId = "kvicchiollo", FirstName = "Ken", LastName = "Vicchiollo", Password = "password" });
            }
        }

        public async Task<bool> LoginAsync(User user)
        {
            try
            {
                if (!string.IsNullOrEmpty(user.UserId))
                {
                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        using (MancalaEntities dc = new MancalaEntities(options))
                        {
                            tblUser userrow = dc.tblUsers.FirstOrDefault(u => u.UserId == user.UserId);

                            if (userrow != null)
                            {
                                // check the password
                                if (userrow.Password == GetHash(user.Password))
                                {
                                    // Login was successfull
                                    user.Id = userrow.Id;
                                    user.FirstName = userrow.FirstName;
                                    user.LastName = userrow.LastName;
                                    user.UserId = userrow.UserId;
                                    user.Password = userrow.Password;
                                    return true;
                                }
                                else
                                {
                                    throw new LoginFailureException("Cannot log in with these credentials.  Your IP address has been saved.");
                                }
                            }
                            else
                            {
                                throw new Exception("User could not be found.");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Password was not set.");
                    }
                }
                else
                {
                    throw new Exception("User Name was not set.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<User>> LoadAsync()
        {
            try
            {
                List<User> rows = new List<User>();
                (await base.LoadAsync())
                     .ForEach(e => rows.Add(Map<tblUser, User>(e)));

                return rows;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<User> LoadByIdAsync(Guid id)
        {
            try
            {
                tblUser row = await base.LoadByIdAsync(id);

                if (row != null)
                    return Map<tblUser, User>(row);
                else
                    throw new Exception("Row does not exist.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Guid> InsertAsync(User user, bool rollback = false)
        {
            try
            {
                Guid results = Guid.Empty;
                using (MancalaEntities dc = new MancalaEntities(options))
                {
                    // Check if username already exists - do not allow ....
                    bool inuse = dc.tblUsers.Any(u => u.UserId.Trim().ToUpper() == user.UserId.Trim().ToUpper());

                    if (inuse && rollback == false)
                    {
                        //throw new Exception("This User Name already exists.");
                    }
                    else
                    {
                        IDbContextTransaction transaction = null;
                        if (rollback) transaction = dc.Database.BeginTransaction();

                        tblUser newUser = new tblUser();

                        newUser.Id = Guid.NewGuid();
                        newUser.FirstName = user.FirstName.Trim();
                        newUser.LastName = user.LastName.Trim();
                        newUser.UserId = user.UserId.Trim();
                        newUser.Password = GetHash(user.Password.Trim());
                        Map<User, tblUser>(user);

                        user.Id = newUser.Id;

                        dc.tblUsers.Add(newUser);

                        dc.SaveChanges();
                        results = user.Id;
                        if (rollback) transaction.Rollback();
                    }
                }
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<int> UpdateAsync(User user, bool rollback = false)
        {
            try
            {
                int results = 0;

                using (MancalaEntities dc = new MancalaEntities())
                {
                    // Check if username already exists - do not allow ....
                    tblUser existingUser = dc.tblUsers.Where(u => u.UserId.Trim().ToUpper() == user.UserId.Trim().ToUpper()).FirstOrDefault();

                    if (existingUser != null && existingUser.Id != user.Id && rollback == false)
                    {
                        throw new Exception("This User Name already exists.");
                    }
                    else
                    {
                        IDbContextTransaction transaction = null;
                        if (rollback) transaction = dc.Database.BeginTransaction();

                        tblUser upDateRow = dc.tblUsers.FirstOrDefault(r => r.Id == user.Id);

                        if (upDateRow != null)
                        {
                            upDateRow.FirstName = user.FirstName.Trim();
                            upDateRow.LastName = user.LastName.Trim();
                            upDateRow.UserId = user.UserId.Trim();
                            upDateRow.Password = GetHash(user.Password.Trim());

                            dc.tblUsers.Update(upDateRow);

                            // Commit the changes and get the number of rows affected
                            results = dc.SaveChanges();

                            if (rollback) transaction.Rollback();
                        }
                        else
                        {
                            throw new Exception("Row was not found.");
                        }
                    }
                }
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<int> DeleteAsync(Guid id, bool rollback = false)
        {
            try
            {
                int results = 0;

                using (MancalaEntities dc = new MancalaEntities())
                {
                 
                    
                    {
                        IDbContextTransaction transaction = null;
                        if (rollback) transaction = dc.Database.BeginTransaction();

                        tblUser deleteRow = dc.tblUsers.FirstOrDefault(r => r.Id == id);

                        if (deleteRow != null)
                        {
                            dc.tblUsers.Remove(deleteRow);

                            // Commit the changes and get the number of rows affected
                            results = dc.SaveChanges();

                            if (rollback) transaction.Rollback();
                        }
                        else
                        {
                            throw new Exception("Row was not found.");
                        }
                    }
                }
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
