using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GenderPayGap.WebUI.Helpers
{
    internal static class DataProtectionKeysHelper
    {

        internal static void AddDataProtectionKeyStorage(IServiceCollection services)
        {
            services.AddDataProtection()
                .AddKeyManagementOptions(options => options.XmlRepository = new CustomDataProtectionKeyRepository());
        }

    }

    internal class CustomDataProtectionKeyRepository : IXmlRepository
    {

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            IDataRepository dataRepository = Global.ContainerIoC.Resolve<IDataRepository>();
            return dataRepository.GetAll<DataProtectionKey>().Select(dpk => XElement.Parse(dpk.Xml)).ToList();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            var dataProtectionKey = new DataProtectionKey
            {
                FriendlyName = friendlyName,
                Xml = element.ToString()
            };

            IDataRepository dataRepository = Global.ContainerIoC.Resolve<IDataRepository>();
            dataRepository.Insert(dataProtectionKey);
            dataRepository.SaveChangesAsync().Wait();
        }

    }
}
