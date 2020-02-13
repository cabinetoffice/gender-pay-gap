using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Abstractions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public interface IMessenger
    {

        Task<bool> SendGeoMessageAsync(string subject, string message, bool test = false);

        Task<bool> SendMessageAsync(string subject, string recipients, string message, bool test = false);

        Task SendEmailTemplateAsync<TTemplate>(TTemplate parameters) where TTemplate : AEmailTemplate;

    }

    public class Messenger : IMessenger
    {

        private readonly ILogger<Messenger> log;

        public Messenger(ILogger<Messenger> logger, GpgEmailProvider gpgEmailProvider)
        {
            log = logger;
            GpgEmailProvider = gpgEmailProvider;
        }

        private string GEODistributionList => Config.GetAppSetting("GEODistributionList");

        private string SmtpSenderName => Config.GetAppSetting("Email:Providers:Smtp:SenderName");
        private string SmtpSenderEmail => Config.GetAppSetting("Email:Providers:Smtp:SenderEmail");
        private string SmtpReplyEmail => Config.GetAppSetting("Email:Providers:Smtp:ReplyEmail");

        private string SmtpServer2 =>
            Config.GetAppSetting("Email:Providers:Smtp:Server2") ?? Config.GetAppSetting("Email:Providers:Smtp:Server");

        private int SmtpPort2 =>
            (Config.GetAppSetting("Email:Providers:Smtp:Port2") ?? Config.GetAppSetting("Email:Providers:Smtp:Port")).ToInt32(25);

        private string SmtpUsername2 =>
            Config.GetAppSetting("Email:Providers:Smtp:Username2") ?? Config.GetAppSetting("Email:Providers:Smtp:Username");

        private string SmtpPassword2 =>
            Config.GetAppSetting("Email:Providers:Smtp:Password2") ?? Config.GetAppSetting("Email:Providers:Smtp:Password");

        public GpgEmailProvider GpgEmailProvider { get; }

        #region Emails

        /// <summary>
        ///     Send a message to GEO distribution list
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public async Task<bool> SendGeoMessageAsync(string subject, string message, bool test = false)
        {
            List<string> emailAddresses = GEODistributionList.SplitI(";").ToList();
            emailAddresses = emailAddresses.RemoveI("sender", "recipient");
            if (emailAddresses.Count == 0)
            {
                throw new ArgumentNullException(nameof(GEODistributionList));
            }

            if (!emailAddresses.ContainsAllEmails())
            {
                throw new ArgumentException($"{GEODistributionList} contains an invalid email address", nameof(GEODistributionList));
            }

            var successCount = 0;
            foreach (string emailAddress in emailAddresses)
            {
                try
                {
                    await Email.QuickSendAsync(
                        Config.IsProduction() ? subject : $"[{Config.EnvironmentName}] {subject}",
                        SmtpSenderEmail,
                        SmtpSenderName,
                        SmtpReplyEmail,
                        emailAddress,
                        message,
                        SmtpServer2,
                        SmtpUsername2,
                        SmtpPassword2,
                        SmtpPort2,
                        test: test);
                    await Global.EmailSendLog.WriteAsync(
                        new EmailSendLogModel {
                            Message = "Email successfully sent via SMTP",
                            Subject = subject,
                            Recipients = emailAddress,
                            Server = $"{SmtpServer2}:{SmtpPort2}",
                            Username = SmtpUsername2,
                            Details = message
                        });
                    successCount++;
                }
                catch (Exception ex1)
                {
                    log.LogError(ex1, $"Cant send message '{subject}' '{message}' directly to {emailAddress}:");
                }
            }

            return successCount == emailAddresses.Count;
        }

        public async Task<bool> SendMessageAsync(string subject, string recipients, string message, bool test = false)
        {
            List<string> emailAddresses = recipients.SplitI(";").ToList();
            emailAddresses = emailAddresses.RemoveI("sender", "recipient");
            if (emailAddresses.Count == 0)
            {
                throw new ArgumentNullException(nameof(recipients));
            }

            if (!emailAddresses.ContainsAllEmails())
            {
                throw new ArgumentException($"{recipients} contains an invalid email address", nameof(recipients));
            }

            var successCount = 0;
            foreach (string emailAddress in emailAddresses)
            {
                try
                {
                    await Email.QuickSendAsync(
                        subject,
                        SmtpSenderEmail,
                        SmtpSenderName,
                        SmtpReplyEmail,
                        emailAddress,
                        message,
                        SmtpServer2,
                        SmtpUsername2,
                        SmtpPassword2,
                        SmtpPort2,
                        test: test);
                    await Global.EmailSendLog.WriteAsync(
                        new EmailSendLogModel {
                            Message = "Email successfully sent via SMTP",
                            Subject = subject,
                            Recipients = emailAddress,
                            Server = $"{SmtpServer2}:{SmtpPort2}",
                            Username = SmtpUsername2,
                            Details = message
                        });
                    successCount++;
                }
                catch (Exception ex1)
                {
                    log.LogError(ex1, $"Cant send message '{subject}' '{message}' directly to {emailAddress}:");
                }
            }

            return successCount == emailAddresses.Count;
        }

        public async Task SendEmailTemplateAsync<TTemplate>(TTemplate parameters) where TTemplate : AEmailTemplate
        {
            await GpgEmailProvider.SendEmailTemplateAsync(parameters);
        }

        #endregion

    }
}
