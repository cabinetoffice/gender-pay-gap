using System.IO;
using System.Threading.Tasks;
using GenderPayGap.Core;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task UpdateFileAsync(ILogger log, string filePath, string action)
        {
            string fileName = Path.GetFileName(filePath);

            switch (Filenames.GetRootFilename(fileName))
            {
                case Filenames.Registrations:
                    await UpdateRegistrationsAsync(log, filePath);
                    break;
                case Filenames.UnverifiedRegistrations:
                    await UpdateUnverifiedRegistrationsAsync(log, filePath);
                    break;
            }
        }


    }
}
