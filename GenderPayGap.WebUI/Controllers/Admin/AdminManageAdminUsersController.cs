using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Repositories;
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
        private readonly UserRepository userRepository;
        private readonly AuditLogger auditLogger;

        public AdminManageAdminUsersController(
            IDataRepository dataRepository,
            UserRepository userRepository,
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
                .Where(user => user.UserRole == UserRole.Admin || user.UserRole == UserRole.AdminReadOnly)
                .AsEnumerable()
                .OrderBy(user => user.Fullname)
                .ToList();

            return View("ViewAdminUsers", adminUsers);
        }

        [HttpGet("change-type-of-admin-user/{userId}")]
        public IActionResult ChangeTypeOfAdminUserGet(long userId)
        {
            User adminUser = dataRepository.Get<User>(userId);

            if (adminUser == null || !adminUser.IsFullOrReadOnlyAdministrator())
            {
                throw new PageNotFoundException();
            }
            
            var viewModel = new AdminChangeTypeOfAdminUserViewModel
            {
                User = adminUser,
                ReadOnly = (adminUser.UserRole == UserRole.AdminReadOnly)
            };

            return View("ChangeTypeOfAdminUser", viewModel);
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost("change-type-of-admin-user/{userId}")]
        public IActionResult ChangeTypeOfAdminUserPost(long userId, AdminChangeTypeOfAdminUserViewModel viewModel)
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
                viewModel.ReadOnly = (adminUser.UserRole == UserRole.AdminReadOnly);
                return View("ChangeTypeOfAdminUser", viewModel);
            }

            // Update the status
            adminUser.UserRole = viewModel.ReadOnly ? UserRole.AdminReadOnly : UserRole.Admin;

            dataRepository.SaveChanges();

            return RedirectToAction("ViewAdminUsers", "AdminManageAdminUsers");
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

            return RedirectToAction("ConfirmNewAdminUserGet", "AdminManageAdminUsers", new {userId = user.UserId, readOnly = viewModel.ReadOnly});
        }
        
        [HttpGet("add-new-admin-user/{userId}")]
        public IActionResult ConfirmNewAdminUserGet(long userId, [FromQuery] bool readOnly)
        {
            User user = dataRepository.Get<User>(userId);

            if (user == null || user.IsFullOrReadOnlyAdministrator())
            {
                throw new PageNotFoundException();
            }

            var viewModel = new AdminConfirmNewAdminUserViewModel {User = user, ReadOnly = readOnly};

            return View("ConfirmNewAdminUser", viewModel);
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost("add-new-admin-user/{userId}")]
        public IActionResult ConfirmNewAdminUserPost(long userId, [FromQuery] bool readOnly)
        {
            User user = dataRepository.Get<User>(userId);

            if (!ModelState.IsValid)
            {
                var viewModel = new AdminConfirmNewAdminUserViewModel {User = user, ReadOnly = readOnly};
                return View("ConfirmNewAdminUser", viewModel);
            }

            if (user == null || user.IsFullOrReadOnlyAdministrator())
            {
                throw new PageNotFoundException();
            }

            user.UserRole = readOnly ? UserRole.AdminReadOnly : UserRole.Admin;

            dataRepository.SaveChanges();
            
            auditLogger.AuditChangeToUser(
                AuditedAction.AdminAddAdminUser,
                user,
                new
                {
                    UserIdToMakeAdmin = user.UserId,
                    Role = user.UserRole
                },
                User);

            return RedirectToAction("ViewAdminUsers", "AdminManageAdminUsers");
        }
        
    }
}
