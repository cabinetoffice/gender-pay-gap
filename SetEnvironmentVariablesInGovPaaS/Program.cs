using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SetEnvironmentVariablesInGovPaaS
{
    class Program
    {
        private const string PaasEnvironmentVariablePrefix = "PAAS_";

        static void Main(string[] args)
        {
            string appName = args[0];

            UnsetAllCurrentPaasEnvironmentVariables(appName);

            SetNewPaasEnvironmentVaribales(appName);
        }

        private static void UnsetAllCurrentPaasEnvironmentVariables(string appName)
        {
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine($"Un-setting all current Gov.UK PaaS environment variables for app: {appName}");
            Console.WriteLine("");

            List<string> allCurrentPaasEnvironmentVariables = GetAllCurrentPaasEnvironmentVariables(appName);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Current PaaS environment variables:");
            foreach (string variableName in allCurrentPaasEnvironmentVariables)
            {
                Console.WriteLine(variableName);
            }
            Console.WriteLine("");

            foreach (string variableName in allCurrentPaasEnvironmentVariables)
            {
                Console.WriteLine("-----");
                Console.WriteLine($"Un-setting variable: {variableName}");
                UnsetPaasEnvironmentVariable(appName, variableName);
                Console.WriteLine("");
            }
        }

        private static List<string> GetAllCurrentPaasEnvironmentVariables(string appName)
        {
            List<string> commandOutput = CommandLineHelper.RunCommandAndGetOutput("cf", $"v3-env {appName}");

            var possiblePaasEnvironmentVariables = new List<string>();

            foreach (string line in commandOutput)
            {
                Match match = Regex.Match(line, @"^(\w*):.*$",
                    RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string possiblePaasEnvironmentVariable = match.Groups[1].Value;
                    possiblePaasEnvironmentVariables.Add(possiblePaasEnvironmentVariable);
                }
            }

            return possiblePaasEnvironmentVariables;
        }

        private static void UnsetPaasEnvironmentVariable(string appName, string variableName)
        {
            CommandLineHelper.RunCommandAndPrintOutputToConsole("cf", $"v3-unset-env {appName} {variableName}");
        }

        private static void SetNewPaasEnvironmentVaribales(string appName)
        {
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine($"Setting new Gov.UK PaaS environment variables for app: {appName}");
            Console.WriteLine("");

            Dictionary<string, string> paasEnvironmentVariablesToSet = GetPaasEnvironmentVariablesToSet();

            Console.WriteLine("-----------------------------");
            Console.WriteLine("Variables we're about to set:");
            foreach ((string variableName, string _) in paasEnvironmentVariablesToSet)
            {
                Console.WriteLine(variableName);
            }
            Console.WriteLine("");

            foreach ((string variableName, string variableValue) in paasEnvironmentVariablesToSet)
            {
                Console.WriteLine("-----");
                Console.WriteLine($"Setting variable: {variableName}");
                SetPaasEnvironmentVariable(appName, variableName, variableValue);
            }
        }

        private static Dictionary<string, string> GetPaasEnvironmentVariablesToSet()
        {
            IDictionary allEnvironmentVariables = Environment.GetEnvironmentVariables();

            List<string> paasEnvironmentVariableKeys =
                allEnvironmentVariables.Keys
                    .OfType<string>()
                    .Where(key => key.StartsWith(PaasEnvironmentVariablePrefix))
                    .ToList();

            var paasEnvironmentVariablesToSet = new Dictionary<string, string>();

            foreach (string paasEnvironmentVariableKey in paasEnvironmentVariableKeys)
            {
                string variableName = paasEnvironmentVariableKey.Substring(PaasEnvironmentVariablePrefix.Length);
                string variableValue = allEnvironmentVariables[paasEnvironmentVariableKey].ToString();

                paasEnvironmentVariablesToSet.Add(variableName, variableValue);
            }

            return paasEnvironmentVariablesToSet;
        }

        private static void SetPaasEnvironmentVariable(string appName, string variableName, string variableValue)
        {
            CommandLineHelper.RunCommandAndPrintOutputToConsole("cf", $"v3-set-env {appName} \"{variableName}\" \"{variableValue}\"");
        }

    }
}
