using System;
using GenderPayGap.Core.Abstractions;
using GenderPayGap.Core.Models;

namespace GenderPayGap.Core.Interfaces
{
    public interface IEmailTemplateRepository
    {

        void Add<TTemplate>(string templateId, string filePath) where TTemplate : AEmailTemplate;

        EmailTemplateInfo GetByTemplateId(string templateId);

        EmailTemplateInfo GetByType(Type templateType);

    }

}
