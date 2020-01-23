using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using IdentityServer4;
using IdentityServer4.Models;

namespace GenderPayGap.IdentityServer4.Classes
{
    public static class Clients
    {

        private static string GpgClientSecret => Config.GetAppSetting("AuthSecret", "secret");

        public static IEnumerable<Client> Get()
        {
            if ((Config.IsProduction() || Config.IsPreProduction()) && GpgClientSecret.EqualsI("secret", "", null))
            {
                throw new Exception("Invalid ClientSecret for IdentityServer. You must set 'AuthSecret' to a unique key");
            }

            return new[] {
                new Client {
                    ClientName = "Gender pay gap reporting service",
                    ClientId = "gpgWeb",
                    ClientSecrets = new List<Secret> {new Secret(GpgClientSecret.GetSHA256Checksum())},
                    ClientUri = Startup.SiteAuthority,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris =
                        new List<string> {
                            Startup.SiteAuthority,
                            Startup.SiteAuthority + "signin-oidc",
                            Startup.SiteAuthority + "manage-organisations",
                            Config.Configuration["DoneUrl"]
                        },
                    PostLogoutRedirectUris =
                        new List<string> {
                            Config.SiteAuthority,
                            Config.SiteAuthority + "signout-callback-oidc",
                            Config.SiteAuthority + "manage-organisations",
                            Config.SiteAuthority + "manage-account/complete-change-email",
                            Config.SiteAuthority + "manage-account/close-account-completed",
                            Config.Configuration["DoneUrl"]
                        },
                    AllowedScopes =
                        new List<string> {
                            IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "roles"
                        },
                    Properties = new Dictionary<string, string> {{"AutomaticRedirectAfterSignOut", "true"}}
                }
            };
        }

    }
}
