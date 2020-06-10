using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WarmUp
{
    class Program
    {
        private const int ConsecutiveTestPassesToSucceed = 5;
        private const double MaximumSecondsToRunWarmUp = 5 * 60;
        private const int MillisecondsToWaitForEachHttpResponse = 20 * 1000;

        static int Main(string[] args)
        {
            string appName = args[0];

            List<Uri> urlsToTest = GetUrlsToTest(appName);

            DateTime testStartTime = DateTime.Now;
            int consecutiveTestPasses = 0;

            while (consecutiveTestPasses < ConsecutiveTestPassesToSucceed &&
                   DateTime.Now.Subtract(testStartTime).TotalSeconds < MaximumSecondsToRunWarmUp)
            {
                Thread.Sleep(1000);

                Console.WriteLine("");
                Console.WriteLine($"Starting new set of HTTP requests - {DateTime.Now.ToString("T")}");

                if (AreAllUrlsAvailable(urlsToTest))
                {
                    consecutiveTestPasses++;
                }
                else
                {
                    consecutiveTestPasses = 0;
                }
            }

            int successOrErrorCode = consecutiveTestPasses == 5 ? 0 : 1;

            return successOrErrorCode;
        }

        private static List<Uri> GetUrlsToTest(string appName)
        {
            string[] paths = {
                "/",
                "/viewing/search-results",
                "/Employer/FukQqlAW",
                "/Employer/FukQqlAW/2018",
                "/viewing/download",
                "/actions-to-close-the-gap",
                "/send-feedback",
                "/login",
                "/logout"
            };

            List<Uri> uris = paths
                .Select(path => new Uri($"https://{appName}.azurewebsites.net{path}"))
                .ToList();

            return uris;
        }

        private static bool AreAllUrlsAvailable(List<Uri> urlsToTest)
        {
            var startTime = DateTime.Now;
            var responses = new ConcurrentDictionary<Uri, int>();

            Task[] tasks = urlsToTest
                .Select(uri => MakeRequestAndAddToListIfSuccessful(uri, responses))
                .ToArray();

            //Task.WaitAll(tasks);
            Task.WaitAll(tasks, MillisecondsToWaitForEachHttpResponse);
            Thread.Sleep(100);

            bool allSucceeded = true;
            var consoleMessages = new List<string>();

            foreach (Uri uri in urlsToTest)
            {
                int? statusCode = responses.ContainsKey(uri) ? responses[uri] : (int?) null;
                bool succeeded = statusCode.HasValue && statusCode.Value >= 200 && statusCode.Value < 400;
                string succeededOrFailed = succeeded ? "Succeeded" : "Failed";
                string statusCodeString = statusCode.HasValue ? statusCode.Value.ToString() : "";
                consoleMessages.Add($"{succeededOrFailed} - {statusCodeString} - {uri}");

                allSucceeded &= succeeded;
            }
            Console.WriteLine(allSucceeded ? "All Succeeded!" : "Some failures");
            consoleMessages.ForEach(Console.WriteLine);

            if (!allSucceeded)
            {
                int waitTime = MillisecondsToWaitForEachHttpResponse - (int) DateTime.Now.Subtract(startTime).TotalMilliseconds;
                if (waitTime > 0)
                {
                    Thread.Sleep(waitTime);
                }
            }

            return allSucceeded;
        }

        private static Task MakeRequestAndAddToListIfSuccessful(Uri uri, ConcurrentDictionary<Uri, int> responses)
        {
            Task task = new Task(() => {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:68.0) Gecko/20100101 Firefox/68.0");
                    client.DefaultRequestHeaders.Add("Accept", "text/html");
                    client.DefaultRequestHeaders.Add("Cookie", "seen_cookie_message=%7B%22Version%22%3A1%7D;");

                    HttpResponseMessage response = client.GetAsync(uri).Result;
                    if (!responses.TryAdd(uri, (int) response.StatusCode))
                    {
                        Console.WriteLine("Couldn't add to dictionary");
                    }
                }
            });

            task.Start();
            return task;
        }

    }
}
