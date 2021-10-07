using System;
using System.Diagnostics;
using System.Text;

namespace Netpips.Core.CommandLine
{
    public static class CommandLineHelper
    {
        public static CommandLineResult ExecuteCommand(CommandLineRequest lineRequest)
        {
            var result = new CommandLineResult();
            var sbStdout = new StringBuilder();
            var sbStderr = new StringBuilder();

            if (!lineRequest.Timeout.HasValue)
            {
                lineRequest.Timeout = TimeSpan.FromSeconds(30);
            }

            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = lineRequest.Command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Arguments = lineRequest.Arguments,
                }
            };
            var sw = Stopwatch.StartNew();
            try
            {
                p.OutputDataReceived += (s, data) => sbStdout.AppendLine(data.Data);
                p.ErrorDataReceived += (s, data) => sbStderr.AppendLine(data.Data);
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit((int) lineRequest.Timeout.Value.TotalMilliseconds);
                result.Stdout = sbStdout.ToString();
                result.Stderr = sbStderr.ToString();
                result.ExitCode = p.ExitCode;
                result.ElapsedMs = sw.ElapsedMilliseconds;
            }
            catch (Exception e)
            {
                result.ElapsedMs = sw.ElapsedMilliseconds;
                result.Exception = e;
            }

            return result;
        }

    }
}