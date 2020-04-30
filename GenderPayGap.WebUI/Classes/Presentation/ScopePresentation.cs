using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Models.Scope;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Classes.Services
{

    public interface IScopePresentation
    {

        // inteface
        ScopingViewModel CreateScopingViewModel(Organisation org, User currentUser);
        Task<OrganisationViewModel> CreateOrganisationViewModelAsync(EnterCodesViewModel enterCodes, User currentUser);
        Task<ScopingViewModel> CreateScopingViewModelAsync(EnterCodesViewModel enterCodes, User currentUser);
        Task SaveScopesAsync(ScopingViewModel model, IEnumerable<int> snapshotYears);
        Task SavePresumedScopeAsync(ScopingViewModel model, int reportingStartYear);

    }

    public class ScopePresentation : IScopePresentation
    {

        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;

        public ScopePresentation(IScopeBusinessLogic scopeBL,
            IDataRepository dataRepo,
            IOrganisationBusinessLogic organisationBusinessLogic)
        {
            ScopeBusinessLogic = scopeBL;
            DataRepository = dataRepo;
            _organisationBusinessLogic = organisationBusinessLogic;
        }

        private IScopeBusinessLogic ScopeBusinessLogic { get; }
        private IDataRepository DataRepository { get; }

        public virtual async Task<OrganisationViewModel> CreateOrganisationViewModelAsync(EnterCodesViewModel enterCodes, User currentUser)
        {
            Organisation org = await _organisationBusinessLogic.GetOrganisationByEmployerReferenceAndSecurityCodeAsync(
                enterCodes.EmployerReference,
                enterCodes.SecurityToken);
            if (org == null)
            {
                return null;
            }

            var model = new OrganisationViewModel();
            // when SecurityToken is expired then model.SecurityCodeExpired should be true
            model.IsSecurityCodeExpired = org.HasSecurityCodeExpired();

            model.Employers = new PagedResult<EmployerRecord>();
            model.Employers.Results = new List<EmployerRecord> {org.ToEmployerRecord()};
            model.SelectedEmployerIndex = 0;

            //Mark the organisation as authorised
            model.SelectedAuthorised = true;
            model.IsFastTrackAuthorised = true;

            return model;
        }

        public virtual async Task<ScopingViewModel> CreateScopingViewModelAsync(EnterCodesViewModel enterCodes, User currentUser)
        {
            // when NonStarterOrg doesn't exist then return
            return null;
        }

        public virtual ScopingViewModel CreateScopingViewModel(Organisation org, User currentUser)
        {
            if (org == null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            var model = new ScopingViewModel {
                OrganisationId = org.OrganisationId,
                OrganisationName = org.OrganisationName,
                OrganisationAddress = org.GetLatestAddress()?.GetAddressString(),
                AccountingDate = org.SectorType.GetAccountingStartDate()
            };
            model.EnterCodes.EmployerReference = org.EmployerReference;

            // get the scope info for this year
            OrganisationScope scope = ScopeBusinessLogic.GetLatestScopeBySnapshotYear(org, model.AccountingDate.Year);
            if (scope != null)
            {
                model.ThisScope = new ScopeViewModel {
                    OrganisationScopeId = scope.OrganisationScopeId,
                    ScopeStatus = scope.ScopeStatus,
                    StatusDate = scope.ScopeStatusDate,
                    RegisterStatus = scope.RegisterStatus,
                    SnapshotDate = scope.SnapshotDate
                };
            }

            // get the scope info for last year
            scope = ScopeBusinessLogic.GetLatestScopeBySnapshotYear(org, model.AccountingDate.Year - 1);
            if (scope != null)
            {
                model.LastScope = new ScopeViewModel {
                    OrganisationScopeId = scope.OrganisationScopeId,
                    ScopeStatus = scope.ScopeStatus,
                    StatusDate = scope.ScopeStatusDate,
                    RegisterStatus = scope.RegisterStatus,
                    SnapshotDate = scope.SnapshotDate
                };
            }

            //Check if the user is registered for this organisation
            model.UserIsRegistered = currentUser != null && org.UserOrganisations.Any(uo => uo.UserId == currentUser.UserId);

            return model;
        }

        public virtual async Task SaveScopesAsync(ScopingViewModel model, IEnumerable<int> snapshotYears)
        {
            if (string.IsNullOrWhiteSpace(model.EnterCodes.EmployerReference))
            {
                throw new ArgumentNullException(nameof(model.EnterCodes.EmployerReference));
            }

            if (model.IsSecurityCodeExpired)
            {
                throw new ArgumentOutOfRangeException(nameof(model.IsSecurityCodeExpired));
            }

            if (!snapshotYears.Any())
            {
                throw new ArgumentNullException(nameof(snapshotYears));
            }

            //Get the organisation with this employer reference
            Organisation org = model.OrganisationId == 0
                ? null
                : await DataRepository.GetAll<Organisation>().FirstOrDefaultAsync(o => o.OrganisationId == model.OrganisationId);
            if (org == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(model.OrganisationId),
                    $"Cannot find organisation with Id: {model.OrganisationId} in the database");
            }

            var newScopes = new List<OrganisationScope>();

            foreach (int snapshotYear in snapshotYears.OrderByDescending(y => y))
            {
                var scope = new OrganisationScope {
                    OrganisationId = org.OrganisationId,
                    ContactEmailAddress = model.EnterAnswers.EmailAddress,
                    ContactFirstname = model.EnterAnswers.FirstName,
                    ContactLastname = model.EnterAnswers.LastName,
                    ReadGuidance = model.EnterAnswers.HasReadGuidance(),
                    Reason = model.EnterAnswers.Reason != "Other" ? model.EnterAnswers.Reason : model.EnterAnswers.OtherReason,
                    ScopeStatus = model.IsOutOfScopeJourney ? ScopeStatuses.OutOfScope : ScopeStatuses.InScope,
                    CampaignId = model.CampaignId,
                    // set the snapshot date according to sector
                    SnapshotDate = org.SectorType.GetAccountingStartDate(snapshotYear)
                };
                newScopes.Add(scope);
            }

            await ScopeBusinessLogic.SaveScopesAsync(org, newScopes);
        }

        public virtual async Task SavePresumedScopeAsync(ScopingViewModel model, int snapshotYear)
        {
            if (string.IsNullOrWhiteSpace(model.EnterCodes.EmployerReference))
            {
                throw new ArgumentNullException(nameof(model.EnterCodes.EmployerReference));
            }

            if (model.IsSecurityCodeExpired)
            {
                throw new ArgumentOutOfRangeException(nameof(model.IsSecurityCodeExpired));
            }

            // get the organisation by EmployerReference
            Organisation org = await GetOrgByEmployerReferenceAsync(model.EnterCodes.EmployerReference);
            if (org == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(model.EnterCodes.EmployerReference),
                    $"Cannot find organisation with EmployerReference: {model.EnterCodes.EmployerReference} in the database");
            }

            // can only save a presumed scope in the prev or current snapshot year
            DateTime currentSnapshotDate = org.SectorType.GetAccountingStartDate();
            if (snapshotYear > currentSnapshotDate.Year || snapshotYear < currentSnapshotDate.Year - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(snapshotYear));
            }

            // skip saving a presumed scope when an active scope already exists for the snapshot year
            if (await ScopeBusinessLogic.GetLatestScopeBySnapshotYearAsync(org.OrganisationId, snapshotYear) != null)
            {
                return;
            }

            // create the new OrganisationScope
            var newScope = new OrganisationScope {
                OrganisationId = org.OrganisationId,
                ContactEmailAddress = model.EnterAnswers.EmailAddress,
                ContactFirstname = model.EnterAnswers.FirstName,
                ContactLastname = model.EnterAnswers.LastName,
                ReadGuidance = model.EnterAnswers.HasReadGuidance(),
                Reason = "",
                ScopeStatus = model.IsOutOfScopeJourney ? ScopeStatuses.PresumedOutOfScope : ScopeStatuses.PresumedInScope,
                CampaignId = model.CampaignId,
                // set the snapshot date according to sector
                SnapshotDate = org.SectorType.GetAccountingStartDate(snapshotYear),
                StatusDetails = "Generated by the system"
            };

            // save the presumed scope
            await ScopeBusinessLogic.SaveScopeAsync(org, true, newScope);
        }

        public async Task<Organisation> GetOrgByEmployerReferenceAsync(string employerReference)
        {
            Organisation org = await DataRepository.GetAll<Organisation>()
                .FirstOrDefaultAsync(o => o.EmployerReference == employerReference);
            return org;
        }

    }
}
