﻿using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin + "," + LoginRoles.GpgAdminReadOnly)]
    [Route("admin")]
    public class AdminViewUserController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminViewUserController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("user/{id}")]
        public IActionResult ViewUser(long id)
        {
            var user = dataRepository.Get<User>(id);

            return View("../Admin/ViewUser", user);
        }

    }
}
