using System.Net;

namespace GenderPayGap.Core.Classes.ErrorMessages
{
    public class InternalMessages
    {

        public static CustomError SameScopesCannotBeUpdated(ScopeStatuses newScopeStatus, ScopeStatuses oldScopeStatus, int snapshotYear)
        {
            return new CustomError(
                4006,
                $"Unable to update to {newScopeStatus} as the record for {snapshotYear} is already showing as {oldScopeStatus}");
        }

        public static CustomError OrganisationRevertOnlyRetiredErrorMessage(string organisationName,
            string employerReference,
            string status)
        {
            return new CustomError(
                4005,
                $"Only organisations with current status 'Retired' are allowed to be reverted, Organisation '{organisationName}' employerReference '{employerReference}' has status '{status}'.");
        }

        public static CustomError SecurityCodeMustExpireInFutureErrorMessage()
        {
            return new CustomError(4004, "Security code must expire in the future");
        }

        public static CustomError SecurityCodeCannotModifyAnAlreadyExpiredSecurityCodeErrorMessage()
        {
            return new CustomError(4004, "Cannot modify the security code information of an already expired security code");
        }

        public static CustomError SecurityCodeCreateIsOnlyAllowedToNonRetiredOrgsErrorMessage(string organisationName,
            string employerReference,
            string status)
        {
            return new CustomError(
                4003,
                $"Generation of security codes cannot be performed for retired organisations. Organisation '{organisationName}' employerReference '{employerReference}' has status '{status}'.");
        }

        public static CustomError HttpBadRequestCausedByInvalidEmployerIdentifier(string employerIdentifier)
        {
            return new CustomError(HttpStatusCode.BadRequest, $"Bad employer identifier {employerIdentifier}");
        }

        public static CustomError HttpNotFoundCausedByOrganisationIdNotInDatabase(string employerIdentifier)
        {
            return new CustomError(HttpStatusCode.NotFound, $"Employer: Could not find organisation '{employerIdentifier}'");
        }

        public static CustomError HttpGoneCausedByOrganisationBeingInactive(OrganisationStatuses organisationStatus)
        {
            return new CustomError(HttpStatusCode.Gone, $"Employer: The status of this organisation is '{organisationStatus}'");
        }

        public static CustomError HttpNotFoundCausedByOrganisationReturnNotInDatabase(string organisationIdEncrypted, int year)
        {
            return new CustomError(
                HttpStatusCode.NotFound,
                $"Employer: Could not find GPG Data for organisation:{organisationIdEncrypted} and year:{year}");
        }

        public static CustomError HttpGoneCausedByReportNotHavingBeenSubmitted(int reportYear, string reportStatus)
        {
            return new CustomError(HttpStatusCode.Gone, $"Employer report '{reportYear}' is showing with status '{reportStatus}'");
        }

        public static CustomError HttpNotFoundCausedByReturnIdNotInDatabase(string returnIdEncrypted)
        {
            return new CustomError(HttpStatusCode.NotFound, $"Employer: Could not find GPG Data for returnId:'{returnIdEncrypted}'");
        }

    }
}
