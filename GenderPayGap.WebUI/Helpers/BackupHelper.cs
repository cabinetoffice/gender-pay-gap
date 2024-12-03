using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Backup;
using GenderPayGap.Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Helpers
{
    public static class BackupHelper
    {

        public static string LoadAllDataFromDatabaseInfoJsonString(IDataRepository dataRepository)
        {
            var allData = new AllData
            {
                AuditLogs = dataRepository.GetAll<AuditLog>().OrderBy(x => x.AuditLogId).AsNoTracking().ToList(),
                DataProtectionKeys = dataRepository.GetAll<DataProtectionKey>().OrderBy(x => x.Id).AsNoTracking().ToList(),
                DraftReturns = dataRepository.GetAll<DraftReturn>().OrderBy(x => x.DraftReturnId).AsNoTracking().ToList(),
                Feedbacks = dataRepository.GetAll<Feedback>().OrderBy(x => x.FeedbackId).AsNoTracking().ToList(),
                InactiveUserOrganisations = dataRepository.GetAll<InactiveUserOrganisation>().OrderBy(x => x.UserId).ThenBy(x => x.OrganisationId).AsNoTracking().ToList(),
                Organisations = dataRepository.GetAll<Organisation>().OrderBy(x => x.OrganisationId).AsNoTracking().ToList(),
                OrganisationAddresses = dataRepository.GetAll<OrganisationAddress>().OrderBy(x => x.AddressId).AsNoTracking().ToList(),
                OrganisationNames = dataRepository.GetAll<OrganisationName>().OrderBy(x => x.OrganisationNameId).AsNoTracking().ToList(),
                OrganisationPublicSectorTypes = dataRepository.GetAll<OrganisationPublicSectorType>().OrderBy(x => x.OrganisationPublicSectorTypeId).AsNoTracking().ToList(),
                OrganisationScopes = dataRepository.GetAll<OrganisationScope>().OrderBy(x => x.OrganisationScopeId).AsNoTracking().ToList(),
                OrganisationSicCodes = dataRepository.GetAll<OrganisationSicCode>().OrderBy(x => x.OrganisationSicCodeId).AsNoTracking().ToList(),
                OrganisationStatuses = dataRepository.GetAll<OrganisationStatus>().OrderBy(x => x.OrganisationStatusId).AsNoTracking().ToList(),
                PublicSectorTypes = dataRepository.GetAll<PublicSectorType>().OrderBy(x => x.PublicSectorTypeId).AsNoTracking().ToList(),
                ReminderEmails = dataRepository.GetAll<ReminderEmail>().OrderBy(x => x.ReminderEmailId).AsNoTracking().ToList(),
                Returns = dataRepository.GetAll<Return>().OrderBy(x => x.ReturnId).AsNoTracking().ToList(),
                SicCodes = dataRepository.GetAll<SicCode>().OrderBy(x => x.SicCodeId).AsNoTracking().ToList(),
                SicSections = dataRepository.GetAll<SicSection>().OrderBy(x => x.SicSectionId).AsNoTracking().ToList(),
                Users = dataRepository.GetAll<User>().OrderBy(x => x.UserId).AsNoTracking().ToList(),
                UserOrganisations = dataRepository.GetAll<UserOrganisation>().OrderBy(x => x.UserId).ThenBy(x => x.OrganisationId).AsNoTracking().ToList(),
                UserStatuses = dataRepository.GetAll<UserStatus>().OrderBy(x => x.UserStatusId).AsNoTracking().ToList(),
            };

            string allDataString = JsonConvert.SerializeObject(allData);
            return allDataString;
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

    }

}
