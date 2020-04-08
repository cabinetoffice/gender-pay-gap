using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using GenderPayGap.Core.Abstractions;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenderPayGap.Core.Classes
{

    public class SmtpEmailProvider : AEmailProvider
    {

        public SmtpEmailProvider(IOptions<SmtpEmailOptions> smtpEmailOptions,
            ILogger<SmtpEmailProvider> logger)
            : base(logger)
        {
            Options = smtpEmailOptions ?? throw new ArgumentNullException(nameof(smtpEmailOptions));
            //TODO ensure smtp config is present (when enabled)
        }

        public IOptions<SmtpEmailOptions> Options { get; }

        public override bool Enabled => Options.Value.Enabled != false;

        public override async Task<SendEmailResult> SendEmailAsync<TModel>(string emailAddress, string templateId, TModel model, bool test)
        {
            // convert the model's public properties to a dictionary
            Dictionary<string, object> mergeParameters = model.GetPropertiesDictionary();

            // prefix subject with environment name
            mergeParameters["Environment"] = Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] ";

            // get template
            EmailTemplateInfo emailTemplateInfo = null; //EmailTemplateRepo.GetByTemplateId(templateId);
            string htmlContent = File.ReadAllText(emailTemplateInfo.FilePath);

            // parse html
            var parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(htmlContent);

            // remove the meta data comments from the document
            IComment templateMetaData = document.Descendents<IComment>().FirstOrDefault();
            if (templateMetaData == null)
            {
                new NullReferenceException(nameof(templateMetaData));
            }

            templateMetaData.Remove();

            string messageSubject = emailTemplateInfo.EmailSubject;
            string messageHtml = document.ToHtml();
            string messageText = document.Text();

            // merge the template parameters
            foreach ((string name, object value) in mergeParameters)
            {
                messageSubject = messageSubject.Replace($"(({name}))", value.ToString());
                messageHtml = messageHtml.Replace($"(({name}))", value.ToString());
            }

            string smtpServer = Options.Value.Server2 ?? Options.Value.Server;
            var smtpServerPort = (Options.Value.Port2 ?? Options.Value.Port).ToInt32(25);
            string smtpUsername = Options.Value.Username2 ?? Options.Value.Username;

            await Email.QuickSendAsync(
                messageSubject,
                Options.Value.SenderEmail,
                Options.Value.SenderName,
                Options.Value.ReplyEmail,
                emailAddress,
                messageHtml,
                smtpServer,
                smtpUsername,
                Options.Value.Password2 ?? Options.Value.Password,
                smtpServerPort,
                test: test);

            return new SendEmailResult
            {
                Status = "sent",
                Server = $"{smtpServer}:{smtpServerPort}",
                ServerUsername = smtpUsername,
                EmailAddress = emailAddress,
                EmailSubject = messageSubject,
                EmailMessagePlainText = messageText
            };
        }

    }

}
