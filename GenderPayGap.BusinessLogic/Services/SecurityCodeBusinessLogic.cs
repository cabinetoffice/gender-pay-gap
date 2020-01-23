using System;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Database;
using GenderPayGap.Extensions;

namespace GenderPayGap.BusinessLogic.Services
{
    public interface ISecurityCodeBusinessLogic
    {

        CustomResult<Organisation> CreateSecurityCode(Organisation organisation,
            DateTime securityCodeExpiryDateTime);

        CustomResult<Organisation> ExtendSecurityCode(Organisation organisation,
            DateTime securityCodeExpiryDateTime);

        CustomResult<Organisation> ExpireSecurityCode(Organisation organisation);

    }

    public class SecurityCodeBusinessLogic : ISecurityCodeBusinessLogic
    {

        public CustomResult<Organisation> CreateSecurityCode(Organisation organisation, DateTime securityCodeExpiryDateTime)
        {
            if (!(organisation.IsActive() | organisation.IsPending()))
            {
                return new CustomResult<Organisation>(
                    InternalMessages.SecurityCodeCreateIsOnlyAllowedToNonRetiredOrgsErrorMessage(
                        organisation.OrganisationName,
                        organisation.EmployerReference,
                        organisation.Status.ToString()),
                    organisation);
            }

            return SecurityCodeGenericMethod(
                organisation,
                securityCodeExpiryDateTime,
                null,
                SecurityCodeDateTimeInFutureValidate,
                SetOrganisationSecurityCode);
        }

        public CustomResult<Organisation> ExtendSecurityCode(Organisation organisation, DateTime securityCodeExpiryDateTime)
        {
            return SecurityCodeGenericMethod(
                organisation,
                securityCodeExpiryDateTime,
                SecurityCodeNotExpiredValidate,
                SecurityCodeDateTimeInFutureValidate,
                SetOrganisationSecurityCodeExpiryDate);
        }

        public CustomResult<Organisation> ExpireSecurityCode(Organisation organisation)
        {
            return SecurityCodeGenericMethod(
                organisation,
                VirtualDateTime.Now.AddDays(-1),
                SecurityCodeNotExpiredValidate,
                null,
                SetOrganisationSecurityCodeExpiryDate);
        }

        private CustomResult<Organisation> SecurityCodeDateTimeInFutureValidate(Organisation organisation,
            DateTime securityCodeExpiryDateTime)
        {
            CustomResult<Organisation> futureValidationResult = null;

            if (securityCodeExpiryDateTime < VirtualDateTime.Now)
            {
                futureValidationResult = new CustomResult<Organisation>(
                    InternalMessages.SecurityCodeMustExpireInFutureErrorMessage(),
                    organisation);
            }

            return futureValidationResult;
        }

        private CustomResult<Organisation> SecurityCodeNotExpiredValidate(Organisation organisation)
        {
            CustomResult<Organisation> securityCodeNotExpiredValidationResult = null;

            if (organisation?.SecurityCodeExpiryDateTime != null && organisation.SecurityCodeExpiryDateTime < VirtualDateTime.Now)
            {
                securityCodeNotExpiredValidationResult = new CustomResult<Organisation>(
                    InternalMessages.SecurityCodeCannotModifyAnAlreadyExpiredSecurityCodeErrorMessage(),
                    organisation);
            }

            return securityCodeNotExpiredValidationResult;
        }

        private Organisation SetOrganisationSecurityCode(Organisation organisation, DateTime securityCodeExpiryDateTime)
        {
            organisation.SetSecurityCode(securityCodeExpiryDateTime);
            return organisation;
        }

        private Organisation SetOrganisationSecurityCodeExpiryDate(Organisation organisation, DateTime securityCodeExpiryDateTime)
        {
            organisation.SetSecurityCodeExpiryDate(securityCodeExpiryDateTime);
            return organisation;
        }

        private CustomResult<Organisation> SecurityCodeGenericMethod(Organisation organisation,
            DateTime securityCodeExpiryDateTime,
            SecurityCodeNotExpiredValidator securityCodeNotExpiredValidator,
            SecurityCodeDateTimeValidator securityCodeDateTimeValidator,
            GetOrganisationAndWorkWithSecurityCode getOrganisationAndWorkWithSecurityCode)
        {
            CustomResult<Organisation> result;

            try
            {
                CustomResult<Organisation> securityCodeNotExpiredValidationResult = securityCodeNotExpiredValidator?.Invoke(organisation);
                if (securityCodeNotExpiredValidationResult != null)
                {
                    return securityCodeNotExpiredValidationResult;
                }

                CustomResult<Organisation> securityValidationResult =
                    securityCodeDateTimeValidator?.Invoke(organisation, securityCodeExpiryDateTime);
                if (securityValidationResult != null)
                {
                    return securityValidationResult;
                }

                Organisation org = getOrganisationAndWorkWithSecurityCode(organisation, securityCodeExpiryDateTime);
                result = new CustomResult<Organisation>(org);
            }
            catch (Exception ex)
            {
                result = new CustomResult<Organisation>(new CustomError(0, ex.Message), organisation);
            }

            return result;
        }

        private delegate CustomResult<Organisation> SecurityCodeDateTimeValidator(Organisation organisation,
            DateTime securityCodeExpiryDateTime);

        private delegate CustomResult<Organisation> SecurityCodeNotExpiredValidator(Organisation organisation);

        private delegate Organisation
            GetOrganisationAndWorkWithSecurityCode(Organisation organisation, DateTime securityCodeExpiryDateTime);

    }
}
