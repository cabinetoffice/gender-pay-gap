using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;

namespace GenderPayGap.WebUI.BusinessLogic.Services
{

    public interface ISubmissionBusinessLogic
    {

        // Submission
        Return GetSubmissionByReturnId(long returnId);
        CustomResult<Return> GetSubmissionByOrganisationAndYear(Organisation organisation, int year);

    }

    public class SubmissionBusinessLogic : ISubmissionBusinessLogic
    {
        public SubmissionBusinessLogic(IDataRepository dataRepo)
        {
            DataRepository = dataRepo;
        }

        private IDataRepository DataRepository { get; }

        #region Repo

        public virtual Return GetSubmissionByReturnId(long returnId)
        {
            return DataRepository.GetAll<Return>()
                .FirstOrDefault(o => o.ReturnId == returnId);
        }
        
        public IEnumerable<Return> GetAllSubmissionsByOrganisationIdAndSnapshotYear(long organisationId, int snapshotYear)
        {
            return DataRepository.GetAll<Return>()
                .Where(s => s.OrganisationId == organisationId)
                .Where(s => s.AccountingDate.Year == snapshotYear);
        }

        public CustomResult<Return> GetSubmissionByOrganisationAndYear(Organisation organisation, int year)
        {
            IEnumerable<Return> reports = GetAllSubmissionsByOrganisationIdAndSnapshotYear(organisation.OrganisationId, year);

            if (!reports.Any())
            {
                return new CustomResult<Return>(
                    InternalMessages.HttpNotFoundCausedByOrganisationReturnNotInDatabase(organisation.GetEncryptedId(), year));
            }

            Return result = reports.OrderByDescending(r => r.Status == ReturnStatuses.Submitted)
                .ThenByDescending(r => r.StatusDate)
                .FirstOrDefault();
            if (!result.IsSubmitted())
            {
                return new CustomResult<Return>(
                    InternalMessages.HttpGoneCausedByReportNotHavingBeenSubmitted(result.AccountingDate.Year, result.Status.ToString()));
            }

            return new CustomResult<Return>(result);
        }

        #endregion

    }

}
