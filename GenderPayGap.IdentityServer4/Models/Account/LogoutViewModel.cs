// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using GenderPayGap.IdentityServer4.Classes;
using IdentityServer4.Models;

namespace GenderPayGap.IdentityServer4.Models.Account
{
    public class LogoutViewModel : LogoutInputModel
    {

        public bool ShowLogoutPrompt { get; set; }
        public string ClientName { get; set; }

        public Client Client
        {
            get { return Clients.Get().FirstOrDefault(c => c.ClientName == ClientName); }
        }

    }
}
