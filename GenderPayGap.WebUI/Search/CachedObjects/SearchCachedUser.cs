using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Search.CachedObjects {

    internal class SearchCachedUser
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public UserStatuses Status { get; set; }
    }
}
