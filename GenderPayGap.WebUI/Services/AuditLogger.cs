using System;
using System.Collections.Generic;
using System.Reflection;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebUI.Services
{
    public class AuditLogger
    {
        private class OriginalAndImpersonatedUser
        {
            public User OriginalUser { get; set; }
            public User ImpersonatedUser { get; set; }
        }


        private readonly IDataRepository dataRepository;
        private readonly IHttpSession session;

        public AuditLogger(IDataRepository dataRepository, IHttpSession session)
        {
            this.dataRepository = dataRepository;
            this.session = session;
        }

        public void AuditGeneralAction(AuditedAction action, object anonymousObject, User userWhoPerformedAction)
        {
            Dictionary<string, string> details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            AuditGeneralAction(action, details, userWhoPerformedAction);
        }

        public void AuditChangeToOrganisation(AuditedAction action, Organisation organisationChanged, object anonymousObject, User userWhoPerformedAction)
        {
            Dictionary<string, string> details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            AuditActionToOrganisation(action, organisationChanged.OrganisationId, details, userWhoPerformedAction);
        }

        public void AuditChangeToUser(AuditedAction action, User userChanged, object anonymousObject, User userWhoPerformedAction)
        {
            Dictionary<string, string> details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            AuditActionToUser(action, userChanged.UserId, details, userWhoPerformedAction);
        }

        private static Dictionary<string, string> ExtractDictionaryOfDetailsFromAnonymousObject(object anonymousObject)
        {
            var details = new Dictionary<string, string>();

            Type type = anonymousObject.GetType();
            PropertyInfo[] propertyInfos = type.GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string propertyName = propertyInfo.Name;
                string propertyValue = propertyInfo.GetValue(anonymousObject)?.ToString();

                details.Add(propertyName, propertyValue);
            }

            return details;
        }

        private void AuditGeneralAction(AuditedAction action, Dictionary<string, string> details, User userWhoPerformedAction)
        {
            OriginalAndImpersonatedUser users = GetOriginalAndImpersonatedUser(userWhoPerformedAction);

            dataRepository.Insert(
                new AuditLog
                {
                    Action = action,
                    OriginalUser = users.OriginalUser,
                    ImpersonatedUser = users.ImpersonatedUser,
                    Organisation = null,
                    Details = details
                });

            dataRepository.SaveChangesAsync().Wait();
        }

        private void AuditActionToOrganisation(AuditedAction action, long organisationId, Dictionary<string, string> details, User userWhoPerformedAction)
        {
            OriginalAndImpersonatedUser users = GetOriginalAndImpersonatedUser(userWhoPerformedAction);

            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            dataRepository.Insert(
                new AuditLog
                {
                    Action = action,
                    OriginalUser = users.OriginalUser,
                    ImpersonatedUser = users.ImpersonatedUser,
                    Organisation = organisation,
                    Details = details
                });

            dataRepository.SaveChangesAsync().Wait();
        }

        private void AuditActionToUser(AuditedAction action, long actionTakenOnUserId, Dictionary<string, string> details, User userWhoPerformedAction)
        {
            OriginalAndImpersonatedUser users = GetOriginalAndImpersonatedUser(userWhoPerformedAction);

            dataRepository.Insert(
                new AuditLog
                {
                    Action = action,
                    OriginalUser = users.OriginalUser,
                    ImpersonatedUser = users.ImpersonatedUser,
                    Organisation = null,
                    Details = details
                });

            dataRepository.SaveChangesAsync().Wait();
        }

        private OriginalAndImpersonatedUser GetOriginalAndImpersonatedUser(User userWhoPerformedAction)
        {
            long impersonatedUserId;
            long originalUserId;
            bool isImpersonating;

            try
            {
                impersonatedUserId = session["ImpersonatedUserId"].ToInt64();
                isImpersonating = impersonatedUserId > 0;
                originalUserId = session["OriginalUser"].ToInt64();
            }
            catch (Exception ex)
            {
                // If we are calling the AuditLogger from a background job, then:
                // - there won't be a session object (so we'll get an Exception)
                // - the concept of Original and Impersonated users doesn't apply
                impersonatedUserId = 0;
                originalUserId = 0;
                isImpersonating = false;
            }

            User originalUser = isImpersonating ? dataRepository.Get<User>(originalUserId) : userWhoPerformedAction;
            User impersonatedUser = dataRepository.Get<User>(impersonatedUserId);

            if (impersonatedUser?.UserId == originalUser?.UserId)
            {
                impersonatedUser = null;
            }

            return new OriginalAndImpersonatedUser
            {
                OriginalUser = originalUser,
                ImpersonatedUser = impersonatedUser
            };
        }

    }
}
