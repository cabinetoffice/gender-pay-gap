using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GenderPayGap.WebUI.Tests.TestHelpers;
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
            string rootCodeFolder = CodeQualityTestHelpers.GetRootCodeFolder();

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

                if (CodeQualityTestHelpers.FileIsExcluded(filePathSuffix, excludedFilesAndFolders))
                {
                    continue;
                }

                // We want to pick up all instances of Redirect(url) or new RedirectResult(url) that do not have the disable comment on the previous line.
                // Ignores those without parameters so we don't pick up any methods defined as Redirect() for example
                var lines = File.ReadAllLines(filePath);
                for (var i = 0; i<lines.Length; i++)
                {
                    if (Regex.IsMatch(lines[i], @"(new){0,1}\s+Redirect(Result){0,1}\s*\(\S+\)") && !lines[i-1].Contains("//disable:DoNotUseRedirectWithReturnUrls"))
                    {
                        failedFiles.Add(filePathSuffix);
                    }
                }
            }

            // Assert
            if (failedFiles.Any())
            {
                Assert.Fail($"The following {failedFiles.Count} files contain a Redirect.\nIf this is to a local url LocalRedirect should be used, otherwise the redirect can be marked to be ignored by this test by adding the \n'//disable:DoNotUseRedirectWithReturnUrls' comment on the preceding line:\n- {string.Join("\n- ", failedFiles.Distinct())}\n");
            }
        }
    }
}