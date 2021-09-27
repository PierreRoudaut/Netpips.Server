using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Netpips.Core.Settings
{
    public static class AppSettingsFactory
    {
        public static IConfigurationRoot BuildConfiguration()
        {
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
            const string testSettingsFilename = "/home/netpips/netpips.test.settings.json";
            if (File.Exists(testSettingsFilename))
            {
                return new ConfigurationBuilder()
                    .AddJsonFile(testSettingsFilename, reloadOnChange: true, optional: false)
                    .Build();
            }

            var testSettingsJson = Environment.GetEnvironmentVariable("NETPIPS_TEST_SETTINGS_JSON");
            if (!string.IsNullOrWhiteSpace(testSettingsJson))
            {
                var tmpFilename = Path.GetTempFileName();
                File.WriteAllText(tmpFilename, testSettingsJson);
                return new ConfigurationBuilder()
                    .AddJsonFile(testSettingsJson, reloadOnChange: true, optional: false)
                    .Build();
            }

            throw new Exception(
                $"Neither {testSettingsFilename} file exists nor NETPIPS_TEST_SETTINGS_JSON env var is not set");
        }
    }
}