using System.Threading.Tasks;

namespace GenderPayGap.Core.Interfaces
{
    public interface IFileRepository
    {

        bool GetFileExists(string filePath);

        void Write(string relativeFilePath, string csvFileContents);

        string Read(string relativeFilePath);

    }
}
