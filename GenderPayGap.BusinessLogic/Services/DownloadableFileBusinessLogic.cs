using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes.Downloadable;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Interfaces.Downloadable;
using GenderPayGap.Core.Models.Downloadable;

namespace GenderPayGap.BusinessLogic.Services
{
    public interface IDownloadableFileBusinessLogic
    {

        Task<DownloadableFileModel> GetFileRemovingSensitiveInformationAsync(string filePath);
        Task<IEnumerable<IDownloadableItem>> GetListOfDownloadableItemsFromPathAsync(string processedLogsPath);

    }

    public class DownloadableFileBusinessLogic : IDownloadableFileBusinessLogic
    {

        private readonly IFileRepository _fileRepository;

        public DownloadableFileBusinessLogic(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public async Task<DownloadableFileModel> GetFileRemovingSensitiveInformationAsync(string filePath)
        {
            var result = new DownloadableFileModel(filePath);

            result.DataTable = await _fileRepository.ReadDataTableAsync(result.Filepath);

            RemoveColumnFromDataTable(result.DataTable, "SecurityCode");
            RemoveColumnFromDataTable(result.DataTable, "SecurityCodeExpiryDateTime");
            RemoveColumnFromDataTable(result.DataTable, "SecurityCodeCreatedDateTime");

            return result;
        }

        public async Task<IEnumerable<IDownloadableItem>> GetListOfDownloadableItemsFromPathAsync(string processedLogsPath)
        {
            var result = new List<IDownloadableItem>();

            // As a minimum, the directory has the special parent folder '..' to go back up to.
            DownloadableDirectory parentDirectoryFolderFileInfo = DownloadableDirectory.GetSpecialParentFolderInfo(processedLogsPath);
            result.Add(parentDirectoryFolderFileInfo);

            foreach (string dirPath in await _fileRepository.GetDirectoriesAsync(processedLogsPath))
            {
                result.Add(new DownloadableDirectory(dirPath.Replace("\\", "/")));
            }

            foreach (string filePath in await _fileRepository.GetFilesAsync(processedLogsPath))
            {
                result.Add(new DownloadableFile(filePath.Replace("\\", "/")));
            }

            return result;
        }

        private void RemoveColumnFromDataTable(DataTable dataTable, string columnName)
        {
            // Removal implemented as per Microsoft specification (see the example in "remove string" section) => https://docs.microsoft.com/en-us/dotnet/api/system.data.datacolumncollection.remove?view=netframework-4.8#System_Data_DataColumnCollection_Remove_System_String_ 

            if (dataTable.Columns.Contains(columnName))
            {
                if (dataTable.Columns.CanRemove(dataTable.Columns[columnName]))
                {
                    dataTable.Columns.Remove(columnName);
                }
            }
        }

    }
}
