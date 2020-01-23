using System;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class SearchLogModel
    {

        public DateTime TimeStamp { get; set; }
        public string QueryTerms { get; set; }
        public int ResultCount { get; set; }
        public string SearchType { get; set; }
        public string SearchParameters { get; set; }

    }
}
