using Castle.Core.Resource;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SS.Mancala.API.Services;
using SS.Mancala.BL;
using SS.Mancala.BL.Models;
using SS.Mancala.PL.Data;
using WebApi.Helpers;
using WebApi.Models;

namespace SS.Mancala.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : GenericController<Player, PlayerManager, MancalaEntities>
    {
        private IUserService _userService;
        private readonly ILogger<PlayerController> logger;
        private readonly DbContextOptions<MancalaEntities> options;

        public PlayerController(IUserService userService,
                              ILogger<PlayerController> logger,
                              DbContextOptions<MancalaEntities> options) : base(logger, options)
        {
            this._userService = userService;
            this.options = options;
            this.logger = logger;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
            {
                logger.LogWarning("Authentication unsuccessful for {UserId}", model.UserId);
                return BadRequest(new { message = "Username or password is incorrect" });
            }
            logger.LogWarning("Authentication successful for {UserId}", model.UserId);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }


    }
}
