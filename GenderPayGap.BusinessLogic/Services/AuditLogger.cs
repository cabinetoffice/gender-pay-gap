using System;
using System.Collections.Generic;
using System.Reflection;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.BusinessLogic.Services
{
    public class AuditLogger
    {

        private readonly IDataRepository dataRepository;
        private readonly IHttpSession session;

        public AuditLogger(IDataRepository dataRepository, IHttpSession session)
        {
            this.dataRepository = dataRepository;
            this.session = session;
        }

        public void AuditChangeToOrganisation(AuditedAction action, Organisation organisation, object anonymousObject, User currentUser)
        {
            Dictionary<string, string> details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            AuditActionToOrganisation(action, organisation.OrganisationId, details, currentUser);
        }

        public void AuditChangeToUser(AuditedAction action, User user, object anonymousObject, User currentUser)
        {
            Dictionary<string, string> details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            AuditActionToUser(action, user.UserId, details, currentUser);
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

        private void AuditActionToOrganisation(AuditedAction action, long organisationId, Dictionary<string, string> details, User currentUser)
        {
            var impersonatedUserId = session["ImpersonatedUserId"].ToInt64();
            var isImpersonating = impersonatedUserId > 0;
            var originalUserId = session["OriginalUser"].ToInt64();

            var originalUser = isImpersonating ? dataRepository.Get<User>(originalUserId) : currentUser;
            var impersonatedUser = dataRepository.Get<User>(impersonatedUserId);
            var organisation = dataRepository.Get<Organisation>(organisationId);

            dataRepository.Insert(
                new AuditLog
                {
                    Action = action,
                    OriginalUser = originalUser,
                    ImpersonatedUser = impersonatedUser,
                    Organisation = organisation,
                    Details = details
                });

            dataRepository.SaveChangesAsync().Wait();
        }

        private void AuditActionToUser(AuditedAction action, long actionTakenOnUserId, Dictionary<string, string> details, User currentUser)
        {
            var impersonatedUserId = session["ImpersonatedUserId"].ToInt64();
            var isImpersonating = impersonatedUserId > 0;
            var originalUserId = session["OriginalUser"].ToInt64();

            var originalUser = isImpersonating ? dataRepository.Get<User>(originalUserId) : currentUser;
            var impersonatedUser = dataRepository.Get<User>(actionTakenOnUserId);

            if (impersonatedUser.UserId == originalUser.UserId)
            {
                impersonatedUser = null;
            }

            dataRepository.Insert(
                new AuditLog
                {
                    Action = action,
                    OriginalUser = originalUser,
                    ImpersonatedUser = impersonatedUser,
                    Organisation = null,
                    Details = details
                });

            dataRepository.SaveChangesAsync().Wait();
        }

    }
}
