using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.BusinessLogic
{

    public interface ISubmissionBusinessLogic
    {

        // Submission
        Task<Return> GetSubmissionByReturnIdAsync(long returnId);
        Task<Return> GetLatestSubmissionBySnapshotYearAsync(long organisationId, int snapshotYear);
        ReturnViewModel ConvertSubmissionReportToReturnViewModel(Return reportToConvert);
        CustomResult<Return> GetSubmissionByOrganisationAndYear(Organisation organisation, int year);

    }

    public class SubmissionBusinessLogic : ISubmissionBusinessLogic
    {

        private ICommonBusinessLogic _commonBusinessLogic;

        public SubmissionBusinessLogic(ICommonBusinessLogic commonBusinessLogic, IDataRepository dataRepo)
        {
            _commonBusinessLogic = commonBusinessLogic;
            DataRepository = dataRepo;
        }

        private IDataRepository DataRepository { get; }

        #region Repo

        public virtual async Task<Return> GetSubmissionByReturnIdAsync(long returnId)
        {
            return await DataRepository.GetAll<Return>()
                .FirstOrDefaultAsync(o => o.ReturnId == returnId);
        }

        /// <summary>
        ///     Gets the latest submitted return for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        /// <returns></returns>
        public virtual async Task<Return> GetLatestSubmissionBySnapshotYearAsync(long organisationId, int snapshotYear)
        {
            Return orgSubmission = await DataRepository.GetAll<Return>()
                .FirstOrDefaultAsync(
                    s => s.AccountingDate.Year == snapshotYear
                         && s.OrganisationId == organisationId
                         && s.Status == ReturnStatuses.Submitted);

            return orgSubmission;
        }

        public IEnumerable<Return> GetAllSubmissionsByOrganisationIdAndSnapshotYear(long organisationId, int snapshotYear)
        {
            return DataRepository.GetAll<Return>().Where(s => s.OrganisationId == organisationId && s.AccountingDate.Year == snapshotYear);
        }

        public ReturnViewModel ConvertSubmissionReportToReturnViewModel(Return reportToConvert)
        {
            var model = new ReturnViewModel {
                SectorType = reportToConvert.Organisation.SectorType,
                ReturnId = reportToConvert.ReturnId,
                OrganisationId = reportToConvert.OrganisationId,
                EncryptedOrganisationId = reportToConvert.Organisation.GetEncryptedId(),
                DiffMeanBonusPercent = reportToConvert.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = reportToConvert.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = reportToConvert.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = reportToConvert.DiffMedianHourlyPercent,
                FemaleLowerPayBand = reportToConvert.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = reportToConvert.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = reportToConvert.FemaleMiddlePayBand,
                FemaleUpperPayBand = reportToConvert.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = reportToConvert.FemaleUpperQuartilePayBand,
                MaleLowerPayBand = reportToConvert.MaleLowerPayBand,
                MaleMedianBonusPayPercent = reportToConvert.MaleMedianBonusPayPercent,
                MaleMiddlePayBand = reportToConvert.MaleMiddlePayBand,
                MaleUpperPayBand = reportToConvert.MaleUpperPayBand,
                MaleUpperQuartilePayBand = reportToConvert.MaleUpperQuartilePayBand,
                JobTitle = reportToConvert.JobTitle,
                FirstName = reportToConvert.FirstName,
                LastName = reportToConvert.LastName,
                CompanyLinkToGPGInfo = reportToConvert.CompanyLinkToGPGInfo,
                AccountingDate = reportToConvert.AccountingDate,
                Address = reportToConvert.Organisation.GetLatestAddress()?.GetAddressString(),
                LatestAddress = reportToConvert.Organisation.GetLatestAddress()?.GetAddressString(),
                EHRCResponse = reportToConvert.EHRCResponse.ToString(),
                IsVoluntarySubmission = reportToConvert.IsVoluntarySubmission(),
                IsLateSubmission = reportToConvert.IsLateSubmission
            };

            if (model.Address.EqualsI(model.LatestAddress))
            {
                model.LatestAddress = null;
            }

            model.OrganisationName = reportToConvert.Organisation.GetName(reportToConvert.StatusDate)?.Name
                                     ?? reportToConvert.Organisation.OrganisationName;
            model.LatestOrganisationName = reportToConvert.Organisation.OrganisationName;

            model.Sector = reportToConvert.Organisation.GetSicSectorsString(reportToConvert.StatusDate);
            model.LatestSector = reportToConvert.Organisation.GetSicSectorsString();

            model.OrganisationSize = reportToConvert.OrganisationSize;
            model.Modified = reportToConvert.Modified;

            model.IsInScopeForThisReportYear =
                reportToConvert.Organisation.GetIsInscope(reportToConvert.AccountingDate.Year);

            return model;
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
