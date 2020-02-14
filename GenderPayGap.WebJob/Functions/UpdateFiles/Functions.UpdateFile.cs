using System.IO;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task UpdateFileAsync(string filePath, string action)
        {
            string fileName = Path.GetFileName(filePath);

            switch (Filenames.GetRootFilename(fileName))
            {
                case Filenames.Users:
                    await UpdateUsersAsync(filePath);
                    break;
                case Filenames.Registrations:
                    await UpdateRegistrationsAsync(filePath);
                    break;
                case Filenames.UnverifiedRegistrations:
                    await UpdateUnverifiedRegistrationsAsync(filePath);
                    break;
            }
        }


    }
}
