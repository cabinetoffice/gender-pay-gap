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

        public async Task CreateDirectoryAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            directoryPath = directoryPath.TrimI(@"/\");
            string[] dirs = directoryPath.SplitI(@"/\");
            if (dirs.Length < 1)
            {
                return;
            }

            CloudFileDirectory directory = _rootDir;

            foreach (string dir in dirs)
            {
                CloudFile file = directory.GetFileReference(dir);
                if (file != null && await file.ExistsAsync())
                {
                    return;
                }

                directory = directory.GetDirectoryReference(dir);
                if (directory != null)
                {
                    await directory.CreateIfNotExistsAsync();
                }
            }
        }

        public async Task<bool> GetDirectoryExistsAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            CloudFileDirectory dir = await GetDirectoryAsync(directoryPath);
            return dir != null && await dir.ExistsAsync();
        }

        public async Task<bool> GetFileExistsAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            filePath = Url.DirToUrlSeparator(filePath);

            CloudFileDirectory directory = await GetDirectoryAsync(Url.GetDirectoryName(filePath));
            if (directory == null || !await directory.ExistsAsync())
            {
                return false;
            }

            CloudFile file = await GetFileAsync(filePath);
            return file != null && await file.ExistsAsync();
        }


        public async Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            filePath = Url.DirToUrlSeparator(filePath);

            CloudFile file = await GetFileAsync(filePath);
            await file?.DeleteIfExistsAsync();
            string metaPath = filePath + ".metadata";
            CloudFile metaFile = await GetFileAsync(metaPath);
            await metaFile?.DeleteIfExistsAsync();
        }

        public async Task CopyFileAsync(string sourceFilePath, string destinationFilePath, bool overwrite)
        {
            sourceFilePath = Url.DirToUrlSeparator(sourceFilePath);
            destinationFilePath = Url.DirToUrlSeparator(destinationFilePath);

            CloudFile sourceCloudFile = await GetFileAsync(sourceFilePath);
            if (sourceCloudFile == null || !await sourceCloudFile.ExistsAsync())
            {
                throw new FileNotFoundException($"Cannot find file '{sourceFilePath}'");
            }

            CloudFile destinationCloudFile = await GetFileAsync(destinationFilePath);
            if (!overwrite && await destinationCloudFile.ExistsAsync())
            {
                throw new FileNotFoundException($"Destination file already exists '{destinationFilePath}'");
            }

            await DeleteFileAsync(destinationFilePath);

            await destinationCloudFile.StartCopyAsync(sourceCloudFile);
        }

        public async Task<IEnumerable<string>> GetFilesAsync(string directoryPath, string searchPattern = null, bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            CloudFileDirectory directory = await GetDirectoryAsync(directoryPath);
            if (directory == null || !await directory.ExistsAsync())
            {
                throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");
            }

            var files = new List<string>();
            var token = new FileContinuationToken();
            FileResultSegment items = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
            foreach (IListFileItem fileDir in items.Results)
            {
                if (fileDir is CloudFile)
                {
                    var file = (CloudFile) fileDir;
                    if (string.IsNullOrWhiteSpace(searchPattern) || file.Name.Like(searchPattern))
                    {
                        files.Add(Url.Combine(directoryPath, file.Name));
                    }
                }
                else if (recursive)
                {
                    var dir = (CloudFileDirectory) fileDir;
                    files.AddRange(await GetFilesAsync(Url.Combine(directoryPath, dir.Name), searchPattern, recursive));
                }
            }

            return files;
        }

        public async Task<bool> GetAnyFileExistsAsync(string directoryPath, string searchPattern = null, bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            CloudFileDirectory directory = await GetDirectoryAsync(directoryPath);
            if (directory == null || !await directory.ExistsAsync())
            {
                throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");
            }

            var files = new List<string>();
            var token = new FileContinuationToken();
            FileResultSegment items = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
            foreach (IListFileItem fileDir in items.Results)
            {
                if (fileDir is CloudFile)
                {
                    var file = (CloudFile) fileDir;
                    if (string.IsNullOrWhiteSpace(searchPattern) || file.Name.Like(searchPattern))
                    {
                        return true;
                    }
                }
                else if (recursive)
                {
                    var dir = (CloudFileDirectory) fileDir;
                    if (await GetAnyFileExistsAsync(Url.Combine(directoryPath, dir.Name), searchPattern, recursive))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<string> ReadAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            filePath = Url.DirToUrlSeparator(filePath);

            CloudFile file = await GetFileAsync(filePath);
            if (file == null || !await file.ExistsAsync())
            {
                throw new FileNotFoundException($"Cannot find file '{filePath}'");
            }

            return await file.DownloadTextAsync();
        }

        public async Task WriteAsync(string filePath, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            filePath = Url.DirToUrlSeparator(filePath);

            //Ensure the directory exists
            string directory = Url.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !await GetDirectoryExistsAsync(directory))
            {
                await CreateDirectoryAsync(directory);
            }

            CloudFile file = await GetFileAsync(filePath);

            var stream = new SyncMemoryStream(bytes, 0, bytes.Length);
            await file.UploadFromStreamAsync(stream);
        }

        public string GetFullPath(string filePath)
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

        public async Task<IDictionary<string, string>> LoadMetaDataAsync(string filePath)
        {
            filePath = Url.DirToUrlSeparator(filePath);

            CloudFile file = await GetFileAsync(filePath);

            if (file == null || !await file.ExistsAsync())
            {
                throw new FileNotFoundException($"Cannot find file '{filePath}'", filePath);
            }

            return file.Metadata;
        }

        public async Task SaveMetaDataAsync(string filePath, IDictionary<string, string> metaData)
        {
            filePath = Url.DirToUrlSeparator(filePath);

            CloudFile file = await GetFileAsync(filePath);
            if (file == null || !await file.ExistsAsync())
            {
                throw new FileNotFoundException($"Cannot find file '{filePath}'", filePath);
            }

            //Set the new values
            foreach (string key in metaData.Keys)
            {
                file.Metadata[key] = metaData[key];
            }

            //Remove the old values
            foreach (string key in file.Metadata.Keys.Except(metaData.Keys))
            {
                file.Metadata.Remove(key);
            }

            await file.SetMetadataAsync();
        }

        public async Task<string> GetMetaDataAsync(string filePath, string key)
        {
            filePath = Url.DirToUrlSeparator(filePath);

            IDictionary<string, string> metaData = await LoadMetaDataAsync(filePath);
            return metaData.ContainsKey(key) ? metaData[key] : null;
        }

        public async Task SetMetaDataAsync(string filePath, string key, string value)
        {
            filePath = Url.DirToUrlSeparator(filePath);

            IDictionary<string, string> metaData = await LoadMetaDataAsync(filePath);

            if (metaData.ContainsKey(key) && metaData[key] == value)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                metaData[key] = value;
            }
            else if (metaData.ContainsKey(key))
            {
                metaData.Remove(key);
            }

            await SaveMetaDataAsync(filePath, metaData);
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
