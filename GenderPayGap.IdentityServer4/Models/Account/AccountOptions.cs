// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Microsoft.AspNetCore.Server.IISIntegration;

namespace GenderPayGap.IdentityServer4.Models.Account
{
    public class AccountOptions
    {

        public static bool AllowLocalLogin = true;
        public static bool AllowRememberLogin = false;
        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(7);

        public static bool ShowLogoutPrompt = false;
        public static bool AutomaticRedirectAfterSignOut = false;

        // specify the Windows authentication scheme being used
        public static readonly string WindowsAuthenticationSchemeName = IISDefaults.AuthenticationScheme;

        // if user uses windows auth, should we load the groups from windows
        public static bool IncludeWindowsGroups = false;

        public static string TooManySigninAttemptsErrorMessage = "Too many failed sign in attempts.";
        public static string InvalidCredentialsErrorMessage = "Please enter your email address and password again.";
        public static string CannotVerifyEmailUsingDifferentAccount = "You cannot verify an email using a different account.";

    }
}
