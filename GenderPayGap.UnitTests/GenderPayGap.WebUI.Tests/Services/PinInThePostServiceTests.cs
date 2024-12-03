using GenderPayGap.Database;
using GenderPayGap.WebUI.Services;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Tests.Services
{
    [TestFixture]
    public class PinInThePostServiceTests
    {

        private void AssertListEqual(List<string> listOne, List<string> listTwo)
        {
            Assert.AreEqual(
                listOne.Count,
                listTwo.Count,
                $"Lists not equal:\nList 1: {JsonConvert.SerializeObject(listOne)}\nList 2: {JsonConvert.SerializeObject(listTwo)}\n");

            for (var i = 0; i < listOne.Count; i++)
            {
                Assert.AreEqual(
                    listOne[i],
                    listTwo[i],
                    $"Lists not equal:\nList 1: {JsonConvert.SerializeObject(listOne)}\nList 2: {JsonConvert.SerializeObject(listTwo)}\n");
            }
        }

        [Test]
        public void FiveLineAddressIsReducedToFourLines()
        {
            // Arrange
            var initialOrganisation = new Organisation {
                OrganisationName = "Softwire",
                OrganisationAddresses = new[] {
                    new OrganisationAddress {
                        PoBox = "",
                        Address1 = "Unit 110",
                        Address2 = "53-79 Highgate Road",
                        Address3 = "Kentish Town",
                        TownCity = "London",
                        County = "Greater London",
                        Country = ""
                    }
                }
            };

            // Act
            List<string> outputAddress = PinInThePostService.GetAddressInFourLineFormat(initialOrganisation);

            // Assert
            AssertListEqual(outputAddress, new List<string> {"Unit 110", "53-79 Highgate Road", "Kentish Town", "London, Greater London"});
        }

        [Test]
        public void FourLineAddressIsNotShortened()
        {
            // Arrange
            var initialOrganisation = new Organisation {
                OrganisationName = "Softwire",
                OrganisationAddresses = new[] {
                    new OrganisationAddress {
                        PoBox = "",
                        Address1 = "Unit 110",
                        Address2 = "53-79 Highgate Road",
                        Address3 = "Kentish Town",
                        TownCity = "London",
                        County = "",
                        Country = ""
                    }
                }
            };

            // Act
            List<string> outputAddress = PinInThePostService.GetAddressInFourLineFormat(initialOrganisation);

            // Assert
            AssertListEqual(outputAddress, new List<string> {"Unit 110", "53-79 Highgate Road", "Kentish Town", "London"});
        }


        [Test]
        public void LongLinesAreNotConcatenated()
        {
            // Arrange
            var initialOrganisation = new Organisation {
                OrganisationName = "Softwire",
                OrganisationAddresses = new[] {
                    new OrganisationAddress {
                        PoBox = "",
                        Address1 = "Unit 110",
                        Address2 = "53-79 Highgate Road",
                        Address3 = "Kentish Town",
                        TownCity = "London",
                        County =
                            "Municipality of Greater London", // TownCity + County > 35 characters, so should not be concatenated
                        Country = ""
                    }
                }
            };

            // Act
            List<string> outputAddress = PinInThePostService.GetAddressInFourLineFormat(initialOrganisation);

            // Assert
            AssertListEqual(
                outputAddress,
                new List<string> {"Unit 110", "53-79 Highgate Road", "Kentish Town, London", "Municipality of Greater London"});
        }

        [Test]
        public void ThreeLineAddressIsNotShortened()
        {
            // Arrange
            var initialOrganisation = new Organisation {
                OrganisationName = "Softwire",
                OrganisationAddresses = new[] {
                    new OrganisationAddress {
                        PoBox = "",
                        Address1 = "Unit 110",
                        Address2 = "53-79 Highgate Road",
                        Address3 = "",
                        TownCity = "London",
                        County = "",
                        Country = ""
                    }
                }
            };

            // Act
            List<string> outputAddress = PinInThePostService.GetAddressInFourLineFormat(initialOrganisation);

            // Assert
            AssertListEqual(outputAddress, new List<string> {"Unit 110", "53-79 Highgate Road", "London"});
        }

        [Test]
        public void UkCountryLineIsRemoved()
        {
            // Arrange
            var initialOrganisation = new Organisation {
                OrganisationName = "Softwire",
                OrganisationAddresses = new[] {
                    new OrganisationAddress {
                        PoBox = "",
                        Address1 = "Unit 110",
                        Address2 = "53-79 Highgate Road",
                        Address3 = "",
                        TownCity = "London",
                        County = "",
                        Country = "UK"
                    }
                }
            };

            // Act
            List<string> outputAddress = PinInThePostService.GetAddressInFourLineFormat(initialOrganisation);

            // Assert
            AssertListEqual(outputAddress, new List<string> {"Unit 110", "53-79 Highgate Road", "London"});
        }

        [Test]
        public void UnitedKingdomCountryLineIsRemoved()
        {
            // Arrange
            var initialOrganisation = new Organisation {
                OrganisationName = "Softwire",
                OrganisationAddresses = new[] {
                    new OrganisationAddress {
                        PoBox = "",
                        Address1 = "Unit 110",
                        Address2 = "53-79 Highgate Road",
                        Address3 = "",
                        TownCity = "London",
                        County = "",
                        Country = "United Kingdom"
                    }
                }
            };

            // Act
            List<string> outputAddress = PinInThePostService.GetAddressInFourLineFormat(initialOrganisation);

            // Assert
            AssertListEqual(outputAddress, new List<string> {"Unit 110", "53-79 Highgate Road", "London"});
        }

    }
}
