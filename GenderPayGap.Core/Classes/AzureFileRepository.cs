using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Core;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace GenderPayGap.Core.Classes
{
    public class AzureFileRepository : IFileRepository
    {

        private readonly CloudFileDirectory _rootDir;

        public AzureFileRepository(string connectionString, string shareName, IRetryPolicy retryPolicy = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrWhiteSpace(shareName))
            {
                throw new ArgumentNullException(nameof(shareName));
            }

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a CloudFileClient object for credentialed access to File storage.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();
            fileClient.DefaultRequestOptions.RetryPolicy =
                retryPolicy ?? new LinearRetry(TimeSpan.FromMilliseconds(500), 10); //Maximum of 5 second wait 

            CloudFileShare share = fileClient.GetShareReference(shareName);

            _rootDir = share.GetRootDirectoryReference();
        }

        public bool GetFileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            filePath = Url.DirToUrlSeparator(filePath);

            CloudFileDirectory directory = GetDirectoryAsync(Url.GetDirectoryName(filePath)).Result;
            if (directory == null || !directory.ExistsAsync().Result)
            {
                return false;
            }

            CloudFile file = GetFileAsync(filePath).Result;
            return file != null && file.ExistsAsync().Result;
        }

        public void Write(string relativeFilePath, string csvFileContents)
        {
            string fullFilePath = GetFullPath(relativeFilePath);

            CloudFile file = GetFile(fullFilePath);

            file.UploadTextAsync(csvFileContents).Wait();
        }

        public string Read(string relativeFilePath)
        {
            string fullFilePath = GetFullPath(relativeFilePath);

            CloudFile file = GetFile(fullFilePath);

            return file.DownloadTextAsync().Result;
        }

        private string GetFullPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!Path.IsPathRooted(filePath))
            {
                filePath = Url.Combine(_rootDir.Name, filePath);
            }

            filePath = Url.DirToUrlSeparator(filePath);
            return filePath;
        }

        private async Task<CloudFileDirectory> GetDirectoryAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");
            }

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            directoryPath = directoryPath.TrimI(@"/\");
            return string.IsNullOrWhiteSpace(directoryPath) ? _rootDir : _rootDir.GetDirectoryReference(directoryPath);
        }

        private async Task<CloudFile> GetFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            filePath = Url.DirToUrlSeparator(filePath);

            filePath = filePath.TrimI(@"/\");
            return _rootDir.GetFileReference(filePath);
        }

        private CloudFile GetFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            filePath = Url.DirToUrlSeparator(filePath);

            filePath = filePath.TrimI(@"/\");
            return _rootDir.GetFileReference(filePath);
        }

    }
}
