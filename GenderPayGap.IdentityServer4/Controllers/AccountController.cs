// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.IdentityServer4.Models.Account;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace GenderPayGap.IdentityServer4.Controllers
{
    /// <summary>
    ///     This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    ///     The login service encapsulates the interactions with the user data store. This data store is in-memory only and
    ///     cannot be used for production!
    ///     The interaction service provides a way for the UI to communicate with identityserver for validation and context
    ///     retrieval
    /// </summary>
    public class AccountController : Controller
    {

        private readonly IIdentityServerInteractionService _interaction;
        private readonly IUserRepository _userRepository;
        protected readonly IDistributedCache _cache;
        protected readonly IEventService _events;


        public AccountController(
            IIdentityServerInteractionService interaction,
            IUserRepository userRepository,
            IEventService events,
            IDistributedCache cache)
        {
            _userRepository = userRepository;
            _interaction = interaction;
            _events = events;
            _cache = cache;
        }


        [Route("~/")]
        public IActionResult Redirect()
        {
            return RedirectToAction("Login");
        }

        /// <summary>
        ///     Show login page
        /// </summary>
        [HttpGet]
        [Route("~/sign-in")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(Startup.SiteAuthority + "manage-organisations");
            }

            // build a model so we know what to show on the login page
            LoginInputModel vm = new LoginInputModel
            {
                ReturnUrl = returnUrl,
            };

            // Check if we are verifying a email change request
            AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (IsReferrerChangeEmailVerification(context, out ChangeEmailVerificationToken changeEmailToken))
            {
                // auto populate new email address
                vm.Username = changeEmailToken.NewEmailAddress;
            }

            return View(vm);
        }

        /// <summary>
        ///     Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("~/sign-in")]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            if (button != "login")
            {
                // the user clicked the "cancel" button
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(model.ReturnUrl);
                }

                // since we don't have a valid context, then we just go back to the home page
                return Redirect("~/");
            }

            if (ModelState.IsValid)
            {
                model.Username = model.Username.ToLower();
                User user = null;

                // Check if we are verifying a email change request
                if (IsReferrerChangeEmailVerification(context, out ChangeEmailVerificationToken changeEmailToken))
                {
                    if (model.Username.ToLower() == changeEmailToken.NewEmailAddress.ToLower())
                    {
                        // try to match any new or active users by id
                        user = await _userRepository.FindBySubjectIdAsync(
                            changeEmailToken.UserId,
                            UserStatuses.New,
                            UserStatuses.Active);
                    }
                    else
                    {
                        ModelState.AddModelError("", AccountOptions.CannotVerifyEmailUsingDifferentAccount);
                    }
                }
                else
                {
                    user = await _userRepository.FindByEmailAsync(model.Username, UserStatuses.New, UserStatuses.Active);
                }

                if (user != null)
                {
                    if (/*user.LockRemaining*/ TimeSpan.Zero > TimeSpan.Zero)
                    {
                        await _events.RaiseAsync(
                            new UserLoginFailureEvent(model.Username, AccountOptions.TooManySigninAttemptsErrorMessage));
                        ModelState.AddModelError(
                            "",
                            $"{AccountOptions.TooManySigninAttemptsErrorMessage}<br/>Please try again in {/*user.LockRemaining*/ TimeSpan.Zero.ToFriendly(maxParts: 2)}.");
                    }
                    else if (await _userRepository.CheckPasswordAsync(user, model.Password) == false)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "Wrong password"));
                        ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
                    }
                    else
                    {
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.EmailAddress, user.UserId.ToString(), user.Fullname));

                        // set the user role
                        var claims = new List<Claim>();

                        if (user.Status == UserStatuses.New || user.Status == UserStatuses.Active)
                        {
                            claims.Add(
                                new Claim(
                                    ClaimTypes.Role,
                                    user.IsAdministrator() ? "GPGadmin" : "GPGemployer"));
                        }

                        // issue authentication cookie with subject ID and username
                        AuthenticationProperties props = null;
                        await HttpContext.SignInAsync(user.UserId.ToString(), user.EmailAddress, props, claims.ToArray());

                        // make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
                        // the IsLocalUrl check is only necessary if you want to support additional local pages, otherwise IsValidReturnUrl is more strict

                        if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }

                        //Return to the default root
                        return Redirect("~/");
                    }
                }
                else
                {
                    //Prompt unknown users same as with known users to prevent guessing of valid usernames
                    string login = await _cache.GetStringAsync($"{model.Username}:login");

                    DateTime loginDate = string.IsNullOrWhiteSpace(login) ? DateTime.MinValue : login.BeforeFirst("|").ToDateTime();
                    int loginAttempts = string.IsNullOrWhiteSpace(login) ? 0 : login.AfterFirst("|").ToInt32();
                    TimeSpan lockRemaining = loginDate == DateTime.MinValue
                        ? TimeSpan.Zero
                        : loginDate.AddMinutes(Global.LockoutMinutes) - VirtualDateTime.Now;
                    if (loginAttempts >= Global.MaxLoginAttempts && lockRemaining > TimeSpan.Zero)
                    {
                        await _events.RaiseAsync(
                            new UserLoginFailureEvent(model.Username, AccountOptions.TooManySigninAttemptsErrorMessage));
                        ModelState.AddModelError(
                            "",
                            $"{AccountOptions.TooManySigninAttemptsErrorMessage}<br/>Please try again in {lockRemaining.ToFriendly(maxParts: 2)}.");
                    }
                    else
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "Invalid user"));
                        ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
                        loginAttempts++;
                        var cacheOptions = new DistributedCacheEntryOptions {
                            AbsoluteExpiration = VirtualDateTime.Now.AddMinutes(Global.LockoutMinutes)
                        };
                        await _cache.SetStringAsync($"{model.Username}:login", $"{VirtualDateTime.Now}|{loginAttempts}", cacheOptions);
                    }
                }
            }

            // something went wrong, show form with error
            return View(model);
        }

        /// <summary>
        ///     Show logout page
        /// </summary>
        [HttpGet]
        [Route("~/sign-out")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            //If there is no logoutid then sign-out via webui
            if (string.IsNullOrWhiteSpace(logoutId))
            {
                return Redirect(Startup.SiteAuthority + "sign-out");
            }

            // if the request for logout was properly authenticated from IdentityServer, then
            // we don't need to show the prompt and can just log the user out directly.
            return await LogoutPost(logoutId);
        }

        /// <summary>
        ///     Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("~/sign-out")]
        public async Task<IActionResult> LogoutPost(string logoutId)
        {
            // build a model so the logged out page knows what to display
            LoggedOutViewModel vm = await BuildLoggedOutViewModelAsync(logoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                var options = new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    Path = "/account",
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UnixEpoch
                };
                HttpContext.Response.Cookies.Append("idsrv", "", options);
                HttpContext.Response.Cookies.Append("idsrv.session", "", options);

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            //Automatically redirect
            if (vm.AutomaticRedirectAfterSignOut && !string.IsNullOrWhiteSpace(vm.PostLogoutRedirectUri))
            {
                return Redirect(vm.PostLogoutRedirectUri);
            }

            return View("LoggedOut", vm);
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            LogoutRequest logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
            };

            //Get the logout properties per client 
            if (vm.Client.Properties.ContainsKey("AutomaticRedirectAfterSignOut"))
            {
                vm.AutomaticRedirectAfterSignOut = vm.Client.Properties["AutomaticRedirectAfterSignOut"]
                    .ToBoolean(AccountOptions.AutomaticRedirectAfterSignOut);
            }

            return vm;
        }

        private bool IsReferrerChangeEmailVerification(AuthorizationRequest authRequest, out ChangeEmailVerificationToken changeEmailToken)
        {
            // Check if the referring url is an email change verification
            string referrerPathAndQuery = authRequest.Parameters["Referrer"];
            if (referrerPathAndQuery != null && referrerPathAndQuery.StartsWith("/manage-account/complete-change-email"))
            {
                string query = referrerPathAndQuery.AfterFirst("?");
                NameValueCollection queryDict = HttpUtility.ParseQueryString(query);
                string code = queryDict["code"];

                changeEmailToken = Encryption.DecryptModel<ChangeEmailVerificationToken>(code);
                return true;
            }

            changeEmailToken = null;
            return false;
        }

    }

}
