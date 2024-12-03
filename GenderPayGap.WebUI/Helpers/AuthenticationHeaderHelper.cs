using System.Net.Http.Headers;
using System.Text;

namespace GenderPayGap.WebUI.Helpers
{
    public static class AuthenticationHeaderHelper
    {

        public static AuthenticationHeaderValue GetBasicAuthenticationHeaderValue(string username, string password)
        {
            string base64UsernameAndPassword = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            
            return new AuthenticationHeaderValue(
                "Basic",
                base64UsernameAndPassword);
        }

    }
}
