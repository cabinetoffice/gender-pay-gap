using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using Moq;

namespace GenderPayGap.Tests.Common.TestHelpers
{
    public static class ReturnHelper
    {

        private static Return GetSubmittedReturnWithAllDataPoints(User user)
        {
            var result = Mock.Of<Return>(report => report.Status == ReturnStatuses.Submitted);

            result.ReturnId = new Random().Next(1000, 9999);

            result.DiffMeanBonusPercent = new Random().Next(1, 99);
            result.DiffMeanHourlyPayPercent = new Random().Next(1, 99);
            result.DiffMedianBonusPercent = new Random().Next(1, 99);
            result.DiffMedianHourlyPercent = new Random().Next(1, 99);

            GetRandomValues(0, 100, out int diffMaleLowerPayBand, out int diffFemaleLowerPayBand);
            result.FemaleLowerPayBand = diffFemaleLowerPayBand;
            result.MaleLowerPayBand = diffMaleLowerPayBand;

            GetRandomValues(1, 100, out int diffMaleMedianBonusPayPercent, out int diffFemaleMedianBonusPayPercent);
            result.FemaleMedianBonusPayPercent = diffFemaleMedianBonusPayPercent;
            result.MaleMedianBonusPayPercent = diffMaleMedianBonusPayPercent;

            GetRandomValues(1, 100, out int diffMaleMiddlePayBand, out int diffFemaleMiddlePayBand);
            result.FemaleMiddlePayBand = diffFemaleMiddlePayBand;
            result.MaleMiddlePayBand = diffMaleMiddlePayBand;

            GetRandomValues(1, 100, out int diffMaleUpperPayBand, out int diffFemaleUpperPayBand);
            result.FemaleUpperPayBand = diffFemaleUpperPayBand;
            result.MaleUpperPayBand = diffMaleUpperPayBand;

            GetRandomValues(1, 100, out int diffMaleUpperQuartilePayBand, out int diffFemaleUpperQuartilePayBand);
            result.FemaleUpperQuartilePayBand = diffFemaleUpperQuartilePayBand;
            result.MaleUpperQuartilePayBand = diffMaleUpperQuartilePayBand;

            int randomString = new Random().Next(10000, 99999);

            result.JobTitle = $"JobTitle_{randomString}";
            result.FirstName = $"Name_{randomString}";
            result.LastName = $"LastName_{randomString}";

            result.CompanyLinkToGPGInfo = string.Empty;

            return result;
        }

        public static Return GetSubmittedReturnForOrganisationAndYear(UserOrganisation userOrganisation, int snapshotYear)
        {
            Return result = GetSubmittedReturnWithAllDataPoints(userOrganisation.User);
            result.Organisation = userOrganisation.Organisation;
            result.OrganisationId = userOrganisation.Organisation.OrganisationId;
            result.AccountingDate = userOrganisation.Organisation.SectorType.GetAccountingStartDate(snapshotYear);
            return result;
        }

        /// <summary>
        ///     Creates a new Empty Return
        /// </summary>
        /// <param name="userOrganisation"></param>
        /// <param name="snapshotYear"></param>
        public static Return GetNewReturnForOrganisationAndYear(UserOrganisation userOrganisation, int snapshotYear)
        {
            var newReturn = new Return();
            newReturn.Organisation = userOrganisation.Organisation;
            newReturn.OrganisationId = userOrganisation.OrganisationId;
            newReturn.AccountingDate = userOrganisation.Organisation.SectorType.GetAccountingStartDate(snapshotYear);

            return newReturn;
        }

        private static void GetRandomValues(int min, int max, out int men, out int women)
        {
            men = new Random().Next(min, max);
            women = max - men;
        }

        public static Return CreateTestReturnWithNoBonus(Organisation organisation, int testYear = 2017)
        {
            return CreateBonusTestReturn(
                organisation,
                default,
                default,
                default,
                default,
                testYear);
        }

        public static Return CreateBonusTestReturn(Organisation organisation,
            decimal femaleMedianBonusPayPercent,
            decimal maleMedianBonusPayPercent,
            decimal diffMeanBonusPercent,
            decimal diffMedianBonusPercent,
            int testYear = 2017)
        {
            return new Return {
                ReturnId = organisation.OrganisationId + 100,
                Organisation = organisation,
                OrganisationId = organisation.OrganisationId,
                AccountingDate = organisation.SectorType.GetAccountingStartDate(testYear),
                Status = ReturnStatuses.Submitted,
                DiffMeanHourlyPayPercent = 99,
                DiffMedianHourlyPercent = 97,
                FemaleLowerPayBand = 96,
                FemaleMiddlePayBand = 94,
                FemaleUpperPayBand = 93,
                FemaleUpperQuartilePayBand = 92,
                MaleLowerPayBand = 91,
                MaleUpperQuartilePayBand = 89,
                MaleMiddlePayBand = 88,
                MaleUpperPayBand = 87,
                FirstName = $"Firstname{organisation.OrganisationId:000}",
                LastName = $"Lastname{organisation.OrganisationId:000}",
                JobTitle = $"Job title {organisation.OrganisationId:000}",
                CompanyLinkToGPGInfo = $"http://WebOrg{organisation.OrganisationId:000}",
                MinEmployees = 250,
                MaxEmployees = 499,

                /* fill bonus information */
                FemaleMedianBonusPayPercent = femaleMedianBonusPayPercent,
                MaleMedianBonusPayPercent = maleMedianBonusPayPercent,
                DiffMeanBonusPercent = diffMeanBonusPercent,
                DiffMedianBonusPercent = diffMedianBonusPercent
            };
        }

        public static Return CreateTestReturn(Organisation organisation, int testYear = 2017)
        {
            return CreateBonusTestReturn(
                organisation,
                95,
                90,
                100,
                98,
                testYear);
        }

        public static Return CreateLateReturn(Organisation organisation, DateTime snapshotDate, DateTime modifiedDate, OrganisationScope scope)
        {
            var lateReturn = new Return {
                Organisation = organisation,
                ReturnId = organisation.OrganisationId + 100,
                AccountingDate = snapshotDate,
                MinEmployees = 250,
                MaxEmployees = 499,
                Created = modifiedDate,
                Modified = modifiedDate
            };

            OrganisationHelper.LinkOrganisationAndReturn(organisation, lateReturn);
            OrganisationHelper.LinkOrganisationAndScope(organisation, scope, true);

            lateReturn.IsLateSubmission = lateReturn.CalculateIsLateSubmission();
            return lateReturn;
        }

        public static Return CreateLateReturn(int reportingYear, SectorTypes sector, ScopeStatuses scopeStatus, int modifiedDateOffset)
        {
            DateTime snapshotDate = sector.GetAccountingStartDate(reportingYear);
            DateTime nextSnapshotDate = snapshotDate.AddYears(1);
            DateTime modifiedDate = nextSnapshotDate.AddDays(modifiedDateOffset);

            Organisation testOrganisation = sector == SectorTypes.Private
                ? OrganisationHelper.GetPrivateOrganisation()
                : OrganisationHelper.GetPublicOrganisation();

            OrganisationScope testScope = ScopeHelper.CreateScope(scopeStatus, snapshotDate);
            
            return CreateLateReturn(testOrganisation, snapshotDate, modifiedDate, testScope);
        }

    }
}
