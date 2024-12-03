using GenderPayGap.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GenderPayGap.WebUI.Tests.TestHelpers
{
    public static class InMemoryTestDabaseHelper
    {
        public static GpgDatabaseContext CreateInMemoryTestDatabase(params object[] dbObjects)
        {
            //Get the method name of the unit test or the parent
            string testName = TestContext.CurrentContext.Test.FullName;
            if (string.IsNullOrWhiteSpace(testName))
            {
                testName = UiTestHelper.GetCurrentTestName();
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
