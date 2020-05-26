using System.Collections.Generic;
using System.Diagnostics;
using GenderPayGap.Extensions;
using NUnit.Framework;

namespace GenderPayGap.Tests.Common.Classes
{
    public static class CompareHelpers
    {

        /// <summary>
        ///     Looks for properties which exist in two objects and compares them and throws an exception containing all the
        ///     differences.
        /// </summary>
        /// <param name="expectedValues">Source object (or list) to compare.</param>
        /// <param name="actualValues">Target object (or list) to compare.</param>
        /// <param name="membersToIgnore">
        ///     Ignore Data Table Names, Data Table Column Names, properties, or fields by name during
        ///     the comparison. Case sensitive. The default is to compare all members
        /// </param>
        /// <param name="membersToInclude">
        ///     Only compare elements by name for Data Table Names, Data Table Column Names, properties
        ///     and fields. Case sensitive. The default is to compare all members
        /// </param>
        /// <param name="ignoreCollectionOrder">
        ///     if false (default) items are compared exactly according to order in collection. If
        ///     true item order is dependent on the default comparer.
        /// </param>
        /// <param name="caseSensitive">When comparing strings or StringBuilder types, perform a case sensitive comparison</param>
        /// <param name="maxDifferences">The maximum number of differences to return. Default is -1 ( for int.maxvalue)</param>
        /// <param name="compareChildren">
        ///     If true, child objects will be compared. The default is true. If false, and a list or
        ///     array is compared list items will be compared but not their children.
        /// </param>
        /// <param name="ignoreObjectTypes">If true, objects will be compared ignore their type diferences. The default is true.</param>
        [DebuggerStepThrough]
        public static void Compare(this object expectedValues,
            object actualValues,
            IEnumerable<string> membersToIgnore = null,
            IEnumerable<string> membersToInclude = null,
            bool ignoreCollectionOrder = false,
            bool caseSensitive = true,
            int maxDifferences = -1,
            bool compareChildren = true,
            bool ignoreObjectTypes = true)
        {
            Assert.Multiple(
                () => {
                    foreach (AutoMap.Diff diff in expectedValues.GetDifferences(
                            actualValues,
                            membersToIgnore,
                            membersToInclude,
                            ignoreCollectionOrder,
                            caseSensitive,
                            maxDifferences < 1 ? int.MaxValue : maxDifferences,
                            compareChildren,
                            ignoreObjectTypes)
                        .ToListOrEmpty())
                    {
                        Assert.Fail($"{diff.Name}='{diff.NewValue}' and was expecting '{diff.OldValue}'");
                    }
                });
        }

    }
}
