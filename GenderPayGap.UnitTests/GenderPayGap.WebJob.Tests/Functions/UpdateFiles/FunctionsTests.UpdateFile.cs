using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;
using GenderPayGap.WebJob.Tests.TestHelpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebJob.Tests.Functions
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class FunctionsTests
    {

        [SetUp]
        public void BeforeEach()
        {
            //Instantiate the dependencies
            _functions = WebJobTestHelper.SetUp();
        }

        private WebJob.Functions _functions;


        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateFile_Shared_Ok")]
        public async Task FunctionsUpdateFile_UpdateFile_Shared_OkAsync()
        {
            await _functions.UpdateFileAsync(string.Empty, string.Empty);

            throw new NotImplementedException();
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateOrganisationLateSubmissions_TimeTrigger_Ok")]
        public void FunctionsUpdateFile_UpdateOrganisationLateSubmissions_TimeTrigger_Ok()
        {
            var timerInfo = new TimerInfo(new DailySchedule(), new ScheduleStatus());

            throw new NotImplementedException();

            //_functions.UpdateOrganisationLateSubmissions(timerInfo, log.Object);
        }

        [Test]
        [Description("Ensure all years are updated")]
        public async Task FunctionsUpdateFile_UpdateOrganisations_Shared_NoYears_UpdatesAllYearsAsync()
        {
            //ARRANGE
            string filePath = Path.Combine(Global.DownloadsPath, Filenames.Organisations);
            int endYear = SectorTypes.Private.GetAccountingStartDate().Year;
            int startYear = Global.FirstReportingYear;
            int expectedFileCount = (endYear - startYear) + 1;

            //ACT
            await _functions.UpdateOrganisationsAsync(filePath);

            //ASSERT
            //Check each return is in the download file
            IEnumerable<string> files = await Global.FileRepository.GetFilesAsync(Global.DownloadsPath);
            Assert.AreEqual(expectedFileCount, files.Count(), $"Expected 2 files in {Global.DownloadsPath}");
            Assert.Multiple(
                () => {
                    for (int year = startYear; year <= endYear; year++)
                    {
                        filePath = Path.Combine(
                            Global.DownloadsPath,
                            $"{Path.GetFileNameWithoutExtension(Filenames.Organisations)}_{year}-{(year + 1).ToTwoDigitYear()}{Path.GetExtension(Filenames.Organisations)}");
                        Assert.That(files.Contains(filePath), $"Expected file for year {year}");
                    }
                });
        }

        [Test]
        [Description("Ensure specific year only is updated")]
        public async Task FunctionsUpdateFile_UpdateOrganisations_Shared_SpecificYear_UpdatesOneYearAsync()
        {
            //ARRANGE
            int endYear = SectorTypes.Private.GetAccountingStartDate().Year;
            int startYear = endYear - 1;

            int year = Numeric.Rand(startYear, endYear);
            string filePath = Path.Combine(
                Global.DownloadsPath,
                $"{Path.GetFileNameWithoutExtension(Filenames.Organisations)}_{year}-{(year + 1).ToTwoDigitYear()}{Path.GetExtension(Filenames.Organisations)}");

            //ACT
            await _functions.UpdateOrganisationsAsync(filePath);

            //ASSERT
            //Check each return is in the download file
            IEnumerable<string> files = await Global.FileRepository.GetFilesAsync(
                Global.DownloadsPath,
                $"{Path.GetFileNameWithoutExtension(Filenames.Organisations)}_*{Path.GetExtension(Filenames.Organisations)}");
            Assert.That(files.Count() == 1, $"Expected only 1 file in {Global.DownloadsPath} got {files.Count()}");
            Assert.That(files.Contains(filePath), $"Expected file for year {year}");
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateOrganisations_TimeTrigger_Ok")]
        public async Task FunctionsUpdateFile_UpdateOrganisations_TimeTrigger_OkAsync()
        {
            var ti = new TimerInfo(new DailySchedule(), new ScheduleStatus());
            await _functions.UpdateOrganisationsAsync(ti);
            throw new NotImplementedException();
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateRegistrationAddresses_Shared_Ok")]
        public void FunctionsUpdateFile_UpdateRegistrationAddresses_Shared_Ok()
        {
            throw new NotImplementedException();

            //_functions.UpdateRegistrationAddressesAsync(filePath: string.Empty,log: log.Object);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateRegistrationAddresses_TimeTrigger_Ok")]
        public void FunctionsUpdateFile_UpdateRegistrationAddresses_TimeTrigger_Ok()
        {
            var timerInfo = new TimerInfo(new DailySchedule(), new ScheduleStatus());

            throw new NotImplementedException();

            //_functions.UpdateRegistrationAddresses(timerInfo, log.Object);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateRegistrations_Shared_Ok")]
        public void FunctionsUpdateFile_UpdateRegistrations_Shared_Ok()
        {
            throw new NotImplementedException();

            //_functions.UpdateRegistrations(filePath: string.Empty, log: log.Object);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateRegistrations_TimeTrigger_Ok")]
        public void FunctionsUpdateFile_UpdateRegistrations_TimeTrigger_Ok()
        {
            var timerInfo = new TimerInfo(new DailySchedule(), new ScheduleStatus());

            throw new NotImplementedException();

            //_functions.UpdateRegistrations(timerInfo, log.Object);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateScopes_Shared_Ok")]
        public void FunctionsUpdateFile_UpdateScopes_Shared_Ok()
        {
            throw new NotImplementedException();

            //_functions.UpdateScopes(filePath: string.Empty);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateScopes_TimeTrigger_Ok")]
        public void FunctionsUpdateFile_UpdateScopes_TimeTrigger_Ok()
        {
            var timerInfo = new TimerInfo(new DailySchedule(), new ScheduleStatus());

            throw new NotImplementedException();

            //_functions.UpdateScopes(timerInfo, log.Object);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateSubmissions_Shared_Ok")]
        public void FunctionsUpdateFile_UpdateSubmissions_Shared_Ok()
        {
            throw new NotImplementedException();

            //_functions.UpdateSubmissions(filePath: string.Empty);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateSubmissions_TimeTrigger_Ok")]
        public void FunctionsUpdateFile_UpdateSubmissions_TimeTrigger_Ok()
        {
            var timerInfo = new TimerInfo(new DailySchedule(), new ScheduleStatus());

            throw new NotImplementedException();

            //_functions.UpdateSubmissions(timerInfo, log.Object);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateUnverifiedRegistrations_Shared_Ok")]
        public void FunctionsUpdateFile_UpdateUnverifiedRegistrations_Shared_Ok()
        {
            throw new NotImplementedException();

            //_functions.UpdateUnverifiedRegistrations(filePath: string.Empty,log: log.Object);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateUnverifiedRegistrations_TimeTrigger_Ok")]
        public void FunctionsUpdateFile_UpdateUnverifiedRegistrations_TimeTrigger_Ok()
        {
            var timerInfo = new TimerInfo(new DailySchedule(), new ScheduleStatus());

            throw new NotImplementedException();

            //_functions.UpdateUnverifiedRegistrations(timerInfo, log.Object);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateUsers_Shared_Ok")]
        public void FunctionsUpdateFile_UpdateUsers_Shared_Ok()
        {
            throw new NotImplementedException();

            //_functions.UpdateUsers(filePath: string.Empty);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateUsers_TimeTrigger_Ok")]
        public void FunctionsUpdateFile_UpdateUsers_TimeTrigger_Ok()
        {
            var timerInfo = new TimerInfo(new DailySchedule(), new ScheduleStatus());

            throw new NotImplementedException();

            //_functions.UpdateUsers(timerInfo, log.Object);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateUsersToSendInfo_Shared_Ok")]
        public void FunctionsUpdateFile_UpdateUsersToSendInfo_Shared_Ok()
        {
            throw new NotImplementedException();

            //_functions.UpdateUsersToSendInfo(filePath: string.Empty);
        }

        [Test]
        [Ignore("Not Implemented")]
        [Description("FunctionsUpdateFile_UpdateUsersToSendInfo_TimeTrigger_Ok")]
        public void FunctionsUpdateFile_UpdateUsersToSendInfo_TimeTrigger_Ok()
        {
            var timerInfo = new TimerInfo(new DailySchedule(), new ScheduleStatus());

            throw new NotImplementedException();

            //_functions.UpdateUsersToSendInfo(timerInfo, log.Object);
        }

    }
}
