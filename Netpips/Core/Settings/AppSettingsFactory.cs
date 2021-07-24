using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Netpips.Core.Settings
{
    public static class AppSettingsFactory
    {
        public static IConfigurationRoot BuildConfiguration()
        {
            // var settingsPath = Directory
            //     .EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            //         "netpips.*.settings.json", SearchOption.TopDirectoryOnly)
            //     .FirstOrDefault(x => Path.GetFileName(x) != "netpips.test.settings.json");
            
            var settingsPath = "/home/netpips/netpips.Production.settings.json";

            if (settingsPath == null)
            {
                throw new ApplicationException("No settings file found");
            }
            var builder = new ConfigurationBuilder()
                .AddJsonFile(settingsPath, reloadOnChange: true, optional: false);

            return builder.Build();
        }

        public static IConfigurationRoot BuildTestConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath("/home/netpips")
                //.SetBasePath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
                .AddJsonFile("netpips.test.settings.json", reloadOnChange: true, optional: false);

            return builder.Build();
        }
    }
}
