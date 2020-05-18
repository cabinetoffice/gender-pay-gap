using System.Collections.Generic;

namespace GenderPayGap.WebUI.Models.Search
{

    public class SuggestOrganisationResult
    {

        public string Id { get; set; }
        public string Text { get; set; }
        public string PreviousName { get; set; }
        public List<double> Rank { get; set; }

    }

}
