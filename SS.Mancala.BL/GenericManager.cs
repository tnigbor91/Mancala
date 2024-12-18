



using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SS.Mancala.BL
{
    public abstract class GenericManager<T> where T : class, IEntity
    {
        protected DbContextOptions<MancalaEntities> options;
        protected readonly ILogger logger;

        public GenericManager(ILogger logger,
                              DbContextOptions<MancalaEntities> options)
        {
            this.logger = logger;
            this.options = options;
        }
        public GenericManager(DbContextOptions<MancalaEntities> options)
        {
            this.options = options;
        }
        public GenericManager()
        {
            int x;
            x = 5;
        }
        public static string[,] ConvertData<U>(List<U> entities, string[] columns) where U : class
        {
            try
            {
                string[,] data = new string[entities.Count + 1, columns.Length];
                int counter = 0;
                for (int i = 0; i < columns.Length; i++)
                {
                    data[counter, i] = columns[i];
                }
                counter++;

                foreach (var entity in entities)
                {
                    for (int i = 0; i < columns.Length; i++)
                    {
                        data[counter, i] = entity.GetType().GetProperty(columns[i]).GetValue(entity, null).ToString();
                    }
                    counter++;
                }

                return data;

            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="rollback">Rollback</param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(T entity, bool rollback = false)
        {
            try
            {
                int results = 0;

                using (MancalaEntities dc = new MancalaEntities(options))
                {
                    IDbContextTransaction transaction = null;
                    if (rollback) transaction = dc.Database.BeginTransaction();

                    dc.Entry(entity).State = EntityState.Modified;

                    results = dc.SaveChanges();

                    if (rollback) transaction.Rollback();
                }

                return results;
            }
            catch (Exception)
            {
                if (logger != null)
                    logger.LogWarning($"Update {typeof(T).Name}s - GenericManager");
                throw;
            }

        }
        public async Task<Guid> InsertAsync(T entity,
                                       Expression<Func<T, bool>> expression = null,
                                       bool rollback = false)
        {
            try
            {
                Guid result = Guid.Empty;

                using (MancalaEntities dc = new MancalaEntities(options))
                {
                    if ((expression == null) || ((expression != null) && (!dc.Set<T>().Any(expression))))
                    {
                        IDbContextTransaction transaction = null;
                        if (rollback)
                        {
                            transaction = dc.Database.BeginTransaction();
                        }

                        entity.Id = Guid.NewGuid();
                        dc.Set<T>().Add(entity);

                        await dc.SaveChangesAsync();

                        if (rollback && transaction != null)
                        {
                            await transaction.RollbackAsync();
                        }

                        result = entity.Id;
                    }
                    else
                    {
                        logger?.LogWarning("Row already exists for entity of type {EntityType}", typeof(T).Name);
                        throw new Exception("Row already exists.");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                logger?.LogWarning($"Insert {typeof(T).Name}s - GenericManager: {ex.Message}");
                throw new Exception($"An error occurred while inserting the entity: {ex.Message}", ex);
            }
        }

        public async Task<int> DeleteAsync(Guid id, bool rollback = false)
        {
            try
            {
                int results = 0;

                using (MancalaEntities dc = new MancalaEntities(options))
                {
                    IDbContextTransaction transaction = null;
                    if (rollback) transaction = dc.Database.BeginTransaction();

                    T row = dc.Set<T>().FirstOrDefault(t => t.Id == id);

                    if (row != null)
                    {
                        dc.Set<T>().Remove(row);
                        results = dc.SaveChanges();
                        if (rollback) transaction.Rollback();
                    }
                    else
                    {
                        throw new Exception("Row does not exist.");
                    }
                }

                return results;
            }
            catch (Exception)
            {
                if (logger != null)
                    logger.LogWarning($"Delete {typeof(T).Name}s - GenericManager");
                throw;
            }

        }

        public async Task<List<T>> LoadAsync(Expression<Func<T, bool>> filter = null)
        {
            try
            {
                if (filter == null) filter = e => true;

                if (logger != null)
                    logger.LogWarning($"Get {typeof(T).Name}s - GenericManager");


                IQueryable<T> rows = new MancalaEntities(options)
                .Set<T>()
                .Where(filter);

                return await rows.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public V Map<U, V>(U objfrom)
        {
            V objTo = (V)Activator.CreateInstance(typeof(V));

            var ToProperties = objTo.GetType().GetProperties();
            var FromProperties = objfrom.GetType().GetProperties();

            ToProperties.ToList().ForEach(o =>
            {
                var fromp = FromProperties.FirstOrDefault(x => x.Name == o.Name
                                                          && x.PropertyType == o.PropertyType);

                if (fromp != null)
                    o.SetValue(objTo, fromp.GetValue(objfrom));
            });

            return objTo;
        }

        public virtual async Task<T> LoadByIdAsync(Guid id)
        {
            using (var context = new MancalaEntities())
            {
                var entity = await context.Set<T>().FindAsync(id);

                if (entity == null)
                {
                    throw new Exception($"{typeof(T).Name} with Id {id} not found.");
                }

                return entity;
            }
        }

    }

}

