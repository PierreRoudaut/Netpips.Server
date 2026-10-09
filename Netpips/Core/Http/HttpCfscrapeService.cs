using System;
using System.Net;
using Netpips.Core.CommandLine;
using Newtonsoft.Json;

namespace Netpips.Core.Http
{
    public static class HttpCfscrapeService
    {
        private class PythonHttpResponseLite
        {
            public int StatusCode { get; set; }
            public string Content { get; set; }
        }

        /// <summary>
        /// https://github.com/Anorov/cloudflare-scrape
        /// e.g python -c "import cfscrape; import json; scraper = cfscrape.create_scraper(); r = scraper.get('https://www.magnetdl.com/t/the-bad-batch-s01e09/'); print(json.dumps({'statusCode': r.status_code, 'content': r.text}));"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpResponseLite GetFromPythonCommandLine(string url)
        {
            var commandLineResult = CommandLineHelper.ExecuteCommand(new CommandLineRequest
            {
                Timeout = TimeSpan.FromSeconds(10),
                Command = "python",
                Arguments =
                    $@"-c ""import cfscrape; import json; scraper = cfscrape.create_scraper(); r = scraper.get('{url}'); print(json.dumps({{'statusCode': r.status_code, 'content': r.text}}));"""
            });

            var httpResult = new HttpResponseLite {ElapsedMs = commandLineResult.ElapsedMs};
            if (!commandLineResult.Suceeded)
            {
                httpResult.Exception = commandLineResult.Exception;
                Console.WriteLine($"ExitCode: {commandLineResult.ExitCode}");
                Console.WriteLine($"Stdout: {commandLineResult.Stdout}");
                Console.WriteLine($"Stderr: {commandLineResult.Stderr}");

                return httpResult;
            }

            try
            {
                var pythonRes = JsonConvert.DeserializeObject<PythonHttpResponseLite>(commandLineResult.Stdout);
                httpResult.StatusCode = (HttpStatusCode) pythonRes.StatusCode;
                httpResult.Html = pythonRes.Content;
            }
            catch (Exception e)
            {
                httpResult.Exception = e;
            }

            return httpResult;
        }
    }
}