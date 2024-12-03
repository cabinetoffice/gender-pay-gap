using System.Xml.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;

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
            using (var serviceScope = Program.DependencyInjectionServiceProvider.CreateScope())
            {
                IDataRepository dataRepository = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();

                List<DataProtectionKey> dataProtectionKeys = dataRepository.GetAll<DataProtectionKey>()
                    .ToList();

                List<XElement> keysAdXmlElements = dataProtectionKeys
                    .Select(dpk =>
                    {
                        string encryptedKey = dpk.Xml;
                        string unencryptedString = Encryption.DecryptData(encryptedKey);
                        XElement xmlElement = XElement.Parse(unencryptedString);
                        return xmlElement;
                    })
                    .ToList();

                return keysAdXmlElements;
            }
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            string serialisedKey = element.ToString();
            string encryptedKey = Encryption.EncryptData(serialisedKey);

            var dataProtectionKey = new DataProtectionKey
            {
                FriendlyName = friendlyName,
                Xml = encryptedKey
            };

            using (var serviceScope = Program.DependencyInjectionServiceProvider.CreateScope())
            {
                IDataRepository dataRepository = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
                dataRepository.Insert(dataProtectionKey);
                dataRepository.SaveChanges();
            }
        }

    }
}
