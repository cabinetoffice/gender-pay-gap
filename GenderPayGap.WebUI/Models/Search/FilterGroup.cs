using System.Collections.Generic;

namespace GenderPayGap.WebUI.Models.Search
{

    public class FilterGroup
    {

        public string Id { get; set; }
        public string Group { get; set; }
        public string Label { get; set; }
        public bool Expanded { get; set; }
        public string MaxHeight { get; set; }
        public List<OptionSelect> Metadata { get; set; }

    }

}
