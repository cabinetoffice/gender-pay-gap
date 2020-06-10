using System;
using System.Diagnostics;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;

namespace GenderPayGap.Extensions.AspNetCore
{
    public static partial class Extensions
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
                string redisConnectionString = Config.GetConnectionString("RedisCache");
                if (string.IsNullOrWhiteSpace(redisConnectionString))
                {
                    throw new ArgumentNullException("RedisCache", "Cannot find 'RedisCache' ConnectionString");
                }

                services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });

                //Use blob storage to persist data protection keys equivalent to old MachineKeys
                string storageConnectionString = Config.GetConnectionString("AzureStorage");
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
