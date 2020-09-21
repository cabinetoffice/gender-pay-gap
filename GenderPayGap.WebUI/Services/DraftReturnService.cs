using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;

namespace GenderPayGap.WebUI.Services
{
    public class DraftReturnService
    {

        private readonly IDataRepository dataRepository;

        public DraftReturnService(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        public DraftReturn GetDraftReturn(long organisationId, int reportingYear)
        {
            return dataRepository.GetAll<DraftReturn>()
                .Where(dr => dr.OrganisationId == organisationId)
                .Where(dr => dr.SnapshotYear == reportingYear)
                .FirstOrDefault();
        }

        public DraftReturn GetOrCreateDraftReturn(long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = dataRepository.GetAll<DraftReturn>()
                .Where(dr => dr.OrganisationId == organisationId)
                .Where(dr => dr.SnapshotYear == reportingYear)
                .FirstOrDefault();

            if (draftReturn == null)
            {
                draftReturn = new DraftReturn
                {
                    OrganisationId = organisationId,
                    SnapshotYear = reportingYear
                };

                dataRepository.Insert(draftReturn);
                dataRepository.SaveChangesAsync().Wait();
            }

            return draftReturn;
        }

        public void SaveDraftReturnOrDeleteIfNotRelevent(DraftReturn draftReturn)
        {
            if (DraftReturnIsEmpty(draftReturn) ||
                DraftReturnIsSameAsSubmittedReturn(draftReturn))
            {
                dataRepository.Delete(draftReturn);
            }

            dataRepository.SaveChangesAsync().Wait();
        }

        private bool DraftReturnIsEmpty(DraftReturn draftReturn)
        {
            return
                draftReturn.DiffMeanHourlyPayPercent == null
                && draftReturn.DiffMedianHourlyPercent == null

                && draftReturn.DiffMeanBonusPercent == null
                && draftReturn.DiffMedianBonusPercent == null
                && draftReturn.MaleMedianBonusPayPercent == null
                && draftReturn.FemaleMedianBonusPayPercent == null

                && draftReturn.MaleLowerPayBand == null
                && draftReturn.FemaleLowerPayBand == null
                && draftReturn.MaleMiddlePayBand == null
                && draftReturn.FemaleMiddlePayBand == null
                && draftReturn.MaleUpperPayBand == null
                && draftReturn.FemaleUpperPayBand == null
                && draftReturn.MaleUpperQuartilePayBand == null
                && draftReturn.FemaleUpperQuartilePayBand == null

                && string.IsNullOrWhiteSpace(draftReturn.FirstName)
                && string.IsNullOrWhiteSpace(draftReturn.LastName)
                && string.IsNullOrWhiteSpace(draftReturn.JobTitle)

                && draftReturn.OrganisationSize == null

                && string.IsNullOrWhiteSpace(draftReturn.CompanyLinkToGPGInfo)
                ;
        }

        private bool DraftReturnIsSameAsSubmittedReturn(DraftReturn draftReturn)
        {
            Organisation organisation = dataRepository.Get<Organisation>(draftReturn.OrganisationId);
            Return submittedReturn = organisation.GetReturn(draftReturn.SnapshotYear);

            return
                submittedReturn != null
                && draftReturn.DiffMeanHourlyPayPercent == submittedReturn.DiffMeanHourlyPayPercent
                && draftReturn.DiffMedianHourlyPercent == submittedReturn.DiffMedianHourlyPercent

                && draftReturn.DiffMeanBonusPercent == submittedReturn.DiffMeanBonusPercent
                && draftReturn.DiffMedianBonusPercent == submittedReturn.DiffMedianBonusPercent
                && draftReturn.MaleMedianBonusPayPercent == submittedReturn.MaleMedianBonusPayPercent
                && draftReturn.FemaleMedianBonusPayPercent == submittedReturn.FemaleMedianBonusPayPercent

                && draftReturn.MaleLowerPayBand == submittedReturn.MaleLowerPayBand
                && draftReturn.FemaleLowerPayBand == submittedReturn.FemaleLowerPayBand
                && draftReturn.MaleMiddlePayBand == submittedReturn.MaleMiddlePayBand
                && draftReturn.FemaleMiddlePayBand == submittedReturn.FemaleMiddlePayBand
                && draftReturn.MaleUpperPayBand == submittedReturn.MaleUpperPayBand
                && draftReturn.FemaleUpperPayBand == submittedReturn.FemaleUpperPayBand
                && draftReturn.MaleUpperQuartilePayBand == submittedReturn.MaleUpperQuartilePayBand
                && draftReturn.FemaleUpperQuartilePayBand == submittedReturn.FemaleUpperQuartilePayBand

                && draftReturn.FirstName == submittedReturn.FirstName
                && draftReturn.LastName == submittedReturn.LastName
                && draftReturn.JobTitle == submittedReturn.JobTitle

                && draftReturn.OrganisationSize == submittedReturn.OrganisationSize

                && draftReturn.CompanyLinkToGPGInfo == submittedReturn.CompanyLinkToGPGInfo
                ;
        }



    }
}
