using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

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
                if (email == null || string.IsNullOrWhiteSpace(email.Address))
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

        public static string GetEmailAddress(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return "";
            }

            try
            {
                return new MailAddress(email).Address.ToLower();
            }
            catch (FormatException)
            {
                return "";
            }
        }

        public static async Task QuickSendAsync(string subject,
            string senderEmailAddress,
            string senderName,
            string replyEmailAddress,
            string recipients,
            string html,
            string smtpServer,
            string smtpUsername,
            string smtpPassword,
            int smtpPort = 25,
            byte[] attachment = null,
            string attachmentFilename = "attachment.dat",
            bool test = false)
        {
            if (string.IsNullOrWhiteSpace(senderEmailAddress))
            {
                throw new ArgumentNullException(nameof(senderEmailAddress), "Missing or empty senderEmailAddress");
            }

            if (string.IsNullOrWhiteSpace(recipients))
            {
                throw new ArgumentNullException(nameof(recipients), "Missing or empty recipients");
            }

            if (string.IsNullOrWhiteSpace(html))
            {
                throw new ArgumentNullException(nameof(html), "Missing or empty html");
            }

            if (string.IsNullOrWhiteSpace(senderEmailAddress))
            {
                throw new ArgumentNullException(nameof(senderEmailAddress), "Missing or empty senderEmailAddress");
            }

            var mySmtpClient = new SmtpClient(smtpServer);

            //Throw an error of no SMTP server, username or password is set
            if (!string.IsNullOrWhiteSpace(smtpServer))
            {
                if (string.IsNullOrWhiteSpace(smtpUsername))
                {
                    throw new ArgumentNullException(nameof(smtpUsername), "Missing or empty SmtpUsername");
                }

                if (string.IsNullOrWhiteSpace(smtpPassword))
                {
                    throw new ArgumentNullException(nameof(smtpPassword), "Missing or empty SmtpPassword");
                }

                mySmtpClient.Port = smtpPort;
                mySmtpClient.EnableSsl = true;
                mySmtpClient.UseDefaultCredentials = false;
                mySmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            }
            else
            {
                throw new ArgumentNullException(nameof(smtpServer), "Missing or empty SmtpServer and SmtpDropPath");
            }

            // set smtp-client with basicAuthentication
            var basicAuthenticationInfo = new NetworkCredential(smtpUsername, smtpPassword);
            mySmtpClient.Credentials = basicAuthenticationInfo;

            var myMail = new MailMessage {
                From = new MailAddress(senderEmailAddress, senderName),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = html,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            //Add
            if (!string.IsNullOrWhiteSpace(replyEmailAddress))
            {
                myMail.ReplyToList.Add(new MailAddress(replyEmailAddress, senderName));
            }

            // set body-message and encoding
            // text or html

            // add mailaddresses
            foreach (string recipient in recipients.SplitI(";"))
            {
                myMail.To.Add(new MailAddress(recipient));
            }

            //Add the attachment
            if (!test)
            {
                if (attachment == null)
                {
                    await mySmtpClient.SendMailAsync(myMail);
                }
                else
                {
                    using (var stream = new MemoryStream(attachment))
                    {
                        myMail.Attachments.Add(new Attachment(stream, attachmentFilename));
                        await mySmtpClient.SendMailAsync(myMail);
                    }
                }
            }
        }

    }
}
