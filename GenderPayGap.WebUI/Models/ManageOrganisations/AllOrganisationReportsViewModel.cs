using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Models.Admin;

namespace GenderPayGap.WebUI.Models.ManageOrganisations
{
    public class AllOrganisationReportsViewModel
    {

        public Database.Organisation Organisation { get; }
        public User User { get; }

        private readonly List<DraftReturn> allDraftReturns;

        public AllOrganisationReportsViewModel(Database.Organisation organisation, User user, List<DraftReturn> allDraftReturns, int? currentPage, int? totalPages, int? entriesPerPage)
        {
            Organisation = organisation;
            User = user;
            this.allDraftReturns = allDraftReturns;
            CurrentPage = currentPage;
            TotalPages = totalPages;
            EntriesPerPage = entriesPerPage;
        }
        
        public int? CurrentPage { get; set; }
        public int? TotalPages { get; set; }
        public int? EntriesPerPage { get; set; }

        public List<User> GetFullyRegisteredUsersForOrganisationWithCurrentUserFirst()
        {
            List<User> users = Organisation.UserOrganisations
                .Where(uo => uo.PINConfirmedDate.HasValue)
                .Select(uo => uo.User)
                .ToList();

            // The current user must be in this list (otherwise we wouldn't be able to visit this page)
            // So, remove the user from wherever they are n the list
            // And insert them at the start of the list
            users.Remove(User);
            users.Insert(0, User);

            return users;
        }

        public List<ManageOrganisationDetailsForYearViewModel> GetOrganisationDetailsForYears()
        {
            List<int> reportingYears = ReportingYearsHelper.GetReportingYears();
            var detailsForYears = new List<ManageOrganisationDetailsForYearViewModel>();

            foreach (int reportingYear in reportingYears)
            {
                // Get organisation's scope for given reporting year
                var scopeForYear = Organisation.GetScopeForYear(reportingYear);

                if (scopeForYear != null)
                {
                    detailsForYears.Add(
                        new ManageOrganisationDetailsForYearViewModel(
                            Organisation,
                            reportingYear,
                            allDraftReturns.Where(d => d.SnapshotYear == reportingYear)
                                .OrderByDescending(d => d.Modified)
                                .FirstOrDefault()
                        )
                    );
                }
            }

            return detailsForYears;
        }
    }
}
