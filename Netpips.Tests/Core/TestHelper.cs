using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Netpips.Core;
using Netpips.Core.Settings;
using Netpips.Identity.Authorization;
using Netpips.Identity.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Netpips.Tests.Core
{
    public static class TestHelper
    {
        public static readonly User NotAnItemOwner =
            new User
            {
                Email = "not-an-owner@example.com",
                Role = Role.User,
                FamilyName = "",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid()
            };

        public static readonly User ItemOwner =
            new User
            {
                Email = "owner@example.com",
                Role = Role.User,
                FamilyName = "",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid()
            };

        public static readonly User Admin =
            new User
            {
                Email = "admin@example.com",
                Role = Role.Admin,
                FamilyName = "Admin",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid()
            };

        public static readonly User SuperAdmin =
            new User
            {
                Email = "super.admin@example.com",
                Role = Role.SuperAdmin,
                FamilyName = "SuperAdmin",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid()
            };

        /// <summary>
        /// Fetches a configuration based on NETPIPS_TEST_ prefix for env vars
        /// </summary>
        /// <returns></returns>
        private static IConfigurationRoot GetTestConfiguration()
        {
            return AppSettingsFactory.BuildTestConfiguration();
        }

        /// <summary>
        /// Create a file given a path and create the necessary parent directories if they do not exist
        /// </summary>
        /// <param name="path"></param>
        public static void CreateFile(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            using (File.Create(path)) { }
        }

        public static NetpipsSettings CreateNetpipsAppSettings()
        {
            var netpips = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "home", "netpips"));
            if (netpips.Exists)
                netpips.Delete(true);

            // netpips
            netpips.Create();

            // netpips/medialibrary/<Movies|TV Shows|Music|Others>
            var mediaLibrary = netpips.CreateSubdirectory("medialibrary");
            NetpipsSettings.MediaFolders.ToList().ForEach(elem => mediaLibrary.CreateSubdirectory(elem));

            // netpips/downloads
            netpips.CreateSubdirectory("downloads");

            // netpips/logs
            netpips.CreateSubdirectory("logs");

            // netpips/.torrent_done.sh
            var torrentDonePath = Path.Combine(netpips.FullName, ".torrent_done.sh");
            CreateFile(torrentDonePath);

            return new NetpipsSettings
            {
                HomePath = netpips.FullName,
                Domain = new Uri("https://some-domain.com"),
                DaemonUserEmail = "some-daemon-user@domain.com"
            };
        }

        public static DirectDownloadSettings CreateDirectDownloadSettings()
        {
            var settings = GetTestConfiguration().GetSectionSettings<DirectDownloadSettings>();
            if (settings == null)
            {
                throw new ApplicationException("Failed to load FilehosterSettings with test configuration");
            }
            if (settings.Filehosters == null || settings.Filehosters.Count == 0)
            {
                throw new ApplicationException("No filehosters were registered. Test will fail");
            }
            return settings;
        }

        public static AuthSettings CreateAuthSettings()
        {
            var settings = new AuthSettings
            {
                JwtKey = "dummykey012345679801234567891234",
                JwtIssuer = "https://some-domain.com",
                JwtExpireMinutes = 30,
                GoogleClientId = "123456789-someurl.apps.googleusercontent.com"
            };
            return settings;
        }

        private static readonly List<PropertyInfo> ShowRssPropertyInfos = typeof(ShowRssSettings).GetProperties().ToList();

        public static ShowRssSettings CreateShowRssSettings()
        {
            var settings = GetTestConfiguration().GetSectionSettings<ShowRssSettings>();
            ShowRssPropertyInfos.ForEach(p =>
            {
                var value = p.GetValue(settings);
                if (string.IsNullOrEmpty(value?.ToString()))
                {
                    throw new ApplicationException($"ShowRss.{p.Name} value is null or empty");
                }
            });
            return settings;
        }

        public static string GetPathWithoutExtension(this string path)
        {
            return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
        }

        public static string Uid()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string GetRessourceContent(string ressourceFilename)
        {
            var asm = Assembly.GetExecutingAssembly();
            var resource = string.Format(asm.GetName().Name + ".ressources.{0}", ressourceFilename);
            using (var stream = asm.GetManifestResourceStream(resource))
            {
                if (stream == null) return string.Empty;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public static string GetRessourceFullPath(string ressourceFileame)
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            path = Path.GetDirectoryName(path);
            var ressourcePath = Path.Combine(path, "ressources", ressourceFileame);
            if (!File.Exists(ressourcePath))
            {
                throw new Exception(ressourcePath + " does not exist");
            }

            return ressourcePath;
        }

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyInfos = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static string ToJson<T>(this T item) => JsonConvert.SerializeObject(item, Formatting.Indented);
        public static string ToStringOfProperties<T>(this T item, int i = 0)
        {
            var propertyInfos = PropertyInfos.GetOrAdd(typeof(T), type => type.GetProperties());

            var sb = new StringBuilder();

            foreach (var info in propertyInfos)
            {
                // if (info.GetType().IsClass && !info.GetType().IsAssignableFrom(typeof(Exception)))
                //     return ToStringOfProperties((T) info.GetValue(item));
                var value = info.GetValue(item) ?? "NULL";
                sb.AppendLine(Enumerable.Repeat('\t', i) + info.Name + ": " + value);
            }

            return sb.ToString();
        }
    }
}