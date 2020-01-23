// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using GenderPayGap.IdentityServer4.Classes;
using IdentityServer4.Models;

namespace GenderPayGap.IdentityServer4.Models.Account
{
    public class LoggedOutViewModel
    {

        public string PostLogoutRedirectUri { get; set; }
        public string ClientName { get; set; }
        public string SignOutIframeUrl { get; set; }

        public bool AutomaticRedirectAfterSignOut { get; set; }

        public string LogoutId { get; set; }
        public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;
        public string ExternalAuthenticationScheme { get; set; }

        public Client Client
        {
            get { return Clients.Get().FirstOrDefault(c => c.ClientName == ClientName); }
        }

    }
}
