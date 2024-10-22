using System.Collections.Generic;

namespace GenderPayGap.WebUI.Search
{

    internal class RankedViewingSearchOrganisation
    {

        public ViewingSearchResultOrganisationViewModel ViewingSearchResult { get; set; }
        public List<RankedName> Names { get; set; }
        public RankedName TopName { get; set; }

    }

    public class ViewingSearchResultOrganisationViewModel
    {

        public string OrganisationName { get; set; }
        public List<string> OrganisationPreviousNames { get; set; }
        public long OrganisationId { get; set; }
        public string EncryptedId { get; set; }
        public string Address { get; set; }
        public List<string> Sectors { get; set; }

    }

}
