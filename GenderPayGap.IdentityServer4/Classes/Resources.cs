using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4.Models;

namespace GenderPayGap.IdentityServer4.Classes
{
    internal class Resources
    {

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource> {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource {Name = "roles", UserClaims = new List<string> {ClaimTypes.Role}}
            };
        }

    }
}
