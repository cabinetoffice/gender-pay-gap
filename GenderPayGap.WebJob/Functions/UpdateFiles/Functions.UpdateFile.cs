using System;
using System.Collections.Generic;
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
                case Filenames.Organisations:
                    await UpdateOrganisationsAsync(filePath);
                    break;
                case Filenames.Users:
                    await UpdateUsersAsync(filePath);
                    break;
                case Filenames.Registrations:
                    await UpdateRegistrationsAsync(filePath);
                    break;
                case Filenames.RegistrationAddresses:
                    await UpdateRegistrationAddressesAsync(filePath);
                    break;
                case Filenames.UnverifiedRegistrations:
                    await UpdateUnverifiedRegistrationsAsync(filePath);
                    break;
                case Filenames.OrganisationScopes:
                    await UpdateScopesAsync(filePath);
                    break;
                case Filenames.OrganisationSubmissions:
                    await UpdateSubmissionsAsync(filePath);
                    break;
                case Filenames.OrganisationLateSubmissions:
                    await UpdateOrganisationLateSubmissionsAsync(filePath);
                    break;
                case Filenames.OrphanOrganisations:
                    await UpdateOrphanOrganisationsAsync(filePath);
                    break;
            }
        }


        public async Task WriteRecordsPerYearAsync<T>(string filePath, Func<int, Task<List<T>>> fillRecordsAsync)
        {
            string path = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string prefix = fileName.BeforeFirst("_");
            string datePart = fileName.AfterLast("_", includeWhenNoSeparator: false);

            int endYear = SectorTypes.Private.GetAccountingStartDate().Year;
            int startYear = Global.FirstReportingYear;
            if (!string.IsNullOrWhiteSpace(datePart))
            {
                int start = datePart.BeforeFirst("-").ToInt32().ToFourDigitYear();
                if (start > startYear && start <= endYear)
                {
                    startYear = start;
                }

                endYear = startYear;
            }

            //Make sure start and end are in correct order
            if (startYear > endYear)
            {
                (startYear, endYear) = (endYear, startYear);
            }

            for (int year = endYear; year >= startYear; year--)
            {
                List<T> records = await fillRecordsAsync(year);

                filePath = $"{prefix}_{year}-{(year + 1).ToTwoDigitYear()}{extension}";
                if (!string.IsNullOrWhiteSpace(path))
                {
                    filePath = Path.Combine(path, filePath);
                }

                await Global.FileRepository.SaveCSVAsync(records, filePath);
            }
        }

        public async Task WriteRecordsForYearAsync<T>(string filePath, int year, Func<Task<List<T>>> fillRecordsAsync)
        {
            string path = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string prefix = fileName.BeforeFirst("_");

            List<T> records = await fillRecordsAsync();

            filePath = $"{prefix}_{year}-{(year + 1).ToTwoDigitYear()}{extension}";
            if (!string.IsNullOrWhiteSpace(path))
            {
                filePath = Path.Combine(path, filePath);
            }

            await Global.FileRepository.SaveCSVAsync(records, filePath);
        }

    }
}
