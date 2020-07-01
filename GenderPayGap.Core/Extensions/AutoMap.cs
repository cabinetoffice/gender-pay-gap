using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using KellermanSoftware.CompareNetObjects;

namespace GenderPayGap.Extensions
{
    public static class AutoMap
    {

        [Obsolete("We are trying to reduce the use of Automapper. Please don't use this")]
        public static S GetClone<S>(this S source)
        {
            if (source == null || source.Equals(default(S)))
            {
                return default;
            }

            var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<S, S>(); });
            IMapper iMapper = mapperConfig.CreateMapper();
            return iMapper.Map<S, S>(source);
        }

        private static T Convert<S, T>(this S source)
        {
            if (source == null || source.Equals(default(S)) || source.Equals(default(T)))
            {
                return default;
            }

            var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<S, T>(); });
            IMapper iMapper = mapperConfig.CreateMapper();

            return iMapper.Map<S, T>(source);
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldObject"></param>
        /// <param name="newObject"></param>
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
        /// <param name="maxDifferences">The maximum number of differences to return. Default is int.MaxValue</param>
        /// <param name="compareChildren">
        ///     If true, child objects will be compared. The default is true. If false, and a list or
        ///     array is compared list items will be compared but not their children.
        /// </param>
        /// <param name="ignoreObjectTypes">If true, objects will be compared ignore their type diferences. The default is false.</param>
        /// <returns></returns>
        [Obsolete("We are trying to reduce the use of Automapper. Please don't use this")]
        public static IEnumerable<Diff> GetDifferences<S, T>(this S oldObject,
            T newObject,
            IEnumerable<string> membersToIgnore = null,
            IEnumerable<string> membersToInclude = null,
            bool ignoreCollectionOrder = false,
            bool caseSensitive = true,
            int maxDifferences = int.MaxValue,
            bool compareChildren = true,
            bool ignoreObjectTypes = false) where S : class
        {
            var target = newObject as S;
            if (target == default(S))
            {
                target = Convert<T, S>(newObject);
            }

            var config = new ComparisonConfig {
                CaseSensitive = caseSensitive,
                IgnoreCollectionOrder = ignoreCollectionOrder,
                ShowBreadcrumb = true,
                TreatStringEmptyAndNullTheSame = true,
                MaxMillisecondsDateDifference = 1000,
                MaxDifferences = maxDifferences,
                CompareChildren = compareChildren,
                IgnoreObjectDisposedException = true,
                IgnoreUnknownObjectTypes = true,
                IgnoreObjectTypes = ignoreObjectTypes,
                MaxStructDepth = 1
            };

            if (membersToIgnore != null && membersToIgnore.Any())
            {
                config.MembersToIgnore = membersToIgnore.ToList();
            }

            if (membersToInclude != null && membersToInclude.Any())
            {
                config.MembersToInclude = membersToInclude.ToList();
            }

            //This is the comparison class
            var compareLogic = new CompareLogic(config);
            ComparisonResult results = compareLogic.Compare(oldObject, target);
            foreach (Difference diff in results.Differences)
            {
                yield return new Diff {
                    Name =
                        $"{(diff.PropertyName.StartsWith($"{diff.ParentPropertyName}.") ? null : diff.ParentPropertyName)}{diff.PropertyName}.{diff.ChildPropertyName}"
                            .TrimI("."),
                    OldValue = diff.Object1Value,
                    NewValue = diff.Object2Value,
                    Description = diff.ToString()
                };
            }
        }

        public class Diff
        {

            public string Description;
            public object Name;
            public object NewValue;
            public object OldValue;

            public override string ToString()
            {
                return Description;
            }

        }

    }
}
