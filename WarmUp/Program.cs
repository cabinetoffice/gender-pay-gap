using System;
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
        private const int SecondsToWaitForEachHttpResponse = 5;

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
                Console.WriteLine("Starting new set of HTTP requests");

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
                "/account/sign-in",
                "/account/sign-out"
            };

            List<Uri> uris = paths
                .Select(path => new Uri($"https://{appName}.azurewebsites.net{path}"))
                .ToList();

            return uris;
        }

        private static bool AreAllUrlsAvailable(List<Uri> urlsToTest)
        {
            var successfulResponses = new HashSet<Uri>();

            Task[] tasks = urlsToTest
                .Select(uri => MakeRequestAndAddToListIfSuccessful(uri, successfulResponses))
                .ToArray();

            Task.WaitAll(tasks, SecondsToWaitForEachHttpResponse * 1000);

            bool allSucceeded = urlsToTest.Count == successfulResponses.Count;

            Console.WriteLine(allSucceeded ? "All Succeeded!" : "Some failures");
            foreach (Uri uri in urlsToTest)
            {
                string succeededOrFailed = successfulResponses.Contains(uri) ? "Succeeded" : "Failed";
                Console.WriteLine($"{succeededOrFailed} - {uri}");
            }

            return allSucceeded;
        }

        private static Task MakeRequestAndAddToListIfSuccessful(Uri uri, HashSet<Uri> successfulResponses)
        {
            Task task = new Task(() => {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(uri).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        successfulResponses.Add(uri);
                    }
                }
            });

            task.Start();
            return task;
        }

    }
}
