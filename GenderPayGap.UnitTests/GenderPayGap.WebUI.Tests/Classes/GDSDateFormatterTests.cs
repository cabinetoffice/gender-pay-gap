using System;
using GenderPayGap.WebUI.Classes.Formatters;
using NUnit.Framework;

namespace GenderPayGap.Tests
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class GDSDateFormatterTests : AssertionHelper
    {

        [Test]
        [Description("ReportingInfoModel: Computes date format according to GDS format")]
        public void Computes_date_format_according_to_GDS_format()
        {
            var testFmtr = new GDSDateFormatter(new DateTime(2017, 4, 5));

            // Assert Start Period
            Expect(testFmtr.StartDay == 5);
            Expect(testFmtr.Month == "April");
            Expect(testFmtr.StartYear == 2017);

            // Assert End Period
            Expect(testFmtr.EndDay == 4);
            Expect(testFmtr.Month == "April");
            Expect(testFmtr.EndYear == 2018);

            // Assert Text Formatting
            Expect(testFmtr.FullStartDate == "5 April 2017");
            Expect(testFmtr.FullEndDate == "4 April 2018");
            Expect(testFmtr.FullYearRange == "2017 to 2018");
            Expect(testFmtr.FullDateRange == "5 April 2017 to 4 April 2018");

            testFmtr = new GDSDateFormatter(new DateTime(2016, 4, 5));

            // Assert Start Period
            Expect(testFmtr.StartDay == 5);
            Expect(testFmtr.Month == "April");
            Expect(testFmtr.StartYear == 2016);

            // Assert End Period
            Expect(testFmtr.EndDay == 4);
            Expect(testFmtr.Month == "April");
            Expect(testFmtr.EndYear == 2017);

            // Assert Text Formatting
            Expect(testFmtr.FullStartDate == "5 April 2016");
            Expect(testFmtr.FullEndDate == "4 April 2017");
            Expect(testFmtr.FullYearRange == "2016 to 2017");
            Expect(testFmtr.FullDateRange == "5 April 2016 to 4 April 2017");
        }

    }

}
