using System;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.ErrorHandling
{

    public abstract class CustomErrorPageException : Exception
    {
        public abstract string ViewName { get; }
        public abstract int StatusCode { get; }
    }

    public abstract class CustomRedirectException : Exception
    {
        public abstract string RedirectUrl { get; }
    }


    public class UserAccountRetiredException : CustomErrorPageException
    {
        public override string ViewName => "../Errors/UserAccountRetired";
        public override int StatusCode => 403;
    }

    public class EmailNotVerifiedException : CustomErrorPageException
    {
        public override string ViewName => "../Errors/EmailNotVerified";
        public override int StatusCode => 403;
        public string EmailAddress { get; set; }
        public DateTime? EmailVerifySendDate { get; set; }
    }

    public class UserNotRegisteredToReportForOrganisationException : CustomErrorPageException
    {
        public override string ViewName => "../Errors/UserNotRegisteredToReportForOrganisation";    
        public override int StatusCode => 403;
    }
    
    public class UserRecentlySentPasswordResetEmailWithoutChangingPasswordException : CustomErrorPageException
    {
        public override string ViewName => "../Errors/UserRecentlySentPasswordResetEmailWithoutChangingPassword";    
        public override int StatusCode => 403;
    }

    public class PasswordResetCodeExpiredException : CustomErrorPageException
    {

        public override string ViewName => "../Errors/PasswordResetCodeExpired";

        public override int StatusCode => 403;

    }

    public class AdminCannotTakeActionIfImpersonatingUserException : CustomErrorPageException
    {
        public override string ViewName => "../Errors/AdminCannotTakeActionIfUserIsBeingImpersonated";
        public override int StatusCode => 403;
    }

    public class PageNotFoundException : CustomErrorPageException
    {
        public override string ViewName => "../Errors/404";
        public override int StatusCode => 404;
    }

    public class RedirectToPrivacyPolicyException : CustomRedirectException
    {
        public override string RedirectUrl { get; }

        public RedirectToPrivacyPolicyException(IUrlHelper url)
        {
            RedirectUrl = url.Action("PrivacyPolicyGet", "PrivacyPolicy");
        }
    }
    
    public class PinExpiredException : CustomErrorPageException
    {
        public override string ViewName => "../Errors/PinExpired";
        public override int StatusCode => 400;
    }
}
