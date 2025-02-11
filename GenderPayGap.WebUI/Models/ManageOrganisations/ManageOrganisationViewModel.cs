using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;

namespace GenderPayGap.WebUI.Models.ManageOrganisations
{
    public class ManageOrganisationViewModel
    {

        public Organisation Organisation { get; }
        public User User { get; }

        private readonly List<DraftReturn> allDraftReturns;

        public ManageOrganisationViewModel(Organisation organisation, User user, List<DraftReturn> allDraftReturns)
        {
            Organisation = organisation;
            User = user;
            this.allDraftReturns = allDraftReturns;
        }

        public List<User> GetFullyRegisteredUsersForOrganisationWithCurrentUserFirst()
        {
            List<User> users = Organisation.UserOrganisations
                .Where(uo => uo.PINConfirmedDate.HasValue)
                .Select(uo => uo.User)
                .OrderBy(user => user.Fullname)
                .ToList();

            // The current user must be in this list (otherwise we wouldn't be able to visit this page)
            // So, remove the user from wherever they are n the list
            // And insert them at the start of the list
            users.Remove(User);
            users.Insert(0, User);

            return users;
        }

        public bool DoesReturnOrDraftReturnExistForYear(int reportingYear)
        {
            bool hasReturnForYear = Organisation.HasSubmittedReturn(reportingYear);
            bool hasDraftReturnForYear = allDraftReturns.Any(d => d.SnapshotYear == reportingYear);

            return hasReturnForYear || hasDraftReturnForYear;
        }
        
        public string GetReportLinkText(int reportingYear)
        {
            bool hasReturnForYear = Organisation.HasSubmittedReturn(reportingYear);
            bool hasDraftReturnForYear = allDraftReturns.Any(d => d.SnapshotYear == reportingYear);

            if (!hasReturnForYear && !hasDraftReturnForYear)
            {
                return "Draft report";
            }

            if (!hasReturnForYear && hasDraftReturnForYear)
            {
                return "Edit draft";
            }

            if (hasReturnForYear && !hasDraftReturnForYear)
            {
                return "Edit report";
            }

            return "Edit draft report";
        }

        public bool OrganisationIsRequiredToSubmit(int reportingYear)
        {
            OrganisationScope scopeForYear = Organisation.GetScopeForYear(reportingYear);
            if (scopeForYear == null)
            {
                return true;
            }
            
            return scopeForYear.IsInScopeVariant()
                   && !Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(reportingYear);
        }

    }
}
