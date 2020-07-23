using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminDataMigrationController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminDataMigrationController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("data-migration/export-all-as-file-download")]
        public IActionResult ExportAllDataAsFileDownload()
        {
            string allDataString = LoadAllDataFromDatabaseInfoJsonString();
            byte[] allDataBytes = Zip(allDataString);

            var fileContentResult = new FileContentResult(allDataBytes, "application/json")
            {
                FileDownloadName = "AllData.json.gz"
            };

            return fileContentResult;
        }

        [AllowAnonymous]
        [HttpGet("data-migration/export-all")]
        public IActionResult ExportAllDataAsResponseBody(string password)
        {
            if (string.IsNullOrWhiteSpace(Global.DataMigrationPassword))
            {
                // Don't allow access if no password is set
                return Unauthorized();
            }
            if (password != Global.DataMigrationPassword)
            {
                // Don't allow access is the user got the password wrong
                return Unauthorized();
            }

            string allDataString = LoadAllDataFromDatabaseInfoJsonString();

            var fileContentResult = new ContentResult
            {
                Content = allDataString,
                ContentType = "application/json",
                StatusCode = 200
            };

            return fileContentResult;
        }

        private string LoadAllDataFromDatabaseInfoJsonString()
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
                OrganisationReferences = dataRepository.GetAll<OrganisationReference>().OrderBy(x => x.OrganisationReferenceId).AsNoTracking().ToList(),
                OrganisationScopes = dataRepository.GetAll<OrganisationScope>().OrderBy(x => x.OrganisationScopeId).AsNoTracking().ToList(),
                OrganisationSicCodes = dataRepository.GetAll<OrganisationSicCode>().OrderBy(x => x.OrganisationSicCodeId).AsNoTracking().ToList(),
                OrganisationStatuses = dataRepository.GetAll<OrganisationStatus>().OrderBy(x => x.OrganisationStatusId).AsNoTracking().ToList(),
                PublicSectorTypes = dataRepository.GetAll<PublicSectorType>().OrderBy(x => x.PublicSectorTypeId).AsNoTracking().ToList(),
                ReminderEmails = dataRepository.GetAll<ReminderEmail>().OrderBy(x => x.ReminderEmailId).AsNoTracking().ToList(),
                Returns = dataRepository.GetAll<Return>().OrderBy(x => x.ReturnId).AsNoTracking().ToList(),
                ReturnStatuses = dataRepository.GetAll<ReturnStatus>().OrderBy(x => x.ReturnStatusId).AsNoTracking().ToList(),
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
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class AllData
    {
        
        [JsonProperty]
        public List<AuditLog> AuditLogs { get; set; }
        [JsonProperty]
        public List<DataProtectionKey> DataProtectionKeys { get; set; }
        [JsonProperty]
        public List<DraftReturn> DraftReturns { get; set; }
        [JsonProperty]
        public List<Feedback> Feedbacks { get; set; }
        [JsonProperty]
        public List<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }
        [JsonProperty]
        public List<Organisation> Organisations { get; set; }
        [JsonProperty]
        public List<OrganisationAddress> OrganisationAddresses { get; set; }
        [JsonProperty]
        public List<OrganisationName> OrganisationNames { get; set; }
        [JsonProperty]
        public List<OrganisationPublicSectorType> OrganisationPublicSectorTypes { get; set; }
        [JsonProperty]
        public List<OrganisationReference> OrganisationReferences { get; set; }
        [JsonProperty]
        public List<OrganisationScope> OrganisationScopes { get; set; }
        [JsonProperty]
        public List<OrganisationSicCode> OrganisationSicCodes { get; set; }
        [JsonProperty]
        public List<OrganisationStatus> OrganisationStatuses { get; set; }
        [JsonProperty]
        public List<PublicSectorType> PublicSectorTypes { get; set; }
        [JsonProperty]
        public List<ReminderEmail> ReminderEmails { get; set; }
        [JsonProperty]
        public List<Return> Returns { get; set; }
        [JsonProperty]
        public List<ReturnStatus> ReturnStatuses { get; set; }
        [JsonProperty]
        public List<SicCode> SicCodes { get; set; }
        [JsonProperty]
        public List<SicSection> SicSections { get; set; }
        [JsonProperty]
        public List<User> Users { get; set; }
        [JsonProperty]
        public List<UserOrganisation> UserOrganisations { get; set; }
        [JsonProperty]
        public List<UserStatus> UserStatuses { get; set; }

    }
}
