using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Search;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Services
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class ViewingServiceTests
    {

        private Mock<IDataRepository> _mockDataRepo;
        private ViewingSearchService viewingSearchService;
        
        [SetUp]
        public void BeforeEach()
        {
            _mockDataRepo = MoqHelpers.CreateMockAsyncDataRepository();
            viewingSearchService = new ViewingSearchService(_mockDataRepo.Object);
        }

        [Test]
        [Description("GetOrgSizeOptions: Marks given org size indexes as checked or not checked")]
        public void GetOrgSizeOptions_Marks_given_org_size_indexes_as_checked_or_not_checked()
        {
            var testOptions = new[] {
                OrganisationSizes.Employees0To249, OrganisationSizes.Employees1000To4999, OrganisationSizes.Employees5000To19999
            };

            // Mocks
            var testService = new ViewingService(
                _mockDataRepo.Object,
                viewingSearchService);
            List<OptionSelect> options = testService.GetOrgSizeOptions(testOptions.Select(x => (int) x), null);

            // Assert
            for (var i = 0; i < options.Count; i++)
            {
                OptionSelect opt = options[i];

                // assert checked
                if (testOptions.Any(x => (int) x == i))
                {
                    Assert.That(opt.Checked);
                }
                else
                {
                    Assert.That(opt.Checked == false);
                }

                var size = (OrganisationSizes) Enum.Parse(typeof(OrganisationSizes), opt.Value);
                Assert.That(opt.Disabled == false);
                Assert.That(opt.Id == $"Size{i}");
                Assert.That(opt.Label == size.GetAttribute<DisplayAttribute>().Name);
                Assert.That(opt.Value == i.ToString());
            }
        }

        [Test]
        [Description("GetSectorOptions: Marks given sector chars as checked or not checked")]
        public async Task GetSectorOptions_Marks_given_sector_chars_as_checked_or_not_checked()
        {
            // Setup
            var testSicSections = new[] {
                new SicSection {SicSectionId = "C", Description = "C Sector"},
                new SicSection {SicSectionId = "D", Description = "D Sector"},
                new SicSection {SicSectionId = "L", Description = "L Sector"},
                new SicSection {SicSectionId = "O", Description = "O Sector"},
                new SicSection {SicSectionId = "R", Description = "R Sector"},
                new SicSection {SicSectionId = "Y", Description = "Y Sector"}
            };

            var testOptions = new[] {'D', 'L', 'O'};

            // Mocks
            _mockDataRepo.Setup(x => x.GetAll<SicSection>())
                .Returns(new List<SicSection>(testSicSections).AsQueryable().BuildMock().Object);

            var testService = new ViewingService(
                _mockDataRepo.Object,
                viewingSearchService);
            List<OptionSelect> options = await testService.GetSectorOptionsAsync(testOptions, null);

            // Assert
            for (var i = 0; i < options.Count; i++)
            {
                OptionSelect opt = options[i];

                // assert checked
                if (testOptions.Any(x => x == opt.Value[0]))
                {
                    Assert.That(opt.Checked);
                }
                else
                {
                    Assert.That(opt.Checked == false);
                }

                Assert.That(opt.Disabled == false);
                Assert.That(opt.Id == testSicSections[i].SicSectionId);
                Assert.That(opt.Label == $"{testSicSections[i].SicSectionId} Sector");
                Assert.That(opt.Value == testSicSections[i].SicSectionId);
            }
        }

        [Test]
        [Ignore("This breaks in late-June and/or 1st-January each year because the date calculation in the test is wrong")]
        [Description("GetReportingYearOptions: Marks given years as checked or not checked")]
        public void GetReportingYearOptions_Marks_given_years_as_checked_or_not_checked()
        {
            // Setup
            var testCheckedYears = new[] {2018, 2017};

            // Mocks
            var testService = new ViewingService(
                _mockDataRepo.Object,
                viewingSearchService);
            List<OptionSelect> options = testService.GetReportingYearOptions(testCheckedYears);

            // Assert
            int firstYear = 2017;
            int nowYear = VirtualDateTime.Now.Year;
            List<int> testAllYears = Enumerable.Range(firstYear, nowYear - firstYear + 1).OrderByDescending(y => y).ToList();

            for (var i = 0; i < options.Count; i++)
            {
                OptionSelect opt = options[i];

                // assert checked status
                if (testCheckedYears.Any(x => x.ToString() == opt.Value))
                {
                    Assert.That(opt.Checked);
                }
                else
                {
                    Assert.That(opt.Checked == false);
                }

                // assert all other meta values
                Assert.AreEqual(false, opt.Disabled);
                Assert.AreEqual(testAllYears[i].ToString(), opt.Id);
                Assert.AreEqual($"{testAllYears[i]} to {testAllYears[i] + 1}", opt.Label);
                Assert.AreEqual(testAllYears[i].ToString(), opt.Value);
            }
        }

        [Test]
        [Description("GetReportingStatusOptions: Marks given status as checked or not checked")]
        public void GetReportingStatusOptions_Marks_given_status_as_checked_or_not_checked()
        {
            // Setup
            var testAllOptions = new[] {
                (int) SearchReportingStatusFilter.ReportedInTheLast7Days,
                (int) SearchReportingStatusFilter.ReportedInTheLast30Days,
                (int) SearchReportingStatusFilter.ReportedLate,
                (int) SearchReportingStatusFilter.ReportedWithCompanyLinkToGpgInfo
            };

            var testCheckedOptions = new[] {
                (int) SearchReportingStatusFilter.ReportedInTheLast7Days, (int) SearchReportingStatusFilter.ReportedLate
            };

            var testService = new ViewingService(
                _mockDataRepo.Object,
                viewingSearchService);
            List<OptionSelect> options = testService.GetReportingStatusOptions(testCheckedOptions);

            // Assert
            for (var i = 0; i < options.Count; i++)
            {
                OptionSelect opt = options[i];

                // assert checked status
                if (testCheckedOptions.Any(x => x.ToString() == opt.Value))
                {
                    Assert.That(opt.Checked);
                }
                else
                {
                    Assert.That(opt.Checked == false);
                }

                // assert all other meta
                Assert.That(opt.Disabled == false);
                Assert.That(opt.Id == $"ReportingStatus{testAllOptions[i]}");
                Assert.That(opt.Label == ((SearchReportingStatusFilter) testAllOptions[i]).GetAttribute<DisplayAttribute>().Name);
                Assert.That(opt.Value == testAllOptions[i].ToString());
            }
        }

        [Test]
        [Description("Search: Expands search and filter to azure query")]
        public void Search_Expands_search_and_filter_to_azure_query()
        {
            // Setup
            string expectedLatestReportedDate = VirtualDateTime.UtcNow.Date.AddDays(-7).ToString("O");
            string expectedFilterQuery = string.Join(
                " and ",
                "SicSectionIds/any(id: id eq 'G' or id eq 'L' or id eq 'C')",
                "(Size eq 1 or Size eq 2 or Size eq 3)",
                "ReportedYears/any(ReportedYear: ReportedYear eq '2017')",
                $"(LatestReportedDate gt {expectedLatestReportedDate} or ReportedExplanationYears/any(ReportedYear: ReportedYear eq '2017'))");

            var testParams = new EmployerSearchParameters {
                Keywords = "word1 word2 word3",
                FilterEmployerSizes = new[] {1, 2, 3},
                FilterSicSectionIds = new[] {'G', 'L', 'C'},
                FilterReportedYears = new[] {2017},
                FilterReportingStatus = new[] {0, 3},
                SearchType = SearchType.ByEmployerName
            };

            // Test
            string resultFilterQuery = testParams.ToFilterQuery();
            Assert.AreEqual(expectedFilterQuery, resultFilterQuery);
        }
        
    }
}
