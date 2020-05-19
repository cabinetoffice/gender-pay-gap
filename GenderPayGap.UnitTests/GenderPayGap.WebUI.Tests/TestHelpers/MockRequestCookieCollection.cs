using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.WebUI.Tests.TestHelpers
{
    class MockRequestCookieCollection : Dictionary<string, string>, IRequestCookieCollection
    {

        public MockRequestCookieCollection(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public ICollection<string> Keys => base.Keys.ToList();
    }
}
