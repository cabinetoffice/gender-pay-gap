using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.ExternalServices.FileRepositories
{
    public class SystemFileRepository : IFileRepository
    {

        private readonly DirectoryInfo _rootDir;

        public SystemFileRepository(string rootPath = null)
        {
            rootPath = string.IsNullOrWhiteSpace(rootPath) ? AppDomain.CurrentDomain.BaseDirectory : FileSystem.ExpandLocalPath(rootPath);
            _rootDir = new DirectoryInfo(rootPath);
        }

        public void Write(string relativeFilePath, string fileContents)
        {
            string fullFilePath = GetFullPath(relativeFilePath);

            string directory = Path.GetDirectoryName(fullFilePath);

            // Create the folder (if it's missing)
            if (!GetDirectoryExists(directory))
            {
                CreateDirectory(directory);
            }

            File.WriteAllText(fullFilePath, fileContents);
        }

        public void Write(string relativeFilePath, byte[] fileContents)
        {
            string fullFilePath = GetFullPath(relativeFilePath);

            string directory = Path.GetDirectoryName(fullFilePath);

            // Create the folder (if it's missing)
            if (!GetDirectoryExists(directory))
            {
                CreateDirectory(directory);
            }

            File.WriteAllBytes(fullFilePath, fileContents);
        }

        public string Read(string relativeFilePath)
        {
            string fullFilePath = GetFullPath(relativeFilePath);

            return File.ReadAllText(fullFilePath);
        }

        public List<string> GetFiles(string relativeDirectoryPath)
        {
            string directoryFilePath = GetFullPath(relativeDirectoryPath);

            string[] filePaths = Directory.GetFiles(directoryFilePath);

            List<string> fileNames = filePaths.Select(fp => Path.GetFileName(fp)).ToList();

            return fileNames;
        }

        public void Delete(string relativeFilePath)
        {
            string fullFilePath = GetFullPath(relativeFilePath);

            File.Delete(fullFilePath);
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
