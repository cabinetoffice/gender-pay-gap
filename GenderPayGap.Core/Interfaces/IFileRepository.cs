using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GenderPayGap.Core.Interfaces
{
    public interface IFileRepository
    {

        Task CreateDirectoryAsync(string directoryPath);

        Task<bool> GetDirectoryExistsAsync(string directoryPath);

        Task<bool> GetFileExistsAsync(string filePath);

        Task DeleteFileAsync(string filePath);
        Task CopyFileAsync(string sourceFilePath, string destinationFilePath, bool overwrite);

        Task<IEnumerable<string>> GetFilesAsync(string directoryPath, string searchPattern = null, bool recursive = false);

        Task<string> ReadAsync(string filePath);

        Task WriteAsync(string filePath, byte[] bytes);
        Task WriteAsync(string filePath, FileInfo uploadFile);

        string GetFullPath(string filePath);

        Task<string> GetMetaDataAsync(string filePath, string key);
        Task SetMetaDataAsync(string filePath, string key, string value);

    }
}
