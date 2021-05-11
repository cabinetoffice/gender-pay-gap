using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Models.Compare;

namespace GenderPayGap.WebUI.Tests.TestHelpers
{
    public static class ViewingServiceHelper
    {

        public static IEnumerable<CompareReportModel> GetCompareTestData(int organisationTotal)
        {
            var results = new List<CompareReportModel>();

            for (var i = 1; i <= organisationTotal; i++)
            {
                var returnViewModel = new CompareReportModel {
                    OrganisationName = "Org" + i,
                    OrganisationSicCodes = "",
                    EncOrganisationId = "X{i:000}",
                    OrganisationSize = (OrganisationSizes) Numeric.Rand(0, 6),
                    DiffMeanHourlyPayPercent = Numeric.Rand(0, 100),
                    DiffMedianHourlyPercent = Numeric.Rand(0, 100),
                    FemaleLowerPayBand = Numeric.Rand(0, 100),
                    FemaleMiddlePayBand = Numeric.Rand(0, 100),
                    FemaleUpperPayBand = Numeric.Rand(0, 100),
                    FemaleUpperQuartilePayBand = Numeric.Rand(0, 100),
                    FemaleMedianBonusPayPercent = Numeric.Rand(0, 100),
                    MaleMedianBonusPayPercent = Numeric.Rand(0, 100),
                    DiffMeanBonusPercent = Numeric.Rand(0, 100),
                    DiffMedianBonusPercent = Numeric.Rand(0, 100)
                };

                results.Add(returnViewModel);
            }

            return results;
        }

    }
}
