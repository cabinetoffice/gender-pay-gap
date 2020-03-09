using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenderPayGap.WebJob
{

    public partial class Functions
    {

        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        public async Task PurgeUsers([TimerTrigger("40 3 * * *" /* 03:40 once per day */)] TimerInfo timer, ILogger log)
        {
            var runId = CreateRunId();
            var startTime = VirtualDateTime.Now;
            LogFunctionStart(runId,  nameof(PurgeUsers), startTime);
            try
            {
                DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeUnverifiedUserDays);
                List<User> users = await _DataRepository.GetAll<User>()
                    .Where(u => u.EmailVerifiedDate == null && (u.EmailVerifySendDate == null || u.EmailVerifySendDate.Value < deadline))
                    .ToListAsync();
                DateTime pinExpireyDate = VirtualDateTime.Now.AddDays(0 - Global.PinInPostExpiryDays);
                foreach (User user in users)
                {
                    //Ignore if they have verified PIN
                    if (user.UserOrganisations.Any(
                        uo => uo.PINConfirmedDate != null || uo.PINSentDate != null && uo.PINSentDate < pinExpireyDate))
                    {
                        continue;
                    }

                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeUsers),
                        ManualActions.Delete,
                        AppDomain.CurrentDomain.FriendlyName,
                        nameof(user.UserId),
                        user.UserId.ToString(),
                        null,
                        JsonConvert.SerializeObject(new {user.UserId, user.EmailAddress, user.JobTitle, user.Fullname}),
                        null);
                    _DataRepository.Delete(user);
                    await _DataRepository.SaveChangesAsync();
                    await Global.ManualChangeLog.WriteAsync(logItem);
                }

                LogFunctionEnd(runId, nameof(PurgeUsers), startTime);
            }
            catch (Exception ex)
            {
                LogFunctionError(runId, nameof(PurgeUsers), startTime, ex );

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", 
                    $"Failed webjob ({nameof(PurgeUsers)}):{ex.Message}:{ex.GetDetailsText()}");
                //Rethrow the error
                throw;
            }
        }

        //Remove any incomplete registrations
        public async Task PurgeRegistrations([TimerTrigger("50 3 * * *" /* 03:50 once per day */)] TimerInfo timer, ILogger log)
        {
            var runId = CreateRunId();
            var startTime = DateTime.Now;
            LogFunctionStart(runId,  nameof(PurgeRegistrations), startTime);
            try
            {
                DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeUnconfirmedPinDays);
                List<UserOrganisation> registrations = await _DataRepository.GetAll<UserOrganisation>()
                    .Where(u => u.PINConfirmedDate == null && u.PINSentDate != null && u.PINSentDate.Value < deadline)
                    .ToListAsync();
                foreach (UserOrganisation registration in registrations)
                {
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeRegistrations),
                        ManualActions.Delete,
                        AppDomain.CurrentDomain.FriendlyName,
                        $"{nameof(UserOrganisation.OrganisationId)}:{nameof(UserOrganisation.UserId)}",
                        $"{registration.OrganisationId}:{registration.UserId}",
                        null,
                        JsonConvert.SerializeObject(
                            new {
                                registration.UserId,
                                registration.User.EmailAddress,
                                registration.OrganisationId,
                                registration.Organisation.EmployerReference,
                                registration.Organisation.OrganisationName,
                                registration.Method,
                                registration.PINSentDate,
                                registration.PINConfirmedDate
                            }),
                        null);
                    _DataRepository.Delete(registration);
                    await _DataRepository.SaveChangesAsync();
                    await Global.ManualChangeLog.WriteAsync(logItem);
                }

                LogFunctionEnd(runId, nameof(PurgeRegistrations), startTime);
            }
            catch (Exception ex)
            {
                LogFunctionError(runId, nameof(PurgeRegistrations), startTime, ex );
                
                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", 
                    $"Failed webjob ({nameof(PurgeRegistrations)}):{ex.Message}:{ex.GetDetailsText()}");
                //Rethrow the error
                throw;
            }
        }

        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        public async Task PurgeOrganisations([TimerTrigger("20 4 * * *" /* 04:20 once per day */)]
            TimerInfo timer,
            ILogger log)
        {
            var runId = CreateRunId();
            var startTime = DateTime.Now;
            LogFunctionStart(runId,  nameof(PurgeOrganisations), startTime);
            try
            {
                DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeUnusedOrganisationDays);
                List<Organisation> orgs = await _DataRepository.GetAll<Organisation>()
                    .Where(
                        o => o.Created < deadline
                             && !o.Returns.Any()
                             && !o.OrganisationScopes.Any(
                                 sc => sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.OutOfScope)
                             && !o.UserOrganisations.Any(
                                 uo => uo.Method == RegistrationMethods.Manual || uo.PINConfirmedDate != null || uo.PINSentDate > deadline)
                             && !o.OrganisationAddresses.Any(a => a.CreatedByUserId == -1))
                    .ToListAsync();

                if (orgs.Any())
                {
                    var count = 0;
                    foreach (Organisation org in orgs)
                    {
                        var logItem = new ManualChangeLogModel(
                            nameof(PurgeOrganisations),
                            ManualActions.Delete,
                            AppDomain.CurrentDomain.FriendlyName,
                            nameof(org.OrganisationId),
                            org.OrganisationId.ToString(),
                            null,
                            JsonConvert.SerializeObject(
                                new {
                                    org.OrganisationId,
                                    Address = org.GetAddressString(),
                                    org.EmployerReference,
                                    org.CompanyNumber,
                                    org.OrganisationName,
                                    org.SectorType,
                                    org.Status,
                                    SicCodes = org.GetSicSectionIdsString(),
                                    SicSource = org.GetSicSource(),
                                    org.DateOfCessation
                                }),
                            null);
                        EmployerSearchModel searchRecord = org.ToEmployerSearchResult(true);

                        await _DataRepository.BeginTransactionAsync(
                            async () => {
                                try
                                {
                                    org.LatestAddress = null;
                                    org.LatestRegistration = null;
                                    org.LatestReturn = null;
                                    org.LatestScope = null;
                                    org.UserOrganisations.ForEach(uo => _DataRepository.Delete(uo));
                                    await _DataRepository.SaveChangesAsync();

                                    _DataRepository.Delete(org);
                                    await _DataRepository.SaveChangesAsync();

                                    _DataRepository.CommitTransaction();
                                }
                                catch (Exception ex)
                                {
                                    _DataRepository.RollbackTransaction();
                                    log.LogError(
                                        ex,
                                        $"{nameof(PurgeOrganisations)}: Failed to purge organisation {org.OrganisationId} '{org.OrganisationName}' ERROR: {ex.Message}:{ex.GetDetailsText()}");
                                }
                            });
                        //Remove this organisation from the search index
                        await Global.SearchRepository.RemoveFromIndexAsync(new[] {searchRecord});

                        await Global.ManualChangeLog.WriteAsync(logItem);
                        count++;
                    }

                    CustomLogger.Information($"Executed {nameof(PurgeOrganisations)} successfully: {count} organisations deleted" , new
                    {
                        runId,
                        Environment = Config.EnvironmentName,
                    });
                }
                LogFunctionEnd(runId, nameof(PurgeOrganisations), startTime);
            }
            catch (Exception ex)
            {
                LogFunctionError(runId, nameof(PurgeOrganisations), startTime, ex );
                
                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", 
                    $"Failed webjob ({nameof(PurgeOrganisations)}):{ex.Message}:{ex.GetDetailsText()}");
                //Rethrow the error
                throw;
            }
        }

        //Remove retired copies of GPG data
        public async Task PurgeGPGData([TimerTrigger("30 4 * * *" /* 04:30 once per day */)] TimerInfo timer, ILogger log)
        {
            var runId = CreateRunId();
            var startTime = DateTime.Now;
            LogFunctionStart(runId,  nameof(PurgeGPGData), startTime);
            try
            {
                DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeRetiredReturnDays);
                List<Return> returns = await _DataRepository.GetAll<Return>()
                    .Where(r => r.StatusDate < deadline && (r.Status == ReturnStatuses.Retired || r.Status == ReturnStatuses.Deleted))
                    .ToListAsync();

                foreach (Return @return in returns)
                {
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeGPGData),
                        ManualActions.Delete,
                        AppDomain.CurrentDomain.FriendlyName,
                        nameof(@return.ReturnId),
                        @return.ReturnId.ToString(),
                        null,
                        JsonConvert.SerializeObject(@return.ToDownloadResult()),
                        null);
                    _DataRepository.Delete(@return);
                    await _DataRepository.SaveChangesAsync();
                    await Global.ManualChangeLog.WriteAsync(logItem);
                }

                LogFunctionEnd(runId, nameof(PurgeGPGData), startTime);
            }
            catch (Exception ex)
            {
                LogFunctionError(runId, nameof(PurgeGPGData), startTime, ex );

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", 
                    $"Failed webjob ({nameof(PurgeGPGData)}):{ex.Message}:{ex.GetDetailsText()}");
                //Rethrow the error
                throw;
            }
        }

        //Remove test users and organisations
        [Disable(typeof(DisableWebjobProvider))]
        public async Task PurgeTestDataAsync([TimerTrigger("40 4 * * *" /* 04:40 once per day */)]
            TimerInfo timer,
            ILogger log)
        {
            if (Config.IsProduction())
            {
                return;
            }
            
            var runId = CreateRunId();
            var startTime = DateTime.Now;
            LogFunctionStart(runId,  nameof(PurgeTestDataAsync), startTime);

            try
            {
                GpgDatabaseContext.DeleteAllTestRecords(VirtualDateTime.Now.AddDays(-1));

                LogFunctionEnd(runId, nameof(PurgeTestDataAsync), startTime);
            }
            catch (Exception ex)
            {
                LogFunctionError(runId, nameof(PurgeTestDataAsync), startTime, ex );
                
                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", 
                    $"Failed webjob ({nameof(PurgeTestDataAsync)}):{ex.Message}:{ex.GetDetailsText()}");

                //Rethrow the error
                throw;
            }
        }

    }

}
