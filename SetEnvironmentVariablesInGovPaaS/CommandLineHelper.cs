using System.Collections.Generic;
using System.Diagnostics;

namespace SetEnvironmentVariablesInGovPaaS
{
    public static class CommandLineHelper
    {

        public static void RunCommandAndPrintOutputToConsole(string command, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }

        public static List<string> RunCommandAndGetOutput(string command, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            var output = new List<string>();

            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                output.Add(line);
            }

            return output;
        }

    }
}
