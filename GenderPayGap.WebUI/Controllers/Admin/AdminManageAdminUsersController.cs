using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin/manage-admin-users")]
    public class AdminManageAdminUsersController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminManageAdminUsersController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }
        
        [HttpGet("")]
        public IActionResult ViewAdminUsers()
        {
            List<User> adminUsers = dataRepository.GetAll<User>()
                .Where(user => user.UserRole == UserRole.Admin)
                .AsEnumerable()
                .OrderBy(user => user.Fullname)
                .ToList();

            return View("ViewAdminUsers", adminUsers);
        }

    }
}
