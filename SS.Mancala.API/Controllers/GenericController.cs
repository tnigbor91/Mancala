using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Helpers;

namespace SS.Mancala.API.Controllers
{
    public class GenericController<T, U, V> : ControllerBase where V : DbContext
    {
        protected DbContextOptions<V> options;
        protected readonly ILogger logger;
        protected dynamic manager;


        public GenericController(ILogger logger, DbContextOptions<V> options)

        {
            this.options = options;
            this.logger = logger;
            this.manager = (U)Activator.CreateInstance(typeof(U), logger, options);
        }



        /// <summary>
        /// Get all the entities of a particular type
        /// </summary>
        /// <returns>List of T</returns>


        [HttpGet]
        public async Task<ActionResult<IEnumerable<T>>> Get()
        {
            try
            {
                return Ok(await manager.LoadAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Get an entity for a particular type
        /// </summary>
        /// <param name="id">Id for the entity</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<T>> Get(Guid id)
        {
            try
            {
                return Ok(await manager.LoadByIdAsync(id));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Insert a new entity
        /// </summary>
        /// <param name="entity">New Entity</param>
        /// <param name="rollback">Should I roll this back?</param>
        /// <returns></returns>
       
        [HttpPost("{rollback?}")]
        public async Task<ActionResult> Post([FromBody] T entity, bool rollback = false)
        {
            try
            {
                Guid id = await manager.InsertAsync(entity, rollback);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}/{rollback?}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] T entity, bool rollback = false)
        {
            try
            {
                int rowsaffected = await manager.UpdateAsync(entity, rollback);

                // Create a small json bit
                var result = new Dictionary<string, string>();
                result.Add("rowsaffected", rowsaffected.ToString());
                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}/{rollback?}")]
        public async Task<ActionResult> Delete(Guid id, bool rollback = false)
        {
            try
            {
                int rowsaffected = await manager.DeleteAsync(id, rollback);

                // Create a small json bit
                var result = new Dictionary<string, string>();
                result.Add("rowsaffected", rowsaffected.ToString());
                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
