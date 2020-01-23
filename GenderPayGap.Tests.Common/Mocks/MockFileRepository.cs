using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.Downloadable;
using GenderPayGap.Extensions;

namespace GenderPayGap.Tests.Common.Mocks
{
    public class MockFileRepository : IFileRepository
    {

        private readonly Dictionary<string, DateTime> fileAccesses = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, IDictionary<string, string>> fileMetaData =
            new Dictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> fileWrites = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public string RootDir => throw new NotImplementedException();
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        public async Task AppendAsync(string filePath, string text)
        {
            string file = null;
            if (files.ContainsKey(filePath))
            {
                file = files[filePath];
            }

            file += text;
            files[filePath] = file;
            fileAccesses[filePath] = VirtualDateTime.Now;
            fileWrites[filePath] = VirtualDateTime.Now;
        }

        public async Task CreateDirectoryAsync(string directoryPath) { }

        public async Task DeleteFileAsync(string filePath)
        {
            if (files.ContainsKey(filePath))
            {
                files.Remove(filePath);
            }

            if (fileAccesses.ContainsKey(filePath))
            {
                fileAccesses.Remove(filePath);
            }

            if (fileWrites.ContainsKey(filePath))
            {
                fileWrites.Remove(filePath);
            }
        }

        public async Task CopyFileAsync(string sourceFilePath, string destinationFilePath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> GetDirectoryExistsAsync(string directoryPath)
        {
            return true;
        }

        public async Task<bool> GetFileExistsAsync(string filePath)
        {
            return files.ContainsKey(filePath);
        }

        public async Task<IEnumerable<string>> GetFilesAsync(string directoryPath, string searchPattern = null, bool recursive = false)
        {
            var results = new List<string>();
            foreach (string key in files.Keys)
            {
                if (!key.StartsWithI(directoryPath))
                {
                    continue;
                }

                if (!recursive && key.Substring(directoryPath.Length).TrimStartI("/\\").ContainsAny('/', '\\'))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(searchPattern) && !Path.GetFileName(key).Like(searchPattern))
                {
                    continue;
                }

                results.Add(key);
            }

            return results;
        }

        public async Task<bool> GetAnyFileExistsAsync(string directoryPath, string searchPattern = null, bool recursive = false)
        {
            var results = new List<string>();
            foreach (string key in files.Keys)
            {
                if (!key.StartsWithI(directoryPath))
                {
                    continue;
                }

                if (!recursive && key.Substring(directoryPath.Length).TrimStartI("/\\").ContainsAny('/', '\\'))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(searchPattern) && !Path.GetFileName(key).Like(searchPattern))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public async Task<long> GetFileSizeAsync(string filePath)
        {
            if (!files.ContainsKey(filePath))
            {
                throw new FileNotFoundException();
            }

            return files[filePath].Length;
        }

        public string GetFullPath(string filePath)
        {
            return filePath;
        }

        public async Task<DateTime> GetLastWriteTimeAsync(string filePath)
        {
            if (!fileWrites.ContainsKey(filePath))
            {
                throw new FileNotFoundException();
            }

            return fileWrites[filePath];
        }

        public async Task<IDictionary<string, string>> LoadMetaDataAsync(string filePath)
        {
            return fileMetaData.ContainsKey(filePath)
                ? fileMetaData[filePath]
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task SaveMetaDataAsync(string filePath, IDictionary<string, string> metaData)
        {
            fileMetaData[filePath] = metaData;
        }

        public async Task<string> GetMetaDataAsync(string filePath, string key)
        {
            IDictionary<string, string> metaData = await LoadMetaDataAsync(filePath);
            return metaData.ContainsKey(key) ? metaData[key] : null;
        }

        public async Task SetMetaDataAsync(string filePath, string key, string value)
        {
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

        public async Task<string> ReadAsync(string filePath)
        {
            if (!files.ContainsKey(filePath))
            {
                throw new FileNotFoundException();
            }

            return files[filePath];
        }

        public async Task<byte[]> ReadBytesAsync(string filePath)
        {
            if (!files.ContainsKey(filePath))
            {
                throw new FileNotFoundException();
            }

            return Encoding.UTF8.GetBytes(files[filePath]);
        }

        public async Task<DataTable> ReadDataTableAsync(string filePath)
        {
            string fileContent = await ReadAsync(filePath);
            return fileContent.ToDataTable();
        }


        public async Task WriteAsync(string filePath, byte[] bytes)
        {
            files[filePath] = Encoding.UTF8.GetString(bytes);
            fileAccesses[filePath] = VirtualDateTime.Now;
            fileWrites[filePath] = VirtualDateTime.Now;
        }

        public async Task WriteAsync(string filePath, Stream stream)
        {
            files[filePath] = stream.ToString();
            fileAccesses[filePath] = VirtualDateTime.Now;
            fileWrites[filePath] = VirtualDateTime.Now;
        }

        public async Task WriteAsync(string filePath, FileInfo uploadFile)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            string content = null;
            if (uploadFile.Exists)
            {
                content = File.ReadAllText(uploadFile.FullName);
            }
            else if (files.ContainsKey(uploadFile.FullName))
            {
                content = files[uploadFile.FullName];
            }
            else
            {
                throw new FileNotFoundException(nameof(uploadFile));
            }

            files[filePath] = content;
            fileAccesses[filePath] = VirtualDateTime.Now;
            fileWrites[filePath] = VirtualDateTime.Now;
        }

        public async Task<IEnumerable<string>> GetDirectoriesAsync(string directoryPath,
            string searchPattern = null,
            bool recursive = false)
        {
            var results = new List<string>();
            foreach (string key in files.Keys)
            {
                if (!key.StartsWithI(directoryPath))
                {
                    continue;
                }

                if (!recursive && key.Substring(directoryPath.Length).TrimStartI("/\\").ContainsAny('/', '\\'))
                {
                    continue;
                }

                string path = key.Substring(0, key.Length - Path.GetFileName(key).Length);
                if (!string.IsNullOrWhiteSpace(searchPattern) && !path.Like(searchPattern))
                {
                    continue;
                }

                results.Add(key);
            }

            return results;
        }

        public async Task ReadAsync(string filePath, Stream stream)
        {
            throw new NotImplementedException();
        }
        
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    }
}
