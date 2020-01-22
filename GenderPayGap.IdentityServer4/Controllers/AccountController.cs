// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.IdentityServer4.Classes;
using GenderPayGap.IdentityServer4.Models.Account;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.IdentityServer4.Controllers
{
    /// <summary>
    ///     This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    ///     The login service encapsulates the interactions with the user data store. This data store is in-memory only and
    ///     cannot be used for production!
    ///     The interaction service provides a way for the UI to communicate with identityserver for validation and context
    ///     retrieval
    /// </summary>
    public class AccountController : BaseController
    {

        private readonly IClientStore _clientStore;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IUserRepository _userRepository;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IUserRepository userRepository,
            IEventService events,
            IDistributedCache cache,
            ILogger<AccountController> logger) : base(events, cache, logger)
        {
            _userRepository = userRepository;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
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
            LoginViewModel vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return await ExternalLogin(vm.ExternalLoginScheme, returnUrl);
            }

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
                    if (user.LockRemaining > TimeSpan.Zero)
                    {
                        await _events.RaiseAsync(
                            new UserLoginFailureEvent(model.Username, AccountOptions.TooManySigninAttemptsErrorMessage));
                        ModelState.AddModelError(
                            "",
                            $"{AccountOptions.TooManySigninAttemptsErrorMessage}<br/>Please try again in {user.LockRemaining.ToFriendly(maxParts: 2)}.");
                    }
                    else if (await _userRepository.CheckPasswordAsync(user, model.Password) == false)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "Wrong password"));
                        ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
                    }
                    else
                    {
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.EmailAddress, user.UserId.ToString(), user.Fullname));

                        // only set explicit expiration here if user chooses "remember me". 
                        // otherwise we rely upon expiration configured in cookie middleware.
                        AuthenticationProperties props = null;
                        if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                        {
                            props = new AuthenticationProperties {
                                IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                            };
                        }

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
                    //Note: Its OK here to use the local web cache since sticky sessions are used by Azure
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
            LoginViewModel vm = await BuildLoginViewModelAsync(model);
            return View(vm);
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

            // build a model so the logout page knows what to display
            LogoutViewModel vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        ///     Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("~/sign-out")]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            LoggedOutViewModel vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new {logoutId = vm.LogoutId});

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties {RedirectUri = url}, vm.ExternalAuthenticationScheme);
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
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                // this is meant to short circuit the UI and only trigger the one external IdP
                return new LoginViewModel {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = new[] {new ExternalProvider {AuthenticationScheme = context.IdP}}
                };
            }

            IEnumerable<AuthenticationScheme> schemes = await _schemeProvider.GetAllSchemesAsync();

            List<ExternalProvider> providers = schemes
                .Where(
                    x => x.DisplayName != null
                         || x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                .Select(x => new ExternalProvider {DisplayName = x.DisplayName, AuthenticationScheme = x.Name})
                .ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                Client client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme))
                            .ToList();
                    }
                }
            }

            return new LoginViewModel {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            LoginViewModel vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            LogoutRequest logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LogoutViewModel {
                ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt,
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                LogoutId = logoutId
            };

            //Get the logout properties per client 
            if (vm.Client.Properties.ContainsKey("AutomaticRedirectAfterSignOut"))
            {
                vm.AutomaticRedirectAfterSignOut = vm.Client.Properties["AutomaticRedirectAfterSignOut"]
                    .ToBoolean(AccountOptions.AutomaticRedirectAfterSignOut);
            }

            if (vm.Client.Properties.ContainsKey("ShowLogoutPrompt"))
            {
                vm.ShowLogoutPrompt = vm.Client.Properties["ShowLogoutPrompt"].ToBoolean(AccountOptions.ShowLogoutPrompt);
            }

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            if (logout?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            LogoutRequest logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            //Get the logout properties per client 
            if (vm.Client.Properties.ContainsKey("AutomaticRedirectAfterSignOut"))
            {
                vm.AutomaticRedirectAfterSignOut = vm.Client.Properties["AutomaticRedirectAfterSignOut"]
                    .ToBoolean(AccountOptions.AutomaticRedirectAfterSignOut);
            }

            if (User?.Identity.IsAuthenticated == true)
            {
                string idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    bool providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            // see if windows auth has already been requested and succeeded
            AuthenticateResult result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                // we will issue the external cookie and then redirect the
                // user back to the external callback, in essence, tresting windows
                // auth the same as any other external authentication mechanism
                var props = new AuthenticationProperties {
                    RedirectUri = Url.Action("ExternalLoginCallback"),
                    Items = {{"returnUrl", returnUrl}, {"scheme", AccountOptions.WindowsAuthenticationSchemeName}}
                };

                var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                // add the groups as claims -- be careful if the number of groups is too large
                if (AccountOptions.IncludeWindowsGroups)
                {
                    var wi = wp.Identity as WindowsIdentity;
                    IdentityReferenceCollection groups = wi.Groups.Translate(typeof(NTAccount));
                    IEnumerable<Claim> roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
                    id.AddClaims(roles);
                }

                await HttpContext.SignInAsync(
                    IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    new ClaimsPrincipal(id),
                    props);
                return Redirect(props.RedirectUri);
            }

            // trigger windows auth
            // since windows auth don't support the redirect uri,
            // this URL is re-triggered when we call challenge
            return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
        }

        private async Task<(User user, string provider, string providerUserId, IEnumerable<Claim> claims)>
            FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            ClaimsPrincipal externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            Claim userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject)
                                ?? externalUser.FindFirst(ClaimTypes.NameIdentifier) ?? throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            List<Claim> claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            string provider = result.Properties.Items["scheme"];
            string providerUserId = userIdClaim.Value;

            // find external user
            User user = await _userRepository.FindByExternalProviderAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        private async Task<User> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            User user = await _userRepository.AutoProvisionUserAsync(provider, providerUserId, claims.ToList());
            return user;
        }

        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult,
            List<Claim> localClaims,
            AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            Claim sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            string id_token = externalResult.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] {new AuthenticationToken {Name = "id_token", Value = id_token}});
            }
        }

        private void ProcessLoginCallbackForWsFed(AuthenticateResult externalResult,
            List<Claim> localClaims,
            AuthenticationProperties localSignInProps) { }

        private void ProcessLoginCallbackForSaml2p(AuthenticateResult externalResult,
            List<Claim> localClaims,
            AuthenticationProperties localSignInProps) { }

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

        #region External Login

        /// <summary>
        ///     initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        [Route("~/external-login")]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            {
                // windows authentication needs special handling
                return await ProcessWindowsLoginAsync(returnUrl);
            }

            // start challenge and roundtrip the return URL and 
            var props = new AuthenticationProperties {
                RedirectUri = Url.Action("ExternalLoginCallback"), Items = {{"returnUrl", returnUrl}, {"scheme", provider}}
            };
            return Challenge(props, provider);
        }

        /// <summary>
        ///     Post processing of external authentication
        /// </summary>
        [HttpGet]
        [Route("~/external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            // read external identity from the temporary cookie
            AuthenticateResult result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            // lookup our user and external provider info
            (User user, string provider, string providerUserId, IEnumerable<Claim> claims) =
                await FindUserFromExternalProviderAsync(result);
            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = await AutoProvisionUserAsync(provider, providerUserId, claims);
            }

            // this allows us to collect any additonal claims or properties
            // for the specific prtotocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.UserId.ToString(), user.Fullname));
            await HttpContext.SignInAsync(
                user.UserId.ToString(),
                user.EmailAddress,
                provider,
                localSignInProps,
                additionalLocalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // validate return URL and redirect back to authorization endpoint or a local page
            string returnUrl = result.Properties.Items["returnUrl"];
            if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/");
        }

        #endregion

    }

}
