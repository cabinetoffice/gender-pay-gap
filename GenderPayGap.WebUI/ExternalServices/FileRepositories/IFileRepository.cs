namespace GenderPayGap.WebUI.ExternalServices.FileRepositories
{
    public interface IFileRepository
    {

        void Write(string relativeFilePath, string fileContents);
        void Write(string relativeFilePath, byte[] fileContents);

        string Read(string relativeFilePath);

        List<string> GetFiles(string relativeDirectoryPath);

        void Delete(string relativeFilePath);

        bool FileExists(string relativeFilePath);

        long? GetFileSize(string relativeFilePath);

    }
}
