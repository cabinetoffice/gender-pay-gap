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

        private void AuditActionToOrganisation(AuditedAction action, long organisationId, Dictionary<string, string> details, User userWhoPerformedAction)
        {
            var impersonatedUserId = session["ImpersonatedUserId"].ToInt64();
            var isImpersonating = impersonatedUserId > 0;
            var originalUserId = session["OriginalUser"].ToInt64();

            var originalUser = isImpersonating ? dataRepository.Get<User>(originalUserId) : userWhoPerformedAction;
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

        private void AuditActionToUser(AuditedAction action, long actionTakenOnUserId, Dictionary<string, string> details, User userWhoPerformedAction)
        {
            var impersonatedUserId = session["ImpersonatedUserId"].ToInt64();
            var isImpersonating = impersonatedUserId > 0;
            var originalUserId = session["OriginalUser"].ToInt64();

            var originalUser = isImpersonating ? dataRepository.Get<User>(originalUserId) : userWhoPerformedAction;
            var impersonatedUser = dataRepository.Get<User>(actionTakenOnUserId);

            if (impersonatedUser?.UserId == originalUser?.UserId)
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
