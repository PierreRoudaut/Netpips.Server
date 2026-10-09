using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Netpips
{
    public class Program
    {
        public static CultureInfo EnUsCulture = new CultureInfo("en-US");

        public static int Main(string[] args)
        {
            CultureInfo.CurrentCulture = EnUsCulture;
            CultureInfo.DefaultThreadCurrentCulture = EnUsCulture;
            BuildWebHost(args).Run();
            return 0;
        }

        public static IHost BuildWebHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .UseSerilog()
                .Build();
        }
    }
}