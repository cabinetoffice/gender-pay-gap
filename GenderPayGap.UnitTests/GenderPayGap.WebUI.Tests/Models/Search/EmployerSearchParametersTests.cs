using GenderPayGap.WebUI.Models.Search;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Models.Search
{
    [TestFixture]
    public class EmployerSearchParametersTests
    {

        [TestCase("PALL MALL AND WOODCOTE PARK CLUBHOUSES LIMITED", "PALL MALL AND WOODCOTE PARK CLUBHOUSES")] // LIMITED
        [TestCase("Balfour Beatty PLC", "Balfour Beatty")] // PLC
        [TestCase("Ernst & Young LLP", "Ernst & Young")] // llp
        [TestCase("WWF-UK", "WWF")] // -uk
        [TestCase("1ST HOME CARE LTD.", "1ST HOME CARE")] // ltd.
        [TestCase("Autism Initiatives (UK)", "Autism Initiatives")] // (uk)
        [TestCase("Wolseley uk", "Wolseley")] // uk
        [TestCase("", "")] // term is empty, nothing to remove
        [TestCase(null, null)] // term is null, nothing to remove
        [TestCase("Wolseley", "Wolseley")] // term does not change
        [TestCase("Wolseley uk limited", "Wolseley")] // multiple matching terms are replaced together
        [TestCase("uk limited", "uk limited")] // Do NOT replace if the result is going to be an empty string
        [TestCase("uk", "uk")] // Do NOT replace (user might to search by 'uk')
        public void EmployerSearchParameters_RemoveTheMostCommonTermsOnOurDatabaseFromTheKeywords(string keywordsToClean,
            string expectedOutcome)
        {
            // Arrange
            var employerSearchParameters = new EmployerSearchParameters {Keywords = keywordsToClean};

            // Act
            string actualOutcome = employerSearchParameters.RemoveTheMostCommonTermsOnOurDatabaseFromTheKeywords();

            // Assert
            Assert.AreEqual(expectedOutcome, actualOutcome);
        }

    }
}
