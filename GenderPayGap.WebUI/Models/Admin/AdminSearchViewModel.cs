using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminSearchViewModel
    {

        public string SearchQuery { get; set; }
        public string Error { get; set; }

        public AdminSearchResultsViewModel SearchResults { get; set; }
        public string OrderBy { get; set; }

    }

    public class AdminSearchResultsViewModel
    {

        public List<AdminSearchResultOrganisationViewModel> OrganisationResults { get; set; }
        public List<AdminSearchResultUserViewModel> UserResults { get; set; }

        public int SearchCacheUpdatedSecondsAgo { get; set; }

    }

    public class AdminSearchResultOrganisationViewModel
    {

        public string OrganisationName { get; set; }
        public List<string> OrganisationPreviousNames { get; set; }
        public long OrganisationId { get; set; }
        public string CompanyNumber { get; set; }
        public OrganisationStatuses Status { get; set; }

    }

    public class AdminSearchResultUserViewModel
    {

        public string UserFullName { get; set; }
        public string UserEmailAddress { get; set; }
        public long UserId { get; set; }
        public UserStatuses Status { get; set; }

    }
    
}
