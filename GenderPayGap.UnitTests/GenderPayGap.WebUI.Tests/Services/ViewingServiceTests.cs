using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Models.Search;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Services
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class ViewingServiceTests
    {

        private Mock<ICommonBusinessLogic> _mockCommonLogic;
        private Mock<IDataRepository> _mockDataRepo;
        private Mock<ISearchRepository<EmployerSearchModel>> _mockSearchRepo;
        private Mock<ISearchRepository<SicCodeSearchModel>> _mockSearchSicCodeRepo;

        [SetUp]
        public void BeforeEach()
        {
            _mockDataRepo = MoqHelpers.CreateMockAsyncDataRepository();
            _mockSearchRepo = new Mock<ISearchRepository<EmployerSearchModel>>();
            _mockSearchSicCodeRepo = new Mock<ISearchRepository<SicCodeSearchModel>>();
            _mockCommonLogic = new Mock<ICommonBusinessLogic>();
        }

        [Test]
        [Description("GetOrgSizeOptions: Marks given org size indexes as checked or not checked")]
        public void GetOrgSizeOptions_Marks_given_org_size_indexes_as_checked_or_not_checked()
        {
            var testOptions = new[] {
                OrganisationSizes.Employees0To249, OrganisationSizes.Employees1000To4999, OrganisationSizes.Employees5000to19999
            };

            // Mocks
            var testService = new ViewingService(
                _mockDataRepo.Object,
                _mockSearchRepo.Object,
                _mockSearchSicCodeRepo.Object,
                _mockCommonLogic.Object);
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
                _mockSearchRepo.Object,
                _mockSearchSicCodeRepo.Object,
                _mockCommonLogic.Object);
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
        [Description("GetReportingYearOptions: Marks given years as checked or not checked")]
        public void GetReportingYearOptions_Marks_given_years_as_checked_or_not_checked()
        {
            // Setup
            var testAllYears = new[] {2020, 2019, 2018, 2017};

            var testCheckedYears = new[] {2018, 2017};

            // Mocks
            _mockCommonLogic.Setup(x => x.GetAccountingStartDate(It.IsAny<SectorTypes>(), 0))
                .Returns(new DateTime(testAllYears[0], 4, 5));

            var testService = new ViewingService(_mockDataRepo.Object, _mockSearchRepo.Object, _mockCommonLogic.Object);
            List<OptionSelect> options = testService.GetReportingYearOptions(testCheckedYears);

            // Assert
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
                Assert.That(opt.Disabled == false);
                Assert.That(opt.Id == testAllYears[i].ToString());
                Assert.That(opt.Label == $"{testAllYears[i]} to {testAllYears[i] + 1}");
                Assert.That(opt.Value == testAllYears[i].ToString());
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
                (int) SearchReportingStatusFilter.ExplanationProvidedByEmployer
            };

            var testCheckedOptions = new[] {
                (int) SearchReportingStatusFilter.ReportedInTheLast7Days, (int) SearchReportingStatusFilter.ReportedLate
            };

            var testService = new ViewingService(_mockDataRepo.Object, _mockSearchRepo.Object, _mockCommonLogic.Object);
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

        [Test]
        [Description("GetPagedResult: Returns paged results")]
        public void GetPagedResult_Returns_paged_results()
        {
            // Setup
            long totalRecords = 100;

            // Mocks
            var testService = new ViewingService(
                _mockDataRepo.Object,
                _mockSearchRepo.Object,
                _mockSearchSicCodeRepo.Object,
                _mockCommonLogic.Object);

            var testResults = new List<EmployerSearchModel>();
            for (var i = 0; i < totalRecords; i++)
            {
                testResults.Add(new EmployerSearchModel());
            }

            // Test
            PagedResult<EmployerSearchModel> resultModel = testService.GetPagedResult(testResults, totalRecords, 1, 20);
            Assert.That(resultModel.CurrentPage == 1);
            Assert.That(resultModel.PageCount == 5);
            Assert.That(resultModel.PageSize == 20);
            Assert.That(resultModel.ActualRecordTotal == totalRecords);
            Assert.That(resultModel.Results != null);
            Assert.That(resultModel.Results.Count == totalRecords);

            resultModel = testService.GetPagedResult(testResults, totalRecords, 2, 20);
            Assert.That(resultModel.CurrentPage == 2);
            Assert.That(resultModel.PageCount == 5);
            Assert.That(resultModel.PageSize == 20);
            Assert.That(resultModel.ActualRecordTotal == totalRecords);
            Assert.That(resultModel.Results != null);
            Assert.That(resultModel.Results.Count == totalRecords);

            resultModel = testService.GetPagedResult(testResults, totalRecords, 5, 20);
            Assert.That(resultModel.CurrentPage == 5);
            Assert.That(resultModel.PageCount == 5);
            Assert.That(resultModel.PageSize == 20);
            Assert.That(resultModel.ActualRecordTotal == totalRecords);
            Assert.That(resultModel.Results != null);
            Assert.That(resultModel.Results.Count == totalRecords);
        }

        [Test]
        [Description("SuggestEmployerName: Returns encrypted org id and matched text")]
        public async Task SuggestEmployerName_Returns_encryptedid_and_matched_text()
        {
            // Setup
            var testRecords = new List<KeyValuePair<string, EmployerSearchModel>>();

            for (var i = 0; i < 10; i++)
            {
                testRecords.Add(
                    new KeyValuePair<string, EmployerSearchModel>(
                        i.ToString(),
                        new EmployerSearchModel {OrganisationIdEncrypted = i.ToString(), Name = $"Org{i.ToString()}"}));
            }

            // Mocks
            _mockSearchRepo.Setup(sr => sr.SuggestAsync(It.IsAny<string>(), It.IsAny<string>(), null, true, 10))
                .ReturnsAsync(testRecords);

            var testService = new ViewingService(
                _mockDataRepo.Object,
                _mockSearchRepo.Object,
                _mockSearchSicCodeRepo.Object,
                _mockCommonLogic.Object);

            // Test
            List<SuggestOrganisationResult> assertResults = await testService.SuggestEmployerNameAsync("testing 123");

            Assert.That(assertResults.Count == testRecords.Count);
            for (var i = 0; i < assertResults.Count; i++)
            {
                SuggestOrganisationResult result = assertResults[i];
                Assert.That(result.Id == i.ToString());
                Assert.That(result.Text == $"Org{i.ToString()}");
            }
        }

    }
}
