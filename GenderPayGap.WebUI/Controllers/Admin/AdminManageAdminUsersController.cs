using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
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
        private readonly IUserRepository userRepository;
        private readonly AuditLogger auditLogger;

        public AdminManageAdminUsersController(
            IDataRepository dataRepository,
            IUserRepository userRepository,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.userRepository = userRepository;
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

            if (adminUser == null || !adminUser.IsFullOrReadOnlyAdministrator())
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

            if (adminUser == null || !adminUser.IsFullOrReadOnlyAdministrator())
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
        
        [HttpGet("add-new-admin-user")]
        public IActionResult AddNewAdminUserGet()
        {
            var viewModel = new AdminAddNewAdminUserViewModel();

            return View("AddNewAdminUser", viewModel);
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost("add-new-admin-user")]
        public IActionResult AddNewAdminUserPost(AdminAddNewAdminUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("AddNewAdminUser", viewModel);
            }

            User user = userRepository.FindByEmail(viewModel.EmailAddress);

            if (user == null)
            {
                ModelState.AddModelError(nameof(viewModel.EmailAddress), "Could not find a user with this email address");
            }
            else if (user.IsFullOrReadOnlyAdministrator())
            {
                ModelState.AddModelError(nameof(viewModel.EmailAddress), "This user is already an admin user");
            }
            
            if (!ModelState.IsValid)
            {
                return View("AddNewAdminUser", viewModel);
            }

            return RedirectToAction("ConfirmNewAdminUserGet", "AdminManageAdminUsers", new {userId = user.UserId});
        }
        
        [HttpGet("add-new-admin-user/{userId}")]
        public IActionResult ConfirmNewAdminUserGet(long userId)
        {
            User user = dataRepository.Get<User>(userId);

            if (user == null || user.IsAdministrator())
            {
                throw new PageNotFoundException();
            }

            return View("ConfirmNewAdminUser", user);
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost("add-new-admin-user/{userId}")]
        public IActionResult ConfirmNewAdminUserPost(long userId)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfirmNewAdminUser");
            }

            User user = dataRepository.Get<User>(userId);

            if (user == null || user.IsAdministrator())
            {
                throw new PageNotFoundException();
            }

            user.UserRole = UserRole.Admin;

            dataRepository.SaveChanges();
            
            auditLogger.AuditChangeToUser(
                AuditedAction.AdminAddAdminUser,
                user,
                new {UserIdToMakeAdmin = user.UserId},
                User);

            return RedirectToAction("ViewAdminUsers", "AdminManageAdminUsers");
        }
        
    }
}
