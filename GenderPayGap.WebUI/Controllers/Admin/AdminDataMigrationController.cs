using System.Text;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Backup;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            string allDataString = BackupHelper.LoadAllDataFromDatabaseInfoJsonString(dataRepository);
            byte[] allDataBytes = BackupHelper.Zip(allDataString);

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

            string allDataString = BackupHelper.LoadAllDataFromDatabaseInfoJsonString(dataRepository);

            var fileContentResult = new ContentResult
            {
                Content = allDataString,
                ContentType = "application/json",
                StatusCode = 200
            };

            return fileContentResult;
        }

        [AllowAnonymous]
        [HttpGet("data-migration/from-file")]
        public IActionResult DataMigrationFromFile()
        {
            if (!IsUserAllowedToImportData())
            {
                return Unauthorized();
            }

            var viewModel = new AdminFileUploadViewModel();
            return View("DataMigrationFromFile", viewModel);
        }

        [AllowAnonymous]
        [HttpPost("data-migration/from-file")]
        [ValidateAntiForgeryToken]
        public IActionResult ImportAllDataFromFile(AdminFileUploadViewModel viewModel)
        {
            if (!IsUserAllowedToImportData())
            {
                return Unauthorized();
            }

            byte[] fileAsBytes;
            using (var memoryStream = new MemoryStream())
            {
                viewModel.File.OpenReadStream().CopyTo(memoryStream);
                fileAsBytes = memoryStream.ToArray();
            }

            string fileAsString = BackupHelper.Unzip(fileAsBytes);

            ImportDataFromJsonString(fileAsString);

            return Json("Succeeded!");
        }

        [AllowAnonymous]
        [HttpGet("data-migration/from-remote-server")]
        public IActionResult DataMigrationFromRemoteServer()
        {
            if (!IsUserAllowedToImportData())
            {
                return Unauthorized();
            }

            var viewModel = new AdminDataMigrationViewModel();
            return View("DataMigrationFromRemoteServer", viewModel);
        }

        [AllowAnonymous]
        [HttpPost("data-migration/from-remote-server")]
        [ValidateAntiForgeryToken]
        public IActionResult ImportAllDataFromRemoteServer(AdminDataMigrationViewModel viewModel)
        {
            if (!IsUserAllowedToImportData())
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(Global.DataMigrationPassword))
            {
                // Don't allow access if no password is set
                return Unauthorized();
            }
            if (viewModel.Password != Global.DataMigrationPassword)
            {
                // Don't allow access is the user got the password wrong
                return Unauthorized();
            }

            StartChunkedResponse();

            WriteParagraph($"Requesting data export from {viewModel.Hostname}");

            var httpClient = new HttpClient();
            if (!string.IsNullOrWhiteSpace(viewModel.BasicAuthUsername) && !string.IsNullOrWhiteSpace(viewModel.BasicAuthPassword))
            {
                httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderHelper.GetBasicAuthenticationHeaderValue(viewModel.BasicAuthUsername, viewModel.BasicAuthPassword);
            }
            string requestUrl = $"https://{viewModel.Hostname}/admin/data-migration/export-all?password={viewModel.Password}";
            string responseString = httpClient.GetStringAsync(requestUrl).Result;

            WriteParagraph($"Received data export from {viewModel.Hostname}");

            ImportDataFromJsonString(responseString);

            WriteParagraph($"Data Migration Complete!");
            Thread.Sleep(TimeSpan.FromSeconds(5)); // Sleep for a few seconds to allow the output to be flushed (sometimes the last sections of output are missed, which is confusing the the user

            EndChunkedResponse();
            Thread.Sleep(TimeSpan.FromSeconds(5)); // Sleep for a few seconds to allow the output to be flushed (sometimes the last sections of output are missed, which is confusing the the user

            return null;
        }

        private bool IsUserAllowedToImportData()
        {
            bool databaseIsEmpty = dataRepository.GetAll<User>().Count() == 0;

            bool userIsAdministrator = User.Identity.IsAuthenticated
                                       && ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository).IsFullAdministrator();

            return databaseIsEmpty || userIsAdministrator;
        }

        private void StartChunkedResponse()
        {
            HttpContext.Items.Add("StartTime", VirtualDateTime.Now);

            Response.StatusCode = 200;
            Response.ContentType = "text/html";
            Response.StartAsync().Wait();

            Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(AdminMigrationTopAndBottomOfView.TopOfView));
        }

        private void WriteParagraph(string text)
        {
            DateTime startTime = (DateTime) HttpContext.Items["StartTime"];
            TimeSpan elapsedTime = VirtualDateTime.Now.Subtract(startTime);

            string html = $@"<p class=""govuk-body"">{elapsedTime.ToString("mm\\:ss")} {text}</p>";

            Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(html));
            Response.BodyWriter.FlushAsync();
        }

        private void EndChunkedResponse()
        {
            Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(AdminMigrationTopAndBottomOfView.BottomOfView));
            Response.CompleteAsync().Wait();
        }

        private void ImportDataFromJsonString(string fileAsString)
        {
            AllData allData = JsonConvert.DeserializeObject<AllData>(fileAsString);

            string deleteAllSql = @"
                DELETE FROM ""Feedback"" WHERE 1 = 1;
                DELETE FROM ""DraftReturns"" WHERE 1 = 1;
                DELETE FROM ""InactiveUserOrganisations"" WHERE 1 = 1;

                DELETE FROM ""Returns"" WHERE 1 = 1;
                DELETE FROM ""AuditLogs"" WHERE 1 = 1;

                UPDATE ""Organisations"" SET ""LatestPublicSectorTypeId"" = NULL WHERE 1 = 1;
                DELETE FROM ""OrganisationPublicSectorTypes"" WHERE 1 = 1;

                DELETE FROM ""OrganisationSicCodes"" WHERE 1 = 1;
                DELETE FROM ""OrganisationScopes"" WHERE 1 = 1;
                DELETE FROM ""UserOrganisations"" WHERE 1 = 1;
                DELETE FROM ""OrganisationStatus"" WHERE 1 = 1;
                DELETE FROM ""OrganisationAddresses"" WHERE 1 = 1;
                DELETE FROM ""OrganisationNames"" WHERE 1 = 1;

                DELETE FROM ""Organisations"" WHERE 1 = 1;

                DELETE FROM ""ReminderEmails"" WHERE 1 = 1;
                DELETE FROM ""UserStatus"" WHERE 1 = 1;
                DELETE FROM ""Users"" WHERE 1 = 1;

                DELETE FROM ""PublicSectorTypes"" WHERE 1 = 1;
                DELETE FROM ""SicCodes"" WHERE 1 = 1;
                DELETE FROM ""SicSections"" WHERE 1 = 1;
            ";
            
            WriteParagraph($"About to delete existing data");
            dataRepository.ExecuteRawSql(deleteAllSql);
            WriteParagraph($"Deleted existing data");

            WriteParagraph($"Starting DataProtectionKey");
            dataRepository.GetAll<DataProtectionKey>().ToList().ForEach(dpk => dataRepository.Delete(dpk));
            dataRepository.Insert(allData.DataProtectionKeys);
            dataRepository.SaveChanges();
            WriteParagraph($"DataProtectionKey done");

            InsertData(allData.Feedbacks);
            InsertData(allData.DraftReturns);
            InsertData(allData.PublicSectorTypes);

            InsertData(allData.SicSections);
            InsertData(allData.SicCodes);

            InsertData(allData.Users);
            InsertData(allData.UserStatuses);
            InsertData(allData.ReminderEmails);

            WriteParagraph($"Starting Organisations and OrganisationPublicSectorTypes");
            using (var serviceScope = Program.DependencyInjectionServiceProvider.CreateScope())
            {
                IDataRepository newDataRepository = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
                newDataRepository.Insert(allData.Organisations);
                newDataRepository.Insert(allData.OrganisationPublicSectorTypes);
                newDataRepository.SaveChanges();
            }
            allData.Organisations.Clear();
            allData.OrganisationPublicSectorTypes.Clear();
            WriteParagraph($"Organisations and OrganisationPublicSectorTypes done");

            InsertData(allData.OrganisationNames);
            InsertData(allData.OrganisationAddresses);
            InsertData(allData.OrganisationStatuses);

            InsertData(allData.InactiveUserOrganisations);
            InsertData(allData.UserOrganisations);

            InsertData(allData.OrganisationScopes);
            InsertData(allData.OrganisationSicCodes);

            InsertData(allData.AuditLogs);
            InsertData(allData.Returns);

            string updateSequencesSql = @"
                SELECT setval('""AuditLogs_AuditLogId_seq""', (SELECT max(""AuditLogId"") FROM ""AuditLogs""), true);
                SELECT setval('""DataProtectionKeys_Id_seq""', (SELECT max(""Id"") FROM ""DataProtectionKeys""), true);
                SELECT setval('""DraftReturns_DraftReturnId_seq""', (SELECT max(""DraftReturnId"") FROM ""DraftReturns""), true);
                SELECT setval('""Feedback_FeedbackId_seq""', (SELECT max(""FeedbackId"") FROM ""Feedback""), true);
                SELECT setval('""OrganisationAddresses_AddressId_seq""', (SELECT max(""AddressId"") FROM ""OrganisationAddresses""), true);
                SELECT setval('""OrganisationNames_OrganisationNameId_seq""', (SELECT max(""OrganisationNameId"") FROM ""OrganisationNames""), true);
                SELECT setval('""OrganisationPublicSectorTypes_OrganisationPublicSectorTypeI_seq""', (SELECT max(""OrganisationPublicSectorTypeId"") FROM ""OrganisationPublicSectorTypes""), true);
                SELECT setval('""Organisations_OrganisationId_seq""', (SELECT max(""OrganisationId"") FROM ""Organisations""), true);
                SELECT setval('""OrganisationScopes_OrganisationScopeId_seq""', (SELECT max(""OrganisationScopeId"") FROM ""OrganisationScopes""), true);
                SELECT setval('""OrganisationSicCodes_OrganisationSicCodeId_seq""', (SELECT max(""OrganisationSicCodeId"") FROM ""OrganisationSicCodes""), true);
                SELECT setval('""OrganisationStatus_OrganisationStatusId_seq""', (SELECT max(""OrganisationStatusId"") FROM ""OrganisationStatus""), true);
                SELECT setval('""PublicSectorTypes_PublicSectorTypeId_seq""', (SELECT max(""PublicSectorTypeId"") FROM ""PublicSectorTypes""), true);
                SELECT setval('""ReminderEmails_ReminderEmailId_seq""', (SELECT max(""ReminderEmailId"") FROM ""ReminderEmails""), true);
                SELECT setval('""Returns_ReturnId_seq""', (SELECT max(""ReturnId"") FROM ""Returns""), true);
                SELECT setval('""Users_UserId_seq""', (SELECT max(""UserId"") FROM ""Users""), true);
                SELECT setval('""UserStatus_UserStatusId_seq""', (SELECT max(""UserStatusId"") FROM ""UserStatus""), true);
            ";

            WriteParagraph($"About to update sequences");
            dataRepository.ExecuteRawSql(updateSequencesSql);
            WriteParagraph($"Updated sequences");
        }

        private void InsertData<T>(List<T> items) where T : class
        {
            using (var serviceScope = Program.DependencyInjectionServiceProvider.CreateScope())
            {
                IDataRepository newDataRepository = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();

                newDataRepository.Insert(items);

                newDataRepository.SaveChanges();
            }

            items.Clear();
            WriteParagraph($"{typeof(T).Name} done");
        }

    }

    public class AdminMigrationTopAndBottomOfView
    {

        public static string TopOfView = @"

<!DOCTYPE html>

<html lang=""en"" class=""govuk-template app-html-class"">
<head>
    <meta charset=""utf-8"" />
    <title>Data Migration - Administration - Gender pay gap service</title>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1, viewport-fit=cover"">
    <meta name=""theme-color"" content=""blue"" />

    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />

    <link rel=""shortcut icon"" sizes=""16x16 32x32 48x48"" href=""/assets/images/favicon.ico"" type=""image/x-icon"" />
    <link rel=""mask-icon"" href=""/assets/images/govuk-mask-icon.svg"" color=""blue"">
    <link rel=""apple-touch-icon"" sizes=""180x180"" href=""/assets/images/govuk-apple-touch-icon-180x180.png"">
    <link rel=""apple-touch-icon"" sizes=""167x167"" href=""/assets/images/govuk-apple-touch-icon-167x167.png"">
    <link rel=""apple-touch-icon"" sizes=""152x152"" href=""/assets/images/govuk-apple-touch-icon-152x152.png"">
    <link rel=""apple-touch-icon"" href=""/assets/images/govuk-apple-touch-icon.png"">

    <link href=""" + StaticAssetsVersioningHelper.GetAppCssFilename() + @""" rel=""stylesheet"" />

    <meta property=""og:image"" content=""/assets/images/govuk-opengraph-image.png"">
</head>

<body class=""govuk-template__body app-body-class"">
    <script>
        document.body.className = ((document.body.className) ? document.body.className + ' js-enabled' : 'js-enabled');
    </script>


    <a href=""#main-content"" class=""govuk-skip-link"">Skip to main content</a>


<header class=""govuk-header"" role=""banner"" data-module=""govuk-header""
        >

    <div class=""govuk-header__container govuk-width-container"">
        <div class=""govuk-header__logo"">
            <a href=""https://www.gov.uk"" class=""govuk-header__link govuk-header__link--homepage"">
                <span class=""govuk-header__logotype"">
                    <svg role=""presentation""
                         focusable=""false""
                         class=""govuk-header__logotype-crown""
                         xmlns=""http://www.w3.org/2000/svg""
                         viewbox=""0 0 132 97""
                         height=""30""
                         width=""36"">
                        <path fill=""currentColor"" fill-rule=""evenodd""
                              d=""M25 30.2c3.5 1.5 7.7-.2 9.1-3.7 1.5-3.6-.2-7.8-3.9-9.2-3.6-1.4-7.6.3-9.1 3.9-1.4 3.5.3 7.5 3.9 9zM9 39.5c3.6 1.5 7.8-.2 9.2-3.7 1.5-3.6-.2-7.8-3.9-9.1-3.6-1.5-7.6.2-9.1 3.8-1.4 3.5.3 7.5 3.8 9zM4.4 57.2c3.5 1.5 7.7-.2 9.1-3.8 1.5-3.6-.2-7.7-3.9-9.1-3.5-1.5-7.6.3-9.1 3.8-1.4 3.5.3 7.6 3.9 9.1zm38.3-21.4c3.5 1.5 7.7-.2 9.1-3.8 1.5-3.6-.2-7.7-3.9-9.1-3.6-1.5-7.6.3-9.1 3.8-1.3 3.6.4 7.7 3.9 9.1zm64.4-5.6c-3.6 1.5-7.8-.2-9.1-3.7-1.5-3.6.2-7.8 3.8-9.2 3.6-1.4 7.7.3 9.2 3.9 1.3 3.5-.4 7.5-3.9 9zm15.9 9.3c-3.6 1.5-7.7-.2-9.1-3.7-1.5-3.6.2-7.8 3.7-9.1 3.6-1.5 7.7.2 9.2 3.8 1.5 3.5-.3 7.5-3.8 9zm4.7 17.7c-3.6 1.5-7.8-.2-9.2-3.8-1.5-3.6.2-7.7 3.9-9.1 3.6-1.5 7.7.3 9.2 3.8 1.3 3.5-.4 7.6-3.9 9.1zM89.3 35.8c-3.6 1.5-7.8-.2-9.2-3.8-1.4-3.6.2-7.7 3.9-9.1 3.6-1.5 7.7.3 9.2 3.8 1.4 3.6-.3 7.7-3.9 9.1zM69.7 17.7l8.9 4.7V9.3l-8.9 2.8c-.2-.3-.5-.6-.9-.9L72.4 0H59.6l3.5 11.2c-.3.3-.6.5-.9.9l-8.8-2.8v13.1l8.8-4.7c.3.3.6.7.9.9l-5 15.4v.1c-.2.8-.4 1.6-.4 2.4 0 4.1 3.1 7.5 7 8.1h.2c.3 0 .7.1 1 .1.4 0 .7 0 1-.1h.2c4-.6 7.1-4.1 7.1-8.1 0-.8-.1-1.7-.4-2.4V34l-5.1-15.4c.4-.2.7-.6 1-.9zM66 92.8c16.9 0 32.8 1.1 47.1 3.2 4-16.9 8.9-26.7 14-33.5l-9.6-3.4c1 4.9 1.1 7.2 0 10.2-1.5-1.4-3-4.3-4.2-8.7L108.6 76c2.8-2 5-3.2 7.5-3.3-4.4 9.4-10 11.9-13.6 11.2-4.3-.8-6.3-4.6-5.6-7.9 1-4.7 5.7-5.9 8-.5 4.3-8.7-3-11.4-7.6-8.8 7.1-7.2 7.9-13.5 2.1-21.1-8 6.1-8.1 12.3-4.5 20.8-4.7-5.4-12.1-2.5-9.5 6.2 3.4-5.2 7.9-2 7.2 3.1-.6 4.3-6.4 7.8-13.5 7.2-10.3-.9-10.9-8-11.2-13.8 2.5-.5 7.1 1.8 11 7.3L80.2 60c-4.1 4.4-8 5.3-12.3 5.4 1.4-4.4 8-11.6 8-11.6H55.5s6.4 7.2 7.9 11.6c-4.2-.1-8-1-12.3-5.4l1.4 16.4c3.9-5.5 8.5-7.7 10.9-7.3-.3 5.8-.9 12.8-11.1 13.8-7.2.6-12.9-2.9-13.5-7.2-.7-5 3.8-8.3 7.1-3.1 2.7-8.7-4.6-11.6-9.4-6.2 3.7-8.5 3.6-14.7-4.6-20.8-5.8 7.6-5 13.9 2.2 21.1-4.7-2.6-11.9.1-7.7 8.8 2.3-5.5 7.1-4.2 8.1.5.7 3.3-1.3 7.1-5.7 7.9-3.5.7-9-1.8-13.5-11.2 2.5.1 4.7 1.3 7.5 3.3l-4.7-15.4c-1.2 4.4-2.7 7.2-4.3 8.7-1.1-3-.9-5.3 0-10.2l-9.5 3.4c5 6.9 9.9 16.7 14 33.5 14.8-2.1 30.8-3.2 47.7-3.2z"">
                        </path>
                        <image src=""/assets/images/govuk-logotype-crown.png"" xlink:href="""" class=""govuk-header__logotype-crown-fallback-image"" width=""36"" height=""32""></image>
                    </svg>
                    <span class=""govuk-header__logotype-text"">
                        GOV.UK
                    </span>
                </span>
            </a>
        </div>

            <div class=""govuk-header__content"">
                    <a href=""/"" class=""govuk-header__link govuk-header__link--service-name"">
                        Gender pay gap service
                    </a>
                    <button type=""button"" role=""button"" class=""govuk-header__menu-button govuk-js-header-toggle"" aria-controls=""navigation"" aria-label=""Show or hide Top Level Navigation"">Menu</button>
                    <nav>
                        <ul id=""navigation"" class=""govuk-header__navigation "" aria-label=""Top Level Navigation"">
                                    <li class=""govuk-header__navigation-item "">
                                        <a class=""govuk-header__link"" href=""/viewing/search-results"">
                                            Search and compare
                                        </a>
                                    </li>
                                    <li class=""govuk-header__navigation-item "">
                                        <a class=""govuk-header__link"" href=""/viewing/download"">
                                            Download
                                        </a>
                                    </li>
                                    <li class=""govuk-header__navigation-item "">
                                        <a class=""govuk-header__link"" href=""/actions-to-close-the-gap"">
                                            Close the gap
                                        </a>
                                    </li>
                        </ul>
                    </nav>
            </div>
    </div>
</header>
    <div class=""govuk-width-container"">
        
        
<div class=""govuk-phase-banner""
     >

    <p class=""govuk-phase-banner__content"">


<strong class=""govuk-tag govuk-phase-banner__content__tag""
        >


Beta</strong>
        <span class=""govuk-phase-banner__text"">

This is a new service � your <a class=""govuk-link"" href=""/send-feedback"">feedback</a> will help us to improve it.        </span>
    </p>
</div>

        

    
<div class=""govuk-breadcrumbs""
     >
    <ol class=""govuk-breadcrumbs__list"">
                <li class=""govuk-breadcrumbs__list-item"">
                    <a class=""govuk-breadcrumbs__link"" href=""/admin"">


Admin                    </a>
                </li>
                <li class=""govuk-breadcrumbs__list-item"" aria-current=""page"">

Data Migration                </li>
    </ol>
</div>



        <main class=""govuk-main-wrapper"" id=""main-content"" role=""main"">
            

<div class=""govuk-grid-row"">
    <div class=""govuk-grid-column-two-thirds"">

        <span class=""govuk-caption-xl"">Administration</span>
        <h1 class=""govuk-heading-xl"">
            Data Migration IN PROGRESS
        </h1>

";

        public const string BottomOfView = @"

</div>
</div>

        </main>
    </div>

    
<footer class=""govuk-footer"" role=""contentinfo""
        >

    <div class=""govuk-width-container"">

        <div class=""govuk-footer__meta"">
            <div class=""govuk-footer__meta-item govuk-footer__meta-item--grow"">
                    <h2 class=""govuk-visually-hidden"">Support links</h2>
                        <ul class=""govuk-footer__inline-list"">
                                <li class=""govuk-footer__inline-list-item"">
                                    <a class=""govuk-footer__link"" href=""/contact-us"">
                                        Contact Us
                                    </a>
                                </li>
                                <li class=""govuk-footer__inline-list-item"">
                                    <a class=""govuk-footer__link"" href=""/cookies"">
                                        Cookies
                                    </a>
                                </li>
                                <li class=""govuk-footer__inline-list-item"">
                                    <a class=""govuk-footer__link"" href=""/privacy-policy"">
                                        Privacy Policy
                                    </a>
                                </li>
                                <li class=""govuk-footer__inline-list-item"">
                                    <a class=""govuk-footer__link"" href=""/report-concerns"">
                                        Report Concerns
                                    </a>
                                </li>
                        </ul>
                <svg role=""presentation""
                     focusable=""false""
                     class=""govuk-footer__licence-logo""
                     xmlns=""http://www.w3.org/2000/svg""
                     viewbox=""0 0 483.2 195.7""
                     height=""17""
                     width=""41"">
                    <path fill=""currentColor""
                          d=""M421.5 142.8V.1l-50.7 32.3v161.1h112.4v-50.7zm-122.3-9.6A47.12 47.12 0 0 1 221 97.8c0-26 21.1-47.1 47.1-47.1 16.7 0 31.4 8.7 39.7 21.8l42.7-27.2A97.63 97.63 0 0 0 268.1 0c-36.5 0-68.3 20.1-85.1 49.7A98 98 0 0 0 97.8 0C43.9 0 0 43.9 0 97.8s43.9 97.8 97.8 97.8c36.5 0 68.3-20.1 85.1-49.7a97.76 97.76 0 0 0 149.6 25.4l19.4 22.2h3v-87.8h-80l24.3 27.5zM97.8 145c-26 0-47.1-21.1-47.1-47.1s21.1-47.1 47.1-47.1 47.2 21 47.2 47S123.8 145 97.8 145""/>
                </svg>
                <span class=""govuk-footer__licence-description"">
                    All content is available under the
                    <a class=""govuk-footer__link""
                       href=""https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/""
                       rel=""license"">
                        Open Government Licence v3.0
                    </a>, except where otherwise stated
                </span>
            </div>
            <div class=""govuk-footer__meta-item"">
                <a class=""govuk-footer__link govuk-footer__copyright-logo""
                   href=""https://www.nationalarchives.gov.uk/information-management/re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/"">
                    � Crown copyright
                </a>
            </div>
        </div>
    </div>
</footer>

</body>
</html>

";

    }

}
