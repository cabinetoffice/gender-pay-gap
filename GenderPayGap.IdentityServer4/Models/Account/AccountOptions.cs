// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace GenderPayGap.IdentityServer4.Models.Account
{
    public class AccountOptions
    {

        public static bool AllowRememberLogin = false;
        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(7);

        public static bool ShowLogoutPrompt = false;
        public static bool AutomaticRedirectAfterSignOut = false;

        public static string TooManySigninAttemptsErrorMessage = "Too many failed sign in attempts.";
        public static string InvalidCredentialsErrorMessage = "Please enter your email address and password again.";
        public static string CannotVerifyEmailUsingDifferentAccount = "You cannot verify an email using a different account.";

    }
}
