using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    [Serializable]
    public partial class Return
    {

        [NotMapped]
        public string ResponsiblePerson
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LastName))
                {
                    return null;
                }

                return $"{FirstName} {LastName} ({JobTitle})";
            }
        }

        [NotMapped]
        public OrganisationSizes OrganisationSize
        {
            get
            {
                OrganisationSizes orgSize;

                switch (MaxEmployees)
                {
                    case 249:
                        orgSize = OrganisationSizes.Employees0To249;
                        break;
                    case 499:
                        orgSize = OrganisationSizes.Employees250To499;
                        break;
                    case 999:
                        orgSize = OrganisationSizes.Employees500To999;
                        break;
                    case 4999:
                        orgSize = OrganisationSizes.Employees1000To4999;
                        break;
                    case 19999:
                        orgSize = OrganisationSizes.Employees5000To19999;
                        break;
                    case int.MaxValue:
                        orgSize = OrganisationSizes.Employees20000OrMore;
                        break;
                    default:
                        orgSize = OrganisationSizes.NotProvided;
                        break;
                }

                return orgSize;
            }
        }
        
        [NotMapped]
        public ReportStatusBadgeType GetBadgeType
        {
            get
            {
                ReportStatusBadgeType reportStatus = ReportStatusBadgeType.NotRequired;

                if (IsVoluntarySubmission())
                {
                    reportStatus = ReportStatusBadgeType.VoluntarilyReported;
                }
                else if (IsLateSubmission && ReturnId != 0)
                {
                    reportStatus = ReportStatusBadgeType.SubmittedLate;
                }
                else if (IsLateSubmission)
                {
                    reportStatus = ReportStatusBadgeType.Overdue;
                }
                else if (ReturnId != 0)
                {
                    reportStatus = ReportStatusBadgeType.Reported;
                }
                else if (Global
                    .ReportingStartYearsToExcludeFromLateFlagEnforcement
                    .Contains(AccountingDate.Year)
                    && AccountingDate.Year == 2019)
                {
                    reportStatus = ReportStatusBadgeType.NotRequiredDueToCovid;
                }
                return reportStatus;
            }
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var target = (Return) obj;
            return ReturnId == target.ReturnId;
        }

        public override int GetHashCode()
        {
            return ReturnId.GetHashCode();
        }

        public bool IsRequired()
        {
            return OrganisationSize != OrganisationSizes.Employees0To249
                && GetScopeStatus().IsAny(ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)
                && !Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(AccountingDate.Year);
        }

        public bool HasBonusesPaid()
        {
            return FemaleMedianBonusPayPercent != default(long)
                   || MaleMedianBonusPayPercent != default(long)
                   || DiffMeanBonusPercent != default(long)
                   || DiffMedianBonusPercent != default(long);
        }

        #region Methods

        public ScopeStatuses GetScopeStatus()
        {
            return Organisation.GetScopeStatusForYear(AccountingDate.Year);
        }

        public bool CalculateIsLateSubmission()
        {
            return Modified > GetDueDate()
                   && OrganisationSize != OrganisationSizes.Employees0To249
                   && GetScopeStatus().IsAny(ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)
                   && !Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(AccountingDate.Year);
        }

        public bool IsVoluntarySubmission()
        {
            return ReturnId > 0
                   && OrganisationSize.IsAny(OrganisationSizes.Employees0To249)
                   && GetScopeStatus().IsAny(ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope);
        }

        public void SetStatus(ReturnStatuses status, long byUserId, string details = null)
        {
            if (status == Status && details == StatusDetails)
            {
                return;
            }

            Status = status;
            StatusDate = VirtualDateTime.Now;
            StatusDetails = details;
        }

        public bool Equals(Return model)
        {
            if (AccountingDate != model.AccountingDate)
            {
                return false;
            }

            if (CompanyLinkToGPGInfo != model.CompanyLinkToGPGInfo)
            {
                return false;
            }

            if (DiffMeanBonusPercent != model.DiffMeanBonusPercent)
            {
                return false;
            }

            if (DiffMeanHourlyPayPercent != model.DiffMeanHourlyPayPercent)
            {
                return false;
            }

            if (DiffMedianBonusPercent != model.DiffMedianBonusPercent)
            {
                return false;
            }

            if (DiffMedianHourlyPercent != model.DiffMedianBonusPercent)
            {
                return false;
            }

            if (FemaleLowerPayBand != model.FemaleLowerPayBand)
            {
                return false;
            }

            if (FemaleMedianBonusPayPercent != model.FemaleMedianBonusPayPercent)
            {
                return false;
            }

            if (FemaleMiddlePayBand != model.FemaleMiddlePayBand)
            {
                return false;
            }

            if (FemaleUpperPayBand != model.FemaleUpperPayBand)
            {
                return false;
            }

            if (FemaleUpperQuartilePayBand != model.FemaleUpperQuartilePayBand)
            {
                return false;
            }

            if (FirstName != model.FirstName)
            {
                return false;
            }

            if (LastName != model.LastName)
            {
                return false;
            }

            if (JobTitle != model.JobTitle)
            {
                return false;
            }

            if (MaleLowerPayBand != model.MaleLowerPayBand)
            {
                return false;
            }

            if (MaleMedianBonusPayPercent != model.MaleMedianBonusPayPercent)
            {
                return false;
            }

            if (MaleUpperQuartilePayBand != model.MaleUpperQuartilePayBand)
            {
                return false;
            }

            if (MaleMiddlePayBand != model.MaleMiddlePayBand)
            {
                return false;
            }

            if (MaleUpperPayBand != model.MaleUpperPayBand)
            {
                return false;
            }

            if (OrganisationId != model.OrganisationId)
            {
                return false;
            }

            if (MinEmployees != model.MinEmployees)
            {
                return false;
            }

            if (MaxEmployees != model.MaxEmployees)
            {
                return false;
            }

            return true;
        }

        public DownloadResult ToDownloadResult()
        {
            return new DownloadResult {
                EmployerName = Organisation?.GetName(StatusDate)?.Name ?? Organisation.OrganisationName,
                EmployerId = OrganisationId,
                Address = Organisation.GetLatestAddress()?.GetAddressString(),
                PostCode = Organisation.GetLatestAddress()?.GetPostCodeInAllCaps(),
                CompanyNumber = Organisation?.CompanyNumber,
                SicCodes = Organisation?.GetSicCodeIdsString(StatusDate, "," + Environment.NewLine),
                DiffMeanHourlyPercent = DiffMeanHourlyPayPercent,
                DiffMedianHourlyPercent = DiffMedianHourlyPercent,
                DiffMeanBonusPercent = DiffMeanBonusPercent,
                DiffMedianBonusPercent = DiffMedianBonusPercent,
                MaleBonusPercent = MaleMedianBonusPayPercent,
                FemaleBonusPercent = FemaleMedianBonusPayPercent,
                MaleLowerQuartile = MaleLowerPayBand,
                FemaleLowerQuartile = FemaleLowerPayBand,
                MaleLowerMiddleQuartile = MaleMiddlePayBand,
                FemaleLowerMiddleQuartile = FemaleMiddlePayBand,
                MaleUpperMiddleQuartile = MaleUpperPayBand,
                FemaleUpperMiddleQuartile = FemaleUpperPayBand,
                MaleTopQuartile = MaleUpperQuartilePayBand,
                FemaleTopQuartile = FemaleUpperQuartilePayBand,
                CompanyLinkToGPGInfo = CompanyLinkToGPGInfo,
                ResponsiblePerson = ResponsiblePerson,
                EmployerSize = OrganisationSize.GetAttribute<DisplayAttribute>().Name,
                CurrentName = Organisation?.OrganisationName,
                SubmittedAfterTheDeadline = IsLateSubmission,
                DueDate = GetDueDate(),
                DateSubmitted = Modified
            };
        }

        public string GetReportingPeriod()
        {
            return ReportingYearsHelper.FormatYearAsReportingPeriod(AccountingDate.Year, "/");
        }

        // The deadline date is the final day that a return can be submitted without being considered late
        // The due date is a day later, the point at which a return is considered late
        // i.e. if the deadline date is 2021/04/01, submissions on that day are not late, any after 2021/04/02 00:00:00 are
        private DateTime GetDueDate()
        {
            return ReportingYearsHelper.GetDeadlineForAccountingDate(AccountingDate).AddDays(1);
        }

        #endregion

    }
}
