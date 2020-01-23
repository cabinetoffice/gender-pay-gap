using System.Collections.Generic;

namespace GenderPayGap.IdentityServer4.Classes
{
    public class ClientConfigModel
    {

        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public ICollection<string> AllowedScopes { get; set; }
        public bool RequireConsent { get; set; }
        public ICollection<string> AllowedGrantTypes { get; set; }
        public string Authority { get; set; }
        public ICollection<string> RedirectUris { get; set; }
        public ICollection<string> PostLogoutRedirectUris { get; set; }

    }
}
