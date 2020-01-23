using System;
using Autofac;
using GenderPayGap.Core.Abstractions;
using GenderPayGap.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GenderPayGap.Core.Classes
{
    public static class EmailTemplateExtensions
    {

        /// <summary>
        ///     Maps a template model to a corresponding entry in the appsetting config
        /// </summary>
        /// <example>
        ///     // appsettings.json example
        ///     {
        ///     "Email": {
        ///     "Templates": {
        ///     "MyTemplateName": "c97cb8d6-4b1b-468f-812e-af77e1f2422c"
        ///     }
        ///     }
        ///     }
        ///     // Email template example
        ///     public class MyTemplateName : ATemplate
        ///     {
        ///     // Merge fields used with Gov Notify or Smtp templates...
        ///     public string Field1 {get; set;}
        ///     public string Field2 {get; set;}
        ///     }
        ///     // usage example
        ///     host.RegisterEmailTemplate<MyTemplateName>("Email:Templates");
        /// </example>
        public static void RegisterEmailTemplate<TTemplate>(this IHost host, string templatesConfigPath) where TTemplate : AEmailTemplate
        {
            // get the autofac container
            IServiceProvider services = host.Services;
            var provider = services.GetService<IContainer>();

            // resolve config and resolve template repository
            var config = provider.Resolve<IConfiguration>();
            var repo = provider.Resolve<IEmailTemplateRepository>();

            // get the template id using the type name
            string templateConfigKey = typeof(TTemplate).Name;
            var templateId = config.GetValue<string>($"{templatesConfigPath}:{templateConfigKey}");

            // add this template to the repository
            repo.Add<TTemplate>(templateId, $"{templateConfigKey}.html");
        }

    }

}
