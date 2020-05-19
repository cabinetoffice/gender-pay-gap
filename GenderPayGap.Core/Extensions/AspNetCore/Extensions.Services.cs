using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Storage;

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

        /// <summary>
        ///     Configure the Owin authentication for Identity Server
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddIdentityServerClient(this IServiceCollection services,
            string authority,
            string signedOutRedirectUri,
            string clientId,
            string clientSecret = null,
            HttpMessageHandler backchannelHttpHandler = null)
        {
            //Turn off the JWT claim type mapping to allow well-known claims (e.g. ‘sub’ and ‘idp’) to flow through unmolested
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(
                    options => {
                        options.DefaultScheme = "Cookies";
                        options.DefaultChallengeScheme = "oidc";
                    })
                .AddOpenIdConnect(
                    "oidc",
                    options => {
                        options.SignInScheme = "Cookies";
                        options.Authority = authority;
                        options.RequireHttpsMetadata = true;
                        options.ClientId = clientId;
                        if (!string.IsNullOrWhiteSpace(clientSecret))
                        {
                            options.ClientSecret = clientSecret.GetSHA256Checksum();
                        }

                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("roles");
                        options.SaveTokens = true;
                        options.SignedOutRedirectUri = signedOutRedirectUri;
                        options.Events.OnRedirectToIdentityProvider = context => {
                            Uri referrer = context.HttpContext?.GetUri();
                            if (referrer != null)
                            {
                                context.ProtocolMessage.SetParameter("Referrer", referrer.PathAndQuery);
                            }

                            return Task.CompletedTask;
                        };
                        options.BackchannelHttpHandler = backchannelHttpHandler;
                    })
                .AddCookie(
                    "Cookies",
                    options => {
                        options.AccessDeniedPath = "/Error/403"; //Show forbidden error page
                    });
            return services;
        }

    }


}
