﻿using System.Collections.Generic;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminSearchViewModel
    {

        public string SearchQuery { get; set; }
        public string Error { get; set; }

        public AdminSearchResultsViewModel SearchResults { get; set; }

    }

    public class AdminSearchResultsViewModel
    {

        public List<AdminSearchResultOrganisationViewModel> OrganisationResults { get; set; }
        public List<AdminSearchResultUserViewModel> UserResults { get; set; }

        public int SearchCacheUpdatedSecondsAgo { get; set; }

    }

    public class AdminSearchResultOrganisationViewModel
    {

        public AdminSearchMatchViewModel OrganisationName { get; set; }
        public List<AdminSearchMatchViewModel> OrganisationPreviousNames { get; set; }
        public long OrganisationId { get; set; }
        public string EmployerRef { get; set; }
        public string CompanyNumber { get; set; }
        public OrganisationStatuses Status { get; set; }

    }

    public class AdminSearchResultUserViewModel
    {

        public AdminSearchMatchViewModel UserFullName { get; set; }
        public AdminSearchMatchViewModel UserEmailAddress { get; set; }
        public long UserId { get; set; }
        public UserStatuses Status { get; set; }

    }

    public class AdminSearchMatchViewModel
    {

        public string Text { get; set; }
        public List<AdminSearchMatchGroupViewModel> MatchGroups { get; set; }

    }

    public class AdminSearchMatchGroupViewModel
    {

        public int Start { get; set; }
        public int Length { get; set; }

    }
}
