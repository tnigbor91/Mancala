using SS.Mancala.BL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using SS.Mancala.API.Controllers;
using SS.Mancala.PL.Data;
using SS.Mancala.BL;

namespace SS.Mancala.MAUI
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoveController : GenericController<Move, MoveManager, MancalaEntities>
    {


        public MoveController(ILogger<MoveController> logger,
                                DbContextOptions<MancalaEntities> options) : base(logger, options)
        {
        }

      
    }
}
