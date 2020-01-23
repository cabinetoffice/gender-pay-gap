using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using GenderPayGap.Core.Abstractions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using Newtonsoft.Json;

namespace GenderPayGap.Core.Classes
{

    public class EmailTemplateRepository : IEmailTemplateRepository
    {

        public EmailTemplateRepository(string templateFolderPath)
        {
            if (string.IsNullOrWhiteSpace(templateFolderPath))
            {
                throw new ArgumentException("The email template folder path must be set", nameof(templateFolderPath));
            }

            TemplateFolderPath = templateFolderPath;
            EmailTemplateStore = new HashSet<EmailTemplateInfo>();
        }

        public string TemplateFolderPath { get; }

        private HashSet<EmailTemplateInfo> EmailTemplateStore { get; }

        public void Add<TTemplate>(string templateId, string filePath) where TTemplate : AEmailTemplate
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                throw new ArgumentException("You must provide an email template id", nameof(templateId));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("You must provide an email template file path", nameof(filePath));
            }

            // ensure file exists
            string fileContent = File.ReadAllText($"{TemplateFolderPath}\\{filePath}");

            // parse html file
            var parser = new HtmlParser();
            using (IHtmlDocument document = parser.ParseDocument(fileContent))
            {
                // extract the first comment and deserialze the json (similar to how 'FrontMatter' works)
                IComment templateMetaData = document.Descendents<IComment>().FirstOrDefault();
                if (templateMetaData == null)
                {
                    throw new NullReferenceException(nameof(templateMetaData));
                }

                var templateInfo = JsonConvert.DeserializeObject<EmailTemplateInfo>(templateMetaData.NodeValue);
                if (string.IsNullOrWhiteSpace(templateInfo.EmailSubject))
                {
                    throw new NullReferenceException(nameof(templateInfo.EmailSubject));
                }

                // set template info
                templateInfo.TemplateId = templateId;
                templateInfo.TemplateType = typeof(TTemplate);
                templateInfo.FilePath = filePath;

                // ensure no duplicate
                EmailTemplateStore.Add(templateInfo);
            }
        }

        public EmailTemplateInfo GetByTemplateId(string templateId)
        {
            return EmailTemplateStore.SingleOrDefault(emailTemplate => emailTemplate.TemplateId == templateId);
        }

        public EmailTemplateInfo GetByType(Type templateType)
        {
            return EmailTemplateStore.SingleOrDefault(emailTemplate => emailTemplate.TemplateType == templateType);
        }

    }

}
