using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public partial class RegisterController : BaseController
    {

        #region EmailConfirmed

        [Authorize]
        [HttpGet("email-confirmed")]
        public IActionResult EmailConfirmed()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //If its an administrator go to admin home
            if (currentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin");
            }

            return View("EmailConfirmed");
        }

        #endregion
        
    }
}
