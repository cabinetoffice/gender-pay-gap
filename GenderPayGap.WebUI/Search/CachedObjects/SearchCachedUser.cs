using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Search.CachedObjects {

    internal class SearchCachedUser
    {
        public long UserId { get; set; }
        public SearchReadyValue FullName { get; set; }
        public SearchReadyValue EmailAddress { get; set; }
        public UserStatuses Status { get; set; }
    }
}
