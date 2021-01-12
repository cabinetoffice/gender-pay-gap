using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.CodeQualityTests
{
    [TestFixture]
    public class DoNotUseRedirectWithReturnUrls
    {

        [Test]
        public void DoNotAllowRedirectWithReturnUrlInCode()
        {
            var excludedFilesAndFolders = new List<string>
            {
                @"GenderPayGap.UnitTests\GenderPayGap.WebUI.Tests\CodeQualityTests\DoNotUseRedirectWithReturnUrls.cs", // This file!
                @"GenderPayGap.WebUI\bin", // Output folder
                @"GenderPayGap.WebUI\obj" // Output folder
            };

            // Arrange
            string rootCodeFolder = GetRootCodeFolder();

            // Pre-Act Assert (to check we're running the test on the right folder)
            Assert.That(File.Exists($"{rootCodeFolder}\\GenderPayGap.sln"), $"We expect to find a file [GenderPayGap.sln] in the root folder [{rootCodeFolder}]");

            // More Arrange
            string searchPattern = "*.cs*" /* We want to find .cs and .cshtml files */;
            FileInfo[] files = new DirectoryInfo(rootCodeFolder).GetFiles(searchPattern, SearchOption.AllDirectories);

            // Pre-Act Assert (to check again that we're running the test on the right folder)
            Assert.Greater(files.Length, 1000, "We expect there to be >1000 .cs/.cshtml files");

            // Act
            var failedFiles = new List<string>();
            foreach (FileInfo fileInfo in files)
            {
                string filePath = fileInfo.FullName;
                string filePathSuffix = filePath.Replace(rootCodeFolder, "");

                if (FileIsExcluded(filePathSuffix, excludedFilesAndFolders))
                {
                    continue;
                }

                string fileText = File.ReadAllText(filePath);
                // There are a couple of actions on controllers Redirect() that wrap a RedirectToAction,
                // we want to exclude these from the check as we are only concerned about redirecting with a url parameter
                if (fileText.Contains(" Redirect(") && !fileText.Contains(" Redirect()"))
                {
                    failedFiles.Add(filePathSuffix);
                }
            }

            // Assert
            if (failedFiles.Any())
            {
                Assert.Fail($"The following {failedFiles.Count} files contain a call to Redirect with a return url, which is not allowed:\n- {string.Join("\n- ", failedFiles)}\n");
            }
        }

        private static string GetRootCodeFolder()
        {
            string compiledDllFilename = Assembly.GetExecutingAssembly().Location;
            Console.WriteLine($"compiledDllFilename is [{compiledDllFilename}]");
            string compiledDllDirectory = new FileInfo(compiledDllFilename).Directory.FullName;

            string rootCodeFolder = new DirectoryInfo($"{compiledDllDirectory}\\..\\..\\..\\..\\..\\").FullName;
            return rootCodeFolder;
        }

        private static bool FileIsExcluded(string filePathSuffix, List<string> excludedFilesAndFolders)
        {
            foreach (string excludedFilePath in excludedFilesAndFolders)
            {
                if (filePathSuffix.StartsWith(excludedFilePath))
                {
                    return true;
                }
            }
            return false;
        }
    }
}