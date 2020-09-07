using System;

namespace GenderPayGap.WebUI.ErrorHandling
{

    public abstract class CustomErrorPageException : Exception
    {
        public abstract string ViewName { get; }
        public abstract int StatusCode { get; }
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

    public class IncorrectPermissionsException : CustomErrorPageException
    {

        // TODO: Update this with a sensible view
        public override string ViewName => "";

        public override int StatusCode => 403;

    }

    public class PageNotFoundException : CustomErrorPageException
    {
        public override string ViewName => "../Errors/404";
        public override int StatusCode => 404;
    }

}
