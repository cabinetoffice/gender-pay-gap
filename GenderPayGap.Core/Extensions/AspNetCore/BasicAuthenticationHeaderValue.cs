using System;
using System.Net.Http.Headers;
using System.Text;

namespace GenderPayGap.Extensions.AspNetCore
{
    public class BasicAuthenticationHeaderValue : AuthenticationHeaderValue
    {

        public BasicAuthenticationHeaderValue(string username, string password) : base(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"))) { }

    }
}
