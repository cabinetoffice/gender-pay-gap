using System;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.Models.ManageOrganisations;
using GenderPayGap.WebUI.Tests.TestHelpers;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Views.ManageOrganisations
{
    [TestFixture]
    public class ManageOrganisation
    {
        private static readonly int ReportingYear = DateTime.Now.Year;
        private const int PastReportingYear = 2018;
        private readonly User user = UserHelpers.CreateUsers().Find(u => u.UserId == 24572);
        
        private readonly Organisation organisationOutOfScope = OrganisationHelper.GetOrganisationInAGivenScope(ScopeStatuses.OutOfScope, null, ReportingYear);
        private readonly Organisation organisationInScope = OrganisationHelper.GetOrganisationInAGivenScope(ScopeStatuses.InScope, null, ReportingYear);
        private readonly Organisation organisationInScopeForPastYear = OrganisationHelper.GetOrganisationInAGivenScope(ScopeStatuses.InScope, null, PastReportingYear);
        
        
        [SetUp]
        public void Setup()
        {
            VirtualDateTime.Initialise(TimeSpan.Zero);
        }
        
        [Test]
        public void WhenOnManageOrganisationPage_IfOrganisationIsNotRequiredToReport_ThenReportStatusIsNotRequired()
        {
            var viewModel = new ManageOrganisationDetailsForYearViewModel(organisationOutOfScope, ReportingYear, new DraftReturn());

            string reportTagText = viewModel.GetReportTagText();
            
            Assert.AreEqual("Report not required", reportTagText);
        }
        
        [Test]
        public void WhenOnManageOrganisationPage_IfHasNotSubmittedBeforeTheDeadline_ThenReportStatusIsDueByDate()
        {
            var viewModel = new ManageOrganisationDetailsForYearViewModel(organisationInScope, ReportingYear, null);

            string reportTagText = viewModel.GetReportTagText();
            
            Assert.True(reportTagText.Contains("Report due by"));
        }
        
        [Test]
        public void WhenOnManageOrganisationPage_IfHasNotSubmittedAfterTheDeadline_ThenReportStatusIsOverdue()
        {
            var viewModel = new ManageOrganisationDetailsForYearViewModel(organisationInScopeForPastYear, PastReportingYear, null);

            string reportTagText = viewModel.GetReportTagText();
            
            Assert.AreEqual("Report overdue", reportTagText);
        }
        
        [Test]
        public void WhenOnManageOrganisationPage_IfHasSubmittedBeforeTheDeadline_ThenReportStatusSubmitted()
        {
            UserOrganisation userOrg = UserOrganisationHelper.LinkUserWithOrganisation(user, organisationInScope);
            Return ret = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(userOrg, ReportingYear);
            OrganisationHelper.LinkOrganisationAndReturn(organisationInScope, ret);

            var viewModel = new ManageOrganisationDetailsForYearViewModel(organisationInScope, ReportingYear, null);

            string reportTagText = viewModel.GetReportTagText();
            
            Assert.AreEqual("Report submitted", reportTagText);
        }
        
        [Test]
        public void WhenOnManageOrganisationPage_IfHasSubmittedAfterTheDeadline_ThenReportStatusSubmittedLate()
        {
            Return ret = ReturnHelper.CreateLateReturn(PastReportingYear, organisationInScopeForPastYear, 3);
            OrganisationHelper.LinkOrganisationAndReturn(organisationInScopeForPastYear, ret);

            var viewModel = new ManageOrganisationDetailsForYearViewModel(organisationInScopeForPastYear, PastReportingYear, null);

            string reportTagText = viewModel.GetReportTagText();
            
            Assert.AreEqual("Report submitted late", reportTagText);
        }

    }
}
