using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace GenderPayGap.Core.Interfaces
{
    public interface IFileRepository
    {

        string RootDir { get; }

        Task<IEnumerable<string>> GetDirectoriesAsync(string directoryPath, string searchPattern = null, bool recursive = false);
        Task CreateDirectoryAsync(string directoryPath);

        Task<bool> GetDirectoryExistsAsync(string directoryPath);

        Task<bool> GetFileExistsAsync(string filePath);
        Task<DateTime> GetLastWriteTimeAsync(string filePath);
        Task<long> GetFileSizeAsync(string filePath);

        Task DeleteFileAsync(string filePath);
        Task CopyFileAsync(string sourceFilePath, string destinationFilePath, bool overwrite);

        Task<IEnumerable<string>> GetFilesAsync(string directoryPath, string searchPattern = null, bool recursive = false);
        Task<bool> GetAnyFileExistsAsync(string directoryPath, string searchPattern = null, bool recursive = false);

        Task<string> ReadAsync(string filePath);
        Task ReadAsync(string filePath, Stream stream);

        Task<byte[]> ReadBytesAsync(string filePath);
        Task<DataTable> ReadDataTableAsync(string filePath);

        Task AppendAsync(string filePath, string text);

        Task WriteAsync(string filePath, byte[] bytes);
        Task WriteAsync(string filePath, Stream stream);
        Task WriteAsync(string filePath, FileInfo uploadFile);

        string GetFullPath(string filePath);

        Task<IDictionary<string, string>> LoadMetaDataAsync(string filePath);

        Task<string> GetMetaDataAsync(string filePath, string key);
        Task SetMetaDataAsync(string filePath, string key, string value);

    }
}
