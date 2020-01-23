using System;
using System.IO;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Controllers.Administration
{
    public partial class AdminController : BaseController
    {

        /// <summary>
        ///     Refresh DATA in SCV files
        /// </summary>
        /// <param name="filePath"></param>
        private async Task UpdateFileAsync(string filePath, string action = null)
        {
            string fileName = Path.GetFileName(filePath);

            if (fileName == Filenames.UnfinishedOrganisations)
            {
                throw new NotImplementedException(
                    $"Cannot execute {nameof(UpdateFileAsync)} on {fileName} as the code has not yet been implemented");
            }

            //Mark the file as updating
            await SetFileUpdatedAsync(filePath);

            //trigger the update webjob
            await Program.MvcApplication.ExecuteWebjobQueue.AddMessageAsync(
                new QueueWrapper($"command=UpdateFile&filePath={filePath}&action={action}"));
        }

        /// <summary>
        ///     Checks if a file update has been triggered
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task<bool> GetFileUpdatingAsync(string filePath)
        {
            filePath = filePath.ToLower();
            DateTime updated = Session[$"FileUpdate:{filePath}"].ToDateTime();
            if (updated == DateTime.MinValue)
            {
                return false;
            }

            DateTime changed = updated;
            if (await Global.FileRepository.GetFileExistsAsync(filePath))
            {
                changed = await Global.FileRepository.GetLastWriteTimeAsync(filePath);
            }

            return updated == changed;
        }

        /// <summary>
        ///     Marks a file as triggered for an update
        /// </summary>
        /// <param name="filePath"></param>
        private async Task SetFileUpdatedAsync(string filePath)
        {
            filePath = filePath.ToLower();
            if (await Global.FileRepository.GetFileExistsAsync(filePath))
            {
                Session[$"FileUpdate:{filePath}"] = await Global.FileRepository.GetLastWriteTimeAsync(filePath);
            }
            else
            {
                Session[$"FileUpdate:{filePath}"] = VirtualDateTime.Now;
            }
        }

    }
}
