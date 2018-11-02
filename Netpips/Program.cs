using System.Globalization;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
        }
    }
}