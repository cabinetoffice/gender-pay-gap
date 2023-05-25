using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
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

    }
}
