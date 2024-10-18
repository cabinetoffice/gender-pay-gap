using System.Collections.Generic;

namespace GenderPayGap.WebUI.Models.Search
{
    public class SearchApiResult
    {

        public SearchPageViewModel SearchParameters { get; set; }
        public List<SearchApiResultEmployer> Employers { get; set; }

    }

    public class SearchApiResultEmployer
    {

        public long Id { get; set; }
        public string EncId { get; set; }
        public string Name { get; set; }
        public string PreviousName { get; set; }
        public string Address { get; set; }
        public string Sector { get; set; }

    }
}
