using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.Mancala.PL.Data;

namespace SS.Mancala.BL.Test
{

    [TestClass]
    public class utBase
    {
        protected MancalaEntities dc;  // declare the DataContext
        protected IDbContextTransaction transaction;
        private IConfigurationRoot _configuration;
        protected DbContextOptions<MancalaEntities> options;

        public utBase()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();

            options = new DbContextOptionsBuilder<MancalaEntities>()
                .UseSqlServer(_configuration.GetConnectionString("MancalaConnection"))
                //  .UseLazyLoadingProxies()
                .Options;

            dc = new MancalaEntities(options);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            transaction = dc.Database.BeginTransaction();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            transaction.Rollback();
            transaction.Dispose();
            dc = null;
        }


    }
}
