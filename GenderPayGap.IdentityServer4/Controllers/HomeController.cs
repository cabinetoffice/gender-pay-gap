// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using GenderPayGap.Extensions;
using GenderPayGap.IdentityServer4.Classes;
using GenderPayGap.IdentityServer4.Models.Home;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.IdentityServer4.Controllers
{
    [Route("Home")]
    public class HomeController : BaseController
    {

        private readonly IIdentityServerInteractionService _interaction;

        public HomeController(
            IIdentityServerInteractionService interaction,
            IEventService events,
            IDistributedCache cache,
            ILogger<HomeController> logger) : base(events, cache, logger)
        {
            _interaction = interaction;
        }

        [Route("~/ping")]
        public IActionResult Ping()
        {
            return new OkResult(); // OK = 200
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("~/login/login")]
        public IActionResult RedirectOldLogin()
        {
            return Redirect(Startup.SiteAuthority);
        }

        /// <summary>
        ///     Shows the error page
        /// </summary>
        [Route("~/error/{errorId?}")]
        public async Task<IActionResult> Error(string errorId = null)
        {
            int errorCode = errorId.ToInt32();

            if (errorCode == 0)
            {
                if (Response.StatusCode.Between(400, 599))
                {
                    errorCode = Response.StatusCode;
                }
                else
                {
                    errorCode = 500;
                }
            }

            var model = new ErrorViewModel();

            // retrieve error details from identityserver
            ErrorMessage message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                model.Title = message.Error;
                model.Description = message.ErrorDescription;
                _Logger.LogError($"{message.Error}: {message.ErrorDescription}");
            }
            else
            {
                //Get the exception which caused this error
                var errorData = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
                if (errorData == null)
                {
                    //Log non-exception events
                    var statusCodeData = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                    if (statusCodeData != null)
                    {
                        _Logger.LogError($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    }
                    else
                    {
                        _Logger.LogError($"HttpStatusCode {errorCode}, Path: Unknown");
                    }
                }
            }

            Response.StatusCode = errorCode;
            return View("Error", model);
        }

    }
}
