using System.Net;

namespace GenderPayGap.Core.Classes.ErrorMessages
{
    public class InternalMessages
    {

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
