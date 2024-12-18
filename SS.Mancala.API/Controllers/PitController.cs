using Castle.Core.Resource;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SS.Mancala.BL;
using SS.Mancala.BL.Models;
using SS.Mancala.PL.Data;
namespace SS.Mancala.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PitController : GenericController<Pit, PitManager, MancalaEntities>
    {


        public PitController(ILogger<PitController> logger,
                                DbContextOptions<MancalaEntities> options) : base(logger, options)
        {
        }
    }
}
