using System.Threading.Tasks;

namespace GenderPayGap.Core.Interfaces
{
    public interface IFileRepository
    {

        Task<bool> GetFileExistsAsync(string filePath);

        Task DeleteFileAsync(string filePath);
        Task CopyFileAsync(string sourceFilePath, string destinationFilePath, bool overwrite);

        Task<string> ReadAsync(string filePath);

        Task WriteAsync(string filePath, byte[] bytes);

        Task<string> GetMetaDataAsync(string filePath, string key);
        Task SetMetaDataAsync(string filePath, string key, string value);

        void Write(string relativeFilePath, string csvFileContents);

        string Read(string relativeFilePath);

    }
}
