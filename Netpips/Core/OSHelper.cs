using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Serilog;

namespace Netpips.Core
{
    public static class OsHelper
    {
        public static string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36";


        public static int ExecuteCommand(string command, string arguments, out string output, out string error,
            TimeSpan? timeout = null)
        {
            var sbOutput = new StringBuilder();
            var sbError = new StringBuilder();
            output = "";
            error = "";
            if (!timeout.HasValue)
            {
                timeout = TimeSpan.FromSeconds(30);
            }

            var maxWaitingDuration =
                TimeSpan.MaxValue == timeout.Value ? int.MaxValue : (int) timeout.Value.TotalMilliseconds;

            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Arguments = arguments,
                }
            };
            p.OutputDataReceived += (s, data) => sbOutput.AppendLine(data.Data);
            p.ErrorDataReceived += (s, data) => sbError.AppendLine(data.Data);
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            var success = p.WaitForExit(maxWaitingDuration);
            try
            {
                output = sbOutput.ToString();
                error = sbError.ToString();
            }
            catch (Exception ex)
            {
                Log.Warning("OsHelper.ExecuteCommande: failed to read cmd stdout and stderr");
                Log.Warning(ex.Message);
            }

            return success ? p.ExitCode : -1;
        }

        public static string GetRessourceContent(string ressourceFilename)
        {
            var asm = Assembly.GetCallingAssembly();
            var resource = $"Netpips.ressources.{ressourceFilename}";
            using (var stream = asm.GetManifestResourceStream(resource))
            {
                if (stream == null) return string.Empty;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }
}