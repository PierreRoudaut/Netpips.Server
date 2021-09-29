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
            const string testSettingsFilePath = "/home/netpips/netpips.test.settings.json";
            if (File.Exists(testSettingsFilePath))
            {
                return new ConfigurationBuilder()
                    .AddJsonFile(testSettingsFilePath, reloadOnChange: false, optional: false)
                    .Build();
            }

            var testSettingsAsJson = Environment.GetEnvironmentVariable("NETPIPS_TEST_SETTINGS_JSON");
            if (!string.IsNullOrWhiteSpace(testSettingsAsJson))
            {
                var tmpFilePath = Path.GetTempFileName();
                File.WriteAllText(tmpFilePath, testSettingsAsJson);
                var config = new ConfigurationBuilder()
                    .AddJsonFile(tmpFilePath, reloadOnChange: false, optional: false)
                    .Build();
                File.Delete(tmpFilePath);
                return config;
            }

            throw new Exception(
                $"Neither {testSettingsFilePath} file exists nor NETPIPS_TEST_SETTINGS_JSON env var is not set");
        }
    }
}