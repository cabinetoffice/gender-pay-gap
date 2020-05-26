using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using NUnit.Framework;

namespace GenderPayGap.BusinessLogic.Tests.Services
{
    [TestFixture]
    public class AuditLoggerTests
    {

        [Test]
        public void AllAuditActionsMustHaveUniqueEnumValues()
        {
            List<AuditedAction> enumOptions = Enum.GetValues(typeof(AuditedAction))
                .Cast<AuditedAction>()
                .ToList();

            foreach (AuditedAction enumOption in enumOptions)
            {
                int numericValueOfEnumOption = (int) enumOption;

                int numberOfEnumOptionsWithThisNumericValue = enumOptions
                    .Select(eo => (int) eo)
                    .Count(eo => eo == numericValueOfEnumOption);

                Assert.AreEqual(
                    1,
                    numberOfEnumOptionsWithThisNumericValue,
                    $"AuditedAction enum has {numberOfEnumOptionsWithThisNumericValue} options with the numerical "
                    + $"value {numericValueOfEnumOption}. The numerical values of an enum must be unique.");
            }
        }

    }
}
