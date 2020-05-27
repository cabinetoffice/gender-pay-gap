using System;
using System.Diagnostics;
using System.Net.Mail;

namespace GenderPayGap.Extensions
{
    public static class Email
    {

        [DebuggerStepThrough]
        public static bool IsEmailAddress(this string inputEmail)
        {
            if (string.IsNullOrWhiteSpace(inputEmail))
            {
                return false;
            }

            try
            {
                var email = new MailAddress(inputEmail);
                if (string.IsNullOrWhiteSpace(email.Address))
                {
                    return false;
                }
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

    }
}
