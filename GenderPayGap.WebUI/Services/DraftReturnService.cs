using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;

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
            DraftReturn draftReturn = GetDraftReturn(organisationId, reportingYear);

            if (draftReturn == null)
            {
                Organisation organisation = dataRepository.Get<Organisation>(organisationId);
                Return submittedReturn = organisation.GetReturn(reportingYear);

                if (submittedReturn != null)
                {
                    draftReturn = CreateDraftReturnBasedOnSubmittedReturn(submittedReturn);
                }
                else
                {
                    draftReturn = CreateBlankDraftReturn(organisationId, reportingYear);
                }

                dataRepository.Insert(draftReturn);
                dataRepository.SaveChanges();
            }

            return draftReturn;
        }

        private DraftReturn CreateDraftReturnBasedOnSubmittedReturn(Return submittedReturn)
        {
            return new DraftReturn
            {
                OrganisationId = submittedReturn.OrganisationId,
                SnapshotYear = submittedReturn.AccountingDate.Year,

                DiffMeanHourlyPayPercent = submittedReturn.DiffMeanHourlyPayPercent,
                DiffMedianHourlyPercent = submittedReturn.DiffMedianHourlyPercent,

                MaleMedianBonusPayPercent = submittedReturn.MaleMedianBonusPayPercent,
                FemaleMedianBonusPayPercent = submittedReturn.FemaleMedianBonusPayPercent,
                DiffMeanBonusPercent = submittedReturn.DiffMeanBonusPercent,
                DiffMedianBonusPercent = submittedReturn.DiffMedianBonusPercent,

                MaleUpperQuartilePayBand = submittedReturn.MaleUpperQuartilePayBand,
                FemaleUpperQuartilePayBand = submittedReturn.FemaleUpperQuartilePayBand,
                MaleUpperPayBand = submittedReturn.MaleUpperPayBand,
                FemaleUpperPayBand = submittedReturn.FemaleUpperPayBand,
                MaleMiddlePayBand = submittedReturn.MaleMiddlePayBand,
                FemaleMiddlePayBand = submittedReturn.FemaleMiddlePayBand,
                MaleLowerPayBand = submittedReturn.MaleLowerPayBand,
                FemaleLowerPayBand = submittedReturn.FemaleLowerPayBand,
                
                FirstName = submittedReturn.FirstName,
                LastName = submittedReturn.LastName,
                JobTitle = submittedReturn.JobTitle,

                OrganisationSize = submittedReturn.OrganisationSize,

                CompanyLinkToGPGInfo = submittedReturn.CompanyLinkToGPGInfo
            };
        }

        private static DraftReturn CreateBlankDraftReturn(long organisationId, int reportingYear)
        {
            return new DraftReturn
            {
                OrganisationId = organisationId,
                SnapshotYear = reportingYear
            };
        }

        public void SaveDraftReturnOrDeleteIfNotRelevant(DraftReturn draftReturn)
        {
            Organisation organisation = dataRepository.Get<Organisation>(draftReturn.OrganisationId);
            Return submittedReturn = organisation.GetReturn(draftReturn.SnapshotYear);

            if (draftReturn.IsEmpty() || draftReturn.IsSameAsSubmittedReturn(submittedReturn))
            {
                dataRepository.Delete(draftReturn);
            }
            else
            {
                draftReturn.Modified = VirtualDateTime.Now;
            }

            dataRepository.SaveChanges();
        }

        public bool DraftReturnExistsAndRequiredFieldsAreComplete(long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = GetDraftReturn(organisationId, reportingYear);
            if (draftReturn == null)
            {
                return false;
            }

            var organisation = dataRepository.Get<Organisation>(organisationId);

            bool employeesByPayQuartileSectionIsComplete =
                (ReportingYearsHelper.IsReportingYearWithFurloughScheme(reportingYear) && draftReturn.OptedOutOfReportingPayQuarters) 
                || EmployeesByPayQuartileSectionIsFilledIn(draftReturn);

            return HourlyPaySectionIsComplete(draftReturn)
                   && BonusPaySectionIsComplete(draftReturn)
                   && employeesByPayQuartileSectionIsComplete
                   && ResponsiblePersonSectionIsComplete(draftReturn, organisation)
                   && WebsiteLinkSectionIsComplete(draftReturn);
        }

        private bool HourlyPaySectionIsComplete(DraftReturn draftReturn)
        {
            return draftReturn.DiffMeanHourlyPayPercent != null
                && draftReturn.DiffMedianHourlyPercent != null;
        }
        
        private bool BonusPaySectionIsComplete(DraftReturn draftReturn)
        {
            bool noBonusesPaid =
                draftReturn.MaleMedianBonusPayPercent == 0
                && draftReturn.FemaleMedianBonusPayPercent == 0;

            bool allValuesComplete =
                draftReturn.MaleMedianBonusPayPercent != null
                && draftReturn.FemaleMedianBonusPayPercent != null
                && draftReturn.DiffMeanBonusPercent != null
                && draftReturn.DiffMedianBonusPercent != null;
            
            return noBonusesPaid || allValuesComplete;
        }

        private bool EmployeesByPayQuartileSectionIsFilledIn(DraftReturn draftReturn)
        {
            return draftReturn.MaleLowerPayBand != null
                   && draftReturn.FemaleLowerPayBand != null
                   && draftReturn.MaleMiddlePayBand != null
                   && draftReturn.FemaleMiddlePayBand != null
                   && draftReturn.MaleUpperPayBand != null
                   && draftReturn.FemaleUpperPayBand != null
                   && draftReturn.MaleUpperQuartilePayBand != null
                   && draftReturn.FemaleUpperQuartilePayBand != null;
        }

        private bool ResponsiblePersonSectionIsComplete(DraftReturn draftReturn, Organisation organisation)
        {
            return organisation.SectorType == SectorTypes.Public // Public sector organisations don't have to provide a responsible person
                   || (!string.IsNullOrWhiteSpace(draftReturn.FirstName)
                       && !string.IsNullOrWhiteSpace(draftReturn.LastName)
                       && !string.IsNullOrWhiteSpace(draftReturn.JobTitle));
        }

        private bool WebsiteLinkSectionIsComplete(DraftReturn draftReturn)
        {
            // The website link is optional, so we should allow it to be missing
            bool linkIsMissing = string.IsNullOrEmpty(draftReturn.CompanyLinkToGPGInfo);
            bool linkIsValid = UriSanitiser.IsValidHttpOrHttpsLink(draftReturn.CompanyLinkToGPGInfo);
            return linkIsMissing || linkIsValid;
        }

        public bool ShouldShowLateWarning(long organisationId, int reportingYear)
        {
            var organisation = dataRepository.Get<Organisation>(organisationId);
            
            return ReportIsLate(organisation, reportingYear) 
                   && OrganisationIsInScope(organisation, reportingYear) 
                   && ReportingYearIsNotExcludedFromLateEnforcement(reportingYear);
        }

        public bool DraftReturnWouldBeNewlyLateIfSubmittedNow(DraftReturn draftReturn)
        {
            Organisation organisation = dataRepository.Get<Organisation>(draftReturn.OrganisationId);
            int reportingYear = draftReturn.SnapshotYear;
            
            return ReportIsLate(organisation, reportingYear) 
                   && OrganisationSizeMakesReportMandatory(draftReturn) 
                   && OrganisationIsInScope(organisation, reportingYear) 
                   && ReportingYearIsNotExcludedFromLateEnforcement(reportingYear)
                   && IsDraftReturnAMaterialChange(draftReturn, organisation);
        }
        
        private bool ReportIsLate(Organisation organisation, int reportingYear)
        {
            // The deadline date is the final day that a return can be submitted without being considered late
            // The due date is a day later, the point at which a return is considered late
            // i.e. if the deadline date is 2021/04/01, submissions on that day are not late, any after 2021/04/02 00:00:00 are
            DateTime snapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
            DateTime dueDate = ReportingYearsHelper.GetDeadlineForAccountingDate(snapshotDate).AddDays(1);
            return VirtualDateTime.Now > dueDate;
        }

        private bool OrganisationIsInScope(Organisation organisation, int reportingYear)
        {
            return organisation.GetScopeForYear(reportingYear).IsInScopeVariant();
        }

        private bool ReportingYearIsNotExcludedFromLateEnforcement(int reportingYear)
        {
            return !Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(reportingYear);
        }
        
        private bool OrganisationSizeMakesReportMandatory(DraftReturn draftReturn)
        {
            return draftReturn.OrganisationSize != OrganisationSizes.Employees0To249;
        }

        private bool IsDraftReturnAMaterialChange(DraftReturn draftReturn, Organisation organisation)
        {
            Return submittedReturn = organisation.GetReturn(draftReturn.SnapshotYear);
            if (submittedReturn == null)
            {
                // If this is the first Return of the year, then it must include a material change (figures)
                return true;
            }

            bool isMaterialChange =
                draftReturn.DiffMeanHourlyPayPercent != submittedReturn.DiffMeanHourlyPayPercent
                || draftReturn.DiffMedianHourlyPercent != submittedReturn.DiffMedianHourlyPercent

                || draftReturn.DiffMeanBonusPercent != submittedReturn.DiffMeanBonusPercent
                || draftReturn.DiffMedianBonusPercent != submittedReturn.DiffMedianBonusPercent
                || draftReturn.MaleMedianBonusPayPercent != submittedReturn.MaleMedianBonusPayPercent
                || draftReturn.FemaleMedianBonusPayPercent != submittedReturn.FemaleMedianBonusPayPercent

                || draftReturn.MaleLowerPayBand != submittedReturn.MaleLowerPayBand
                || draftReturn.FemaleLowerPayBand != submittedReturn.FemaleLowerPayBand
                || draftReturn.MaleMiddlePayBand != submittedReturn.MaleMiddlePayBand
                || draftReturn.FemaleMiddlePayBand != submittedReturn.FemaleMiddlePayBand
                || draftReturn.MaleUpperPayBand != submittedReturn.MaleUpperPayBand
                || draftReturn.FemaleUpperPayBand != submittedReturn.FemaleUpperPayBand
                || draftReturn.MaleUpperQuartilePayBand != submittedReturn.MaleUpperQuartilePayBand
                || draftReturn.FemaleUpperQuartilePayBand != submittedReturn.FemaleUpperQuartilePayBand;

            return isMaterialChange;
        }

    }
}
