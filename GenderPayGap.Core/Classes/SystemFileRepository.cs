using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Core.Classes
{
    public class SystemFileRepository : IFileRepository
    {

        private readonly DirectoryInfo _rootDir;

        public SystemFileRepository(string rootPath = null)
        {
            rootPath = string.IsNullOrWhiteSpace(rootPath) ? AppDomain.CurrentDomain.BaseDirectory : FileSystem.ExpandLocalPath(rootPath);
            _rootDir = new DirectoryInfo(rootPath);
        }

        public void Write(string relativeFilePath, string csvFileContents)
        {
            string fullFilePath = GetFullPath(relativeFilePath);

            string directory = Path.GetDirectoryName(fullFilePath);

            // Create the folder (if it's missing)
            if (!GetDirectoryExists(directory))
            {
                CreateDirectory(directory);
            }

            File.WriteAllText(fullFilePath, csvFileContents);
        }

        public string Read(string relativeFilePath)
        {
            string fullFilePath = GetFullPath(relativeFilePath);

            return File.ReadAllText(fullFilePath);
        }

        private string GetFullPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(_rootDir.FullName, filePath);
            }

            return filePath;
        }

        private void CreateDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (!Path.IsPathRooted(directoryPath))
            {
                directoryPath = Path.Combine(_rootDir.FullName, directoryPath);
            }

            var retries = 0;
            Retry:
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (IOException)
            {
                if (retries >= 10)
                {
                    throw;
                }

                retries++;
                Thread.Sleep(500);
                goto Retry;
            }
        }

        private bool GetDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (!Path.IsPathRooted(directoryPath))
            {
                directoryPath = Path.Combine(_rootDir.FullName, directoryPath);
            }

            return Directory.Exists(directoryPath);
        }

    }
}
