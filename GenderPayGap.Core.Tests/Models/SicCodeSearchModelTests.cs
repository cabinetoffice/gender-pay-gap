using GenderPayGap.Core.Models;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.Models
{
    /// <summary>
    ///     Tests in this class have been extracted from the examples here:
    ///     http://www.loganfranken.com/blog/687/overriding-equals-in-c-part-1/
    /// </summary>
    [TestFixture]
    public class SicCodeSearchModelTests
    {

        [SetUp]
        public void BeforeEach()
        {
            _testSicCodeSearchModel = new SicCodeSearchModel();
        }

        private SicCodeSearchModel _testSicCodeSearchModel;

        [TestCase(null, null, "-")]
        [TestCase(null, "", "-")]
        [TestCase("", null, "-")]
        [TestCase("", "", "-")]
        [TestCase("  ", "  ", "-")]
        [TestCase("01120", "", "01120-")]
        [TestCase("", "Growing of rice", "-Growing")]
        [TestCase("54120", "Manufacturing of engines", "54120-Manufac")]
        [TestCase("12", "x", "12-x")] // short string
        [TestCase("    9874", "ar", "9874-ar")]
        [TestCase("    9874", "   ar", "9874-ar")]
        [TestCase("9874", "   ar", "9874-ar")]
        [TestCase("13  ", "Ax", "13-Ax")]
        [TestCase("13  ", "Ax  ", "13-Ax")]
        [TestCase("13", "Ax  ", "13-Ax")]
        [TestCase("  155  ", "Boo", "155-Boo")]
        [TestCase("  155  ", "  Boo  ", "155-Boo")]
        [TestCase("155", "  Boo  ", "155-Boo")]
        public void SicCodeSearchModel_To(string sicCodeId, string sicCodeDescription, string expectedLogFriendlyString)
        {
            // Arrange
            _testSicCodeSearchModel.SicCodeId = sicCodeId;
            _testSicCodeSearchModel.SicCodeDescription = sicCodeDescription;

            // Act
            string actualLogFriendlyString = _testSicCodeSearchModel.ToLogFriendlyString();

            // Assert
            Assert.AreEqual(expectedLogFriendlyString, actualLogFriendlyString);
        }

        [Test]
        public void SicCodeSearchModel_ConsolidatedSynonyms_Can_Be_Set_And_Get()
        {
            // Arrange
            var consolidatedSynonymsToSet = "first synonym, second synonym";
            var sicCodeSearchModel = new SicCodeSearchModel();

            // Act
            sicCodeSearchModel.ConsolidatedSynonyms = consolidatedSynonymsToSet;
            string actualDescription = sicCodeSearchModel.ConsolidatedSynonyms;

            // Assert
            Assert.AreEqual(consolidatedSynonymsToSet, actualDescription);
        }

        [Test]
        public void SicCodeSearchModel_Equals_operator_When_Both_Null()
        {
            SicCodeSearchModel sicCodeSearchModelE = null;
            SicCodeSearchModel sicCodeSearchModelF = null;

            Assert.True(sicCodeSearchModelE == sicCodeSearchModelF);
        }

        [Test]
        public void SicCodeSearchModel_Equals_operator_When_Left_Null()
        {
            SicCodeSearchModel sicCodeSearchModelG = null;
            var sicCodeSearchModelH = new SicCodeSearchModel();

            Assert.False(sicCodeSearchModelG == sicCodeSearchModelH);
        }

        [Test]
        public void SicCodeSearchModel_Equals_operator_When_Right_Null()
        {
            var sicCodeSearchModelJ = new SicCodeSearchModel();
            SicCodeSearchModel sicCodeSearchModelK = null;

            Assert.False(sicCodeSearchModelJ == sicCodeSearchModelK);
        }

        [Test]
        public void SicCodeSearchModel_Equals_Returns_False_When_Object_Is_Null()
        {
            var sicCodeSearchModelInstance = new SicCodeSearchModel {SicCodeId = "999", SicCodeDescription = "Some description"};
            var differentTypeObjectThatIsNull = (int?) null;

            Assert.False(sicCodeSearchModelInstance.Equals(differentTypeObjectThatIsNull));
        }

        [Test]
        public void SicCodeSearchModel_Equals_Returns_False_When_SicCodeIds_Are_Different()
        {
            var sicCodeSearchModelSameDescriptionLeft = new SicCodeSearchModel {SicCodeId = "4", SicCodeDescription = "Same description"};
            var sicCodeSearchModelSameDescriptionRight = new SicCodeSearchModel {SicCodeId = "6", SicCodeDescription = "Same description"};

            Assert.False(sicCodeSearchModelSameDescriptionLeft.Equals((object) sicCodeSearchModelSameDescriptionRight));
        }

        [Test]
        public void SicCodeSearchModel_Equals_Returns_False_When_Types_Are_Different()
        {
            var sicCodeSearchModelInstance = new SicCodeSearchModel {SicCodeId = "121", SicCodeDescription = "Some description"};
            var differentTypeObject = "IAmAnStringObject";

            Assert.False(sicCodeSearchModelInstance.Equals(differentTypeObject));
        }

        [Test]
        public void SicCodeSearchModel_Equals_Returns_True_When_Compared_With_Itself()
        {
            var sicCodeSearchModelInstance = new SicCodeSearchModel {SicCodeId = "999", SicCodeDescription = "Some description"};

            Assert.True(sicCodeSearchModelInstance.Equals((object) sicCodeSearchModelInstance));
        }

        [Test]
        public void SicCodeSearchModel_Equals_Returns_True_When_SicCodeIds_Are_Equal()
        {
            var sicCodeSearchModelLeft = new SicCodeSearchModel {SicCodeId = "425", SicCodeDescription = "Some description"};
            var sicCodeSearchModelRight = new SicCodeSearchModel {SicCodeId = "425", SicCodeDescription = "No description"};

            Assert.True(sicCodeSearchModelLeft.Equals((object) sicCodeSearchModelRight));
        }

        [Test]
        public void SicCodeSearchModel_Equals_Should_Check_For_Null()
        {
            var sicCodeSearchModelInstance = new SicCodeSearchModel {SicCodeId = "456", SicCodeDescription = "Some description"};
            SicCodeSearchModel nullSicCodeSearchModel = null;

            Assert.False(sicCodeSearchModelInstance == nullSicCodeSearchModel); // FALSE
            Assert.False(sicCodeSearchModelInstance.Equals(nullSicCodeSearchModel)); // FALSE

            // To avoid using == we'll use the reference equality method available in 'Object'
            Assert.False(ReferenceEquals(sicCodeSearchModelInstance, nullSicCodeSearchModel));
        }

        [Test]
        public void SicCodeSearchModel_GetHashCode_Is_Bigger_Than_Zero_When_SicCodeId_Not_Null()
        {
            // Arrange
            var testSicCode = "698";
            int expectedHashCode = testSicCode.GetHashCode();
            var sicCodeSearchModel = new SicCodeSearchModel {SicCodeId = testSicCode};

            // Act
            int actual = sicCodeSearchModel.GetHashCode();

            // Assert
            Assert.AreEqual(expectedHashCode, actual);
        }

        [Test]
        public void SicCodeSearchModel_GetHashCode_Is_Zero_When_SicCodeId_Is_Null()
        {
            // Arrange
            var sicCodeSearchModel = new SicCodeSearchModel {SicCodeId = null};

            // Act
            int actual = sicCodeSearchModel.GetHashCode();

            // Assert
            Assert.AreEqual(0, actual);
        }

        [Test]
        public void SicCodeSearchModel_NotEquals_operator_When_Both_Equal_Returns_False()
        {
            var sicCodeSearchModelP = new SicCodeSearchModel {SicCodeId = "4547"};
            var sicCodeSearchModelQ = new SicCodeSearchModel {SicCodeId = "4547"};

            Assert.False(sicCodeSearchModelP != sicCodeSearchModelQ);
        }

        [Test]
        public void SicCodeSearchModel_Reference_Equality_Different_Instances()
        {
            var sicCodeSearchModelC = new SicCodeSearchModel {SicCodeId = "123", SicCodeDescription = "Some description 1"};
            var sicCodeSearchModelD = new SicCodeSearchModel {SicCodeId = "123", SicCodeDescription = "Some description 2"};

            Assert.False(
                ReferenceEquals(sicCodeSearchModelC, sicCodeSearchModelD),
                "These two objects should have been different instances (their object references should have been different)");

            // Equality checks fail because these are two different objects
            // But these are the *same* SicCodeSearchModels as far as we're concerned
            Assert.True(sicCodeSearchModelC == sicCodeSearchModelD);
            Assert.True(sicCodeSearchModelC.Equals(sicCodeSearchModelD));
        }

        [Test]
        public void SicCodeSearchModel_Reference_Equality_Same_Instance()
        {
            SicCodeSearchModel sicCodeSearchModelA = _testSicCodeSearchModel;
            SicCodeSearchModel sicCodeSearchModelB = sicCodeSearchModelA;

            // Well, of course, it's the same exact object
            Assert.True(sicCodeSearchModelA == sicCodeSearchModelB); // TRUE
            Assert.True(sicCodeSearchModelA.Equals(sicCodeSearchModelB)); // TRUE

            // To avoid using == we'll use the reference equality method available in 'Object'
            Assert.True(ReferenceEquals(sicCodeSearchModelA, sicCodeSearchModelB));
        }

        [Test]
        public void SicCodeSearchModel_SicCodeDescription_Can_Be_Set_And_Get()
        {
            // Arrange
            var descriptionToSet = "description to set";
            var sicCodeSearchModel = new SicCodeSearchModel();

            // Act
            sicCodeSearchModel.SicCodeDescription = descriptionToSet;
            string actualDescription = sicCodeSearchModel.SicCodeDescription;

            // Assert
            Assert.AreEqual(descriptionToSet, actualDescription);
        }

        [Test]
        public void SicCodeSearchModel_SicCodeListOfSynonyms_Can_Be_Set_And_Get()
        {
            // Arrange
            var consolidatedSynonymsToSet = "first synonym, second synonym";
            var sicCodeSearchModel = new SicCodeSearchModel();

            // Act
            sicCodeSearchModel.ConsolidatedSynonyms = consolidatedSynonymsToSet;
            string[] actualListOfSynonyms = sicCodeSearchModel.SicCodeListOfSynonyms;

            // Assert
            Assert.AreEqual("first synonym", actualListOfSynonyms[0]);
            Assert.AreEqual(" second synonym", actualListOfSynonyms[1]);
        }

    }
}
