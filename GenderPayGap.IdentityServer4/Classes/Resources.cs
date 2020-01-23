using System.Collections.Generic;
using System.Security.Claims;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
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

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource> {
                new ApiResource {
                    Name = Config.Configuration["GpgApiScope"],
                    DisplayName = "GPG API",
                    Description = "Access to a GPG API",
                    UserClaims = new List<string> {ClaimTypes.Role},
                    ApiSecrets = new List<Secret> {new Secret(Config.GetAppSetting("AuthSecret", "secret").GetSHA256Checksum())},
                    Scopes = new List<Scope> {new Scope("customAPI.read"), new Scope("customAPI.write")}
                }
            };
        }

    }
}
