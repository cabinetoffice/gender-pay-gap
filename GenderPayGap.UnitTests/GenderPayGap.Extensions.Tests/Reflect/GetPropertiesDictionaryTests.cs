using System.Collections.Generic;
using NUnit.Framework;

namespace GenderPayGap.Extensions.Tests.Reflect
{

    [TestFixture]
    public class GetPropertiesDictionaryTests
    {

        public class TestClass
        {

            private int PrivateField1 = 1352;

            private int PrivateField2 = 2531;

            public int PublicPropertyInt1 { get; } = 123;

            public int PublicPropertyInt2 { get; set; } = 456;

            public string PublicPropertyStr1 { get; set; } = "Str1";

            private int PrivateProperty1 { get; } = 987;

            private int PrivateProperty2 { get; } = 654;

        }

        [Test]
        public void ShouldCollectInstancePublicGettersOnlyByDefault()
        {
            var testObject = new TestClass();

            Dictionary<string, object> actual = testObject.GetPropertiesDictionary();

            Assert.AreEqual(3, actual.Count, "Expected 2 items.");

            // assert public getters property names exist
            Assert.IsTrue(
                actual.ContainsKey(nameof(TestClass.PublicPropertyInt1)),
                $"Expected '{nameof(TestClass.PublicPropertyInt1)}' to exist.");
            Assert.IsTrue(
                actual.ContainsKey(nameof(TestClass.PublicPropertyInt2)),
                $"Expected '{nameof(TestClass.PublicPropertyInt2)}' to exist.");
            Assert.IsTrue(
                actual.ContainsKey(nameof(TestClass.PublicPropertyStr1)),
                $"Expected '{nameof(TestClass.PublicPropertyStr1)}' to exist.");

            // assert public getters property values match
            Assert.AreEqual(
                123,
                actual[nameof(TestClass.PublicPropertyInt1)],
                $"Expected '{nameof(TestClass.PublicPropertyInt1)}' value to match.");
            Assert.AreEqual(
                456,
                actual[nameof(TestClass.PublicPropertyInt2)],
                $"Expected '{nameof(TestClass.PublicPropertyInt2)}' value to match.");
            Assert.AreEqual(
                "Str1",
                actual[nameof(TestClass.PublicPropertyStr1)],
                $"Expected '{nameof(TestClass.PublicPropertyStr1)}' value to match.");
        }

        [Test]
        public void ShouldIgnoreNullEntries()
        {
            var testObject = new TestClass {PublicPropertyStr1 = null};

            Dictionary<string, object> actual = testObject.GetPropertiesDictionary();

            Assert.AreEqual(2, actual.Count, "Expected 2 items.");

            Assert.IsFalse(
                actual.ContainsKey(nameof(TestClass.PublicPropertyStr1)),
                $"Expected '{nameof(TestClass.PublicPropertyStr1)}' to be excluded.");
        }

    }

}
