using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests.Common.Classes
{
    public static class AutoFacHelpers
    {

        /// <summary>
        ///     Resolves to a Mock object for the of specified instance and resolves the constructor arguments using the container
        ///     context
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="context"></param>
        /// <param name="callBase">
        ///     Whether the base member virtual implementation will be called for mocked classes if no setup is
        ///     matched
        /// </param>
        /// <param name="ctorArgs"></param>
        /// <returns></returns>
        public static Mock<TInstance> ResolveAsMock<TInstance>(this IComponentContext context, bool callBase, params Type[] ctorArgs)
            where TInstance : class
        {
            var r = new List<object>();
            foreach (Type t in ctorArgs)
            {
                r.Add(context.Resolve(t));
            }

            var mock = new Mock<TInstance>(r.ToArray());
            mock.CallBase = callBase;
            return mock;
        }


        public static GpgDatabaseContext CreateInMemoryTestDatabase(params object[] dbObjects)
        {
            //Get the method name of the unit test or the parent
            string testName = TestContext.CurrentContext.Test.FullName;
            if (string.IsNullOrWhiteSpace(testName))
            {
                testName = MethodBase.GetCurrentMethod().FindParentWithAttribute<TestAttribute>().Name;
            }

            DbContextOptionsBuilder<GpgDatabaseContext> optionsBuilder =
                new DbContextOptionsBuilder<GpgDatabaseContext>().UseInMemoryDatabase(testName);

            optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            // show more detailed EF errors i.e. ReturnId value instead of '{ReturnId}' n the logs etc...
            optionsBuilder.EnableSensitiveDataLogging();

            var dbContext = new GpgDatabaseContext(optionsBuilder.Options);
            if (dbObjects != null && dbObjects.Length > 0)
            {
                foreach (object item in dbObjects)
                {
                    var enumerable = item as IEnumerable<object>;
                    if (enumerable == null)
                    {
                        dbContext.Add(item);
                    }
                    else
                    {
                        dbContext.AddRange(enumerable);
                    }
                }

                dbContext.SaveChanges();
            }

            return dbContext;
        }

    }
}
