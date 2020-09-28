using System.Collections.Generic;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class ControllerBuilder<T>
    where T : Controller
    {

        private long userId = 0;
        private Dictionary<string, StringValues> requestFormValues;
        private readonly List<object> databaseObjects = new List<object>();

        public ControllerBuilder<T> WithUserId(long userId)
        {
            this.userId = userId;
            return this;
        }

        public ControllerBuilder<T> WithRequestFormValues(Dictionary<string, StringValues> requestFormValues)
        {
            this.requestFormValues = requestFormValues;
            return this;
        }

        public ControllerBuilder<T> WithDatabaseObjects(params object[] databaseObjects)
        {
            this.databaseObjects.AddRange(databaseObjects);
            return this;
        }

        public T Build()
        {
            return NewUiTestHelper.GetController<T>(userId, requestFormValues, databaseObjects.ToArray());
        }

    }
}
