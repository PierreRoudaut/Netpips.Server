using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Netpips.Core.Model
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {

        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            //AppContext.BaseDirectory
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("NETPIPS_")
                .Build();

            var connectionString = configuration.GetConnectionString("Default");
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
