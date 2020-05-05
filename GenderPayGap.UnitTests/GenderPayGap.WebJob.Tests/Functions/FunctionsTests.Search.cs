using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebJob.Tests.TestHelpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OrganisationHelper = GenderPayGap.WebJob.Tests.TestHelpers.OrganisationHelper;

namespace GenderPayGap.WebJob.Tests.Functions
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class UpdateSearchTests
    {

        [SetUp]
        public void BeforeEach()
        {
            //Create 10 test orgs with returns
            IEnumerable<Organisation> orgs = OrganisationHelper.CreateTestOrganisations(10);

            //Instantiate the dependencies
            _functions = WebJobTestHelper.SetUp(orgs);
        }

        private WebJob.Functions _functions;

        [Test]
        [Ignore("incomplete test, needs reviewing to confirm that indexes are NOT recreated as part of the running of this code")]
        public async Task Functions_UpdateSearch()
        {
            // Arrange
            var timespan = new TimeSpan();
            var schedule = new ConstantSchedule(timespan);
            var timer = new TimerInfo(schedule, new ScheduleStatus());

            // Act
            await _functions.UpdateSearchAsync(timer);
        }

        [Test]
        [Ignore("Seems broken")]
        [Description("Check indexes populated OK")]
        public async Task Functions_UpdateSearch_AddAllIndexes()
        {
            //ARRANGE
            //Ensure all orgs are in scope for current year
            IEnumerable<Organisation> orgs = _functions._DataRepository.GetAll<Organisation>();

            //Add a random number of in scope orgs
            int inScope = Numeric.Rand(1, orgs.Count());
            OrganisationHelper.AddScopeStatus(ScopeStatuses.InScope, VirtualDateTime.Now.Year, orgs.Take(inScope).ToArray());

            //Add returns to remaining orgs 
            ReturnHelper.CreateTestReturns(orgs.Skip(inScope).ToArray(), VirtualDateTime.Now.Year);

            orgs = orgs
                .Where(
                    o => o.Status == OrganisationStatuses.Active
                         && (o.Returns.Any(r => r.Status == ReturnStatuses.Submitted)
                             || o.OrganisationScopes.Any(
                                 sc =>
                                     sc.Status == ScopeRowStatuses.Active
                                     && (sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.PresumedInScope))))
                .ToList();

            //ACT
            await _functions.UpdateSearchAsync("testadmin@user.com", true);

            //ASSERT

            //Check for correct number of indexes
            long documentCount = await Global.SearchRepository.GetDocumentCountAsync();
            Assert.That(documentCount == orgs.Count(), $"Expected '{documentCount}' indexes ");

            //Get the actual results
            IList<EmployerSearchModel> actualResults = await Global.SearchRepository.ListAsync();

            //Generate the expected results
            IEnumerable<EmployerSearchModel> expectedResults = orgs.Select(o => o.ToEmployerSearchResult());

            //Check the results
            expectedResults.Compare(actualResults);
        }

    }
}
