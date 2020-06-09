using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;

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

        public AuditLogger(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public void AuditGeneralAction(AuditedAction action, object anonymousObject, IPrincipal userWhoPerformedAction)
        {
            OriginalAndImpersonatedUser users = GetOriginalAndImpersonatedUser(userWhoPerformedAction);

            AuditAction(action, null, anonymousObject, users);
        }

        public void AuditChangeToOrganisation(AuditedAction action, Organisation organisationChanged, object anonymousObject)
        {
            var users = new OriginalAndImpersonatedUser();

            AuditAction(action, organisationChanged.OrganisationId, anonymousObject, users);
        }

        public void AuditChangeToOrganisation(AuditedAction action, Organisation organisationChanged, object anonymousObject, IPrincipal userWhoPerformedAction)
        {
            OriginalAndImpersonatedUser users = GetOriginalAndImpersonatedUser(userWhoPerformedAction);

            AuditAction(action, organisationChanged.OrganisationId, anonymousObject, users);
        }

        public void AuditChangeToOrganisation(AuditedAction action, Organisation organisationChanged, object anonymousObject, User userWhoPerformedAction)
        {
            var users = new OriginalAndImpersonatedUser {OriginalUser = userWhoPerformedAction};

            AuditAction(action, organisationChanged.OrganisationId, anonymousObject, users);
        }

        public void AuditChangeToUser(AuditedAction action, User userChanged, object anonymousObject, IPrincipal userWhoPerformedAction)
        {
            OriginalAndImpersonatedUser users = GetOriginalAndImpersonatedUser(userWhoPerformedAction);

            AuditAction(action, null, anonymousObject, users);
        }

        public void AuditChangeToUser(AuditedAction action, User userChanged, object anonymousObject)
        {
            var users = new OriginalAndImpersonatedUser();

            AuditAction(action, null, anonymousObject, users);
        }

        public void AuditChangeToUser(AuditedAction action, User userChanged, object anonymousObject, User userWhoPerformedAction)
        {
            var users = new OriginalAndImpersonatedUser { OriginalUser = userWhoPerformedAction };

            AuditAction(action, null, anonymousObject, users);
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

        private void AuditAction(AuditedAction action, long? organisationId, object anonymousObject, OriginalAndImpersonatedUser users)
        {
            Dictionary<string, string> details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            Organisation organisation = organisationId.HasValue ? dataRepository.Get<Organisation>(organisationId.Value) : null;

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


        private OriginalAndImpersonatedUser GetOriginalAndImpersonatedUser(IPrincipal user)
        {
            long impersonatedUserId;
            long originalUserId;

            try
            {
                if (LoginHelper.IsUserBeingImpersonated(user))
                {
                    originalUserId = LoginHelper.GetAdminImpersonatorUserId(user);
                    impersonatedUserId = LoginHelper.GetUserId(user);
                }
                else
                {
                    originalUserId = LoginHelper.GetUserId(user);
                    impersonatedUserId = 0;
                }
            }
            catch (Exception ex)
            {
                // If we are calling the AuditLogger from a background job, then:
                // - there won't be a session object (so we'll get an Exception)
                // - the concept of Original and Impersonated users doesn't apply
                impersonatedUserId = 0;
                originalUserId = 0;
            }

            User originalUser = dataRepository.Get<User>(originalUserId);
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
