namespace GenderPayGap.WebUI.ExternalServices.FileRepositories
{
    public interface IFileRepository
    {

        void Write(string relativeFilePath, string csvFileContents);

        string Read(string relativeFilePath);

    }
}
