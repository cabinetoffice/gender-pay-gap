using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin/manage-admin-users")]
    public class AdminManageAdminUsersController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;

        public AdminManageAdminUsersController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
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

        [HttpGet("retire-user/{userId}")]
        public IActionResult RetireAdminUserGet(long userId)
        {
            User adminUser = dataRepository.Get<User>(userId);

            if (adminUser == null || !adminUser.IsAdministrator())
            {
                throw new PageNotFoundException();
            }
            
            var viewModel = new AdminRetireAdminUserViewModel();
            viewModel.User = adminUser;

            return View("RetireAdminUser", viewModel);
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost("retire-user/{userId}")]
        public IActionResult RetireAdminUserPost(long userId, AdminRetireAdminUserViewModel viewModel)
        {
            User adminUser = dataRepository.Get<User>(userId);
            User userMakingTheChange = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            if (adminUser == null || !adminUser.IsAdministrator())
            {
                throw new PageNotFoundException();
            }
            
            if (!ModelState.IsValid)
            {
                viewModel.User = adminUser;
                return View("RetireAdminUser", viewModel);
            }

            // Update the status
            adminUser.SetStatus(
                UserStatuses.Retired,
                userMakingTheChange,
                viewModel.Reason);

            dataRepository.SaveChanges();

            return RedirectToAction("ViewAdminUsers", "AdminManageAdminUsers");
        }
        
    }
}
