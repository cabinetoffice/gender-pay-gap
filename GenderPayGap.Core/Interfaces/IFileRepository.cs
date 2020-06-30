namespace GenderPayGap.Core.Interfaces
{
    public interface IFileRepository
    {

        void Write(string relativeFilePath, string csvFileContents);

        string Read(string relativeFilePath);

    }
}
