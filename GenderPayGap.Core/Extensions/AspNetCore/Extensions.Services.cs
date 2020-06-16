using System;
using System.Diagnostics;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using StackExchange.Redis;

namespace GenderPayGap.Core.Extensions.AspNetCore
{
    public static class Extensions
    {

        public static IServiceCollection AddRedisCache(this IServiceCollection services, string applicationDiscriminator = null)
        {
            //Add distributed cache service backed by Redis cache
            if (Debugger.IsAttached || Config.IsEnvironment("Local"))
            {
                //Use a memory cache
                services.AddDistributedMemoryCache();
                services.AddDataProtection(
                    options => {
                        if (!string.IsNullOrWhiteSpace(applicationDiscriminator))
                        {
                            options.ApplicationDiscriminator = applicationDiscriminator;
                        }
                    });
            }
            else
            {
                if (Global.VcapServices != null && Global.VcapServices.Redis != null)
                {
                    VcapRedis redisConfiguration = Global.VcapServices.Redis.First(b => b.Name.EndsWith("-cache"));

                    services.AddStackExchangeRedisCache(
                        options =>
                        {
                            options.ConfigurationOptions = new ConfigurationOptions
                            {
                                EndPoints = {{redisConfiguration.Credentials.Host, redisConfiguration.Credentials.Port}},
                                Password = redisConfiguration.Credentials.Password,
                                Ssl = redisConfiguration.Credentials.TlsEnabled,
                                AbortOnConnectFail = false
                            };
                        });
                }
                else
                {
                    throw new ArgumentNullException("VCAP_SERVICES", "Cannot find 'VCAP_SERVICES' config setting");
                }

                //Use blob storage to persist data protection keys equivalent to old MachineKeys
                string storageConnectionString = Global.AzureStorageConnectionString;
                if (string.IsNullOrWhiteSpace(storageConnectionString))
                {
                    throw new ArgumentNullException("AzureStorage", "Cannot find 'AzureStorage' ConnectionString");
                }

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                //var redis = ConnectionMultiplexer.Connect(redisConnectionString);
                services.AddDataProtection(
                        options => {
                            if (!string.IsNullOrWhiteSpace(applicationDiscriminator))
                            {
                                options.ApplicationDiscriminator = applicationDiscriminator;
                            }
                        })
                    .PersistKeysToAzureBlobStorage(storageAccount, "/data-protection/keys.xml");
                //.PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
                /* 
                 * May need to add .SetApplicationName("shared app name") to force IDSrv4 and WebUI to use same keys
                 * May need to add .DisableAutomaticKeyGeneration(); on IDSrv4 and WebUI for when key expires after 90 days to prevent one app from resetting keys
                 */
            }

            return services;
        }

    }
}
