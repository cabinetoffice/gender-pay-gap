using System;
using System.Text.RegularExpressions;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class EhrcController : Controller
    {
        private readonly IDataRepository dataRepository;

        public EhrcController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [Route("/download")]
        public IActionResult EhrcAllOrganisationsForYear_EhrcIpProtectedLink(
            string p
            /* The EHRC are given URLs of the form:
               https://gender-pay-gap.service.gov.uk/download?p=App_Data\Downloads\GPG-Organisations_2017-18.csv
               https://gender-pay-gap.service.gov.uk/download?p=App_Data\Downloads\GPG-Organisations-Without-Reports_2017-18.csv
               So this query parameter 'p' is of the form
               App_Data\Downloads\GPG-Organisations_2017-18.csv

               Previously, we would use 'p' to go and find a real file (in the App_Data\Downloads folder).
               But now, we generate the file on-the-fly from the database.
               So now all we need to do is check that 'p' is the filename of one of the files we support
               and extract the year from the URL
            */
        )
        {
            var organisationsYear = ValidatePath(
                p,
                "GPG-Organisations",
                Global.FirstReportingYear);

            if (organisationsYear != null)
            {
                return AdminDownloadsController.GenerateEhrcAllOrganisationsForYearFile(dataRepository, (int)organisationsYear);
            }

            var OrganisationsWithoutReportsYear = ValidatePath(
                p,
                "GPG-Organisations-Without-Reports",
                Global.FirstReportingYear);

            if (OrganisationsWithoutReportsYear != null)
            {
                return AdminDownloadsController.GenerateOrganisationsWithNoSubmittedReturnsForYear(dataRepository, (int)OrganisationsWithoutReportsYear, true);
            }

            return NotFound();
        }

        private int? ValidatePath(string p, string fileName, int minYear)
        {
            Match match = Regex.Match(p, $@"^App_Data\\Downloads\\{fileName}_(?<year4digits>\d\d\d\d)-(?<year2digits>\d\d)\.csv$");
            if (match.Success)
            {
                string year4digitsString = match.Groups["year4digits"].Value;
                string year2digitsString = match.Groups["year2digits"].Value;

                if (int.TryParse(year4digitsString, out int year4digits)
                    && int.TryParse(year2digitsString, out int year2digits)
                    && (year4digits + 1) % 100 == year2digits
                    && year4digits >= minYear)
                {
                    return year4digits;
                }
            }
            return null;
        }
    }
}
