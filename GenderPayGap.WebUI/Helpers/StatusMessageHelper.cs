using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Helpers
{
    public static class StatusMessageHelper
    {

        public static void SetStatusMessage(
            HttpResponse httpResponse,
            string message,
            string path)
        {
            string randomId = Guid.NewGuid().ToString("N").Substring(0, 8);
            string cookieName = $"status_message_{randomId}";

            var statusMessage = new StatusMessage
            {
                Message = message,
                Path = path
            };
            string statusMessageJsonString = JsonConvert.SerializeObject(statusMessage);

            httpResponse.Cookies.Append(
                cookieName,
                statusMessageJsonString,
                new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true,
                    MaxAge = TimeSpan.FromSeconds(30),
                    Path = path
                });
        }

        public static List<string> GetStatusMessages(HttpRequest httpRequest, HttpResponse httpResponse)
        {
            List<string> statusMessageCookieNames = httpRequest.Cookies.Keys
                .Where(k => k.StartsWith("status_message_"))
                .ToList();

            var messages = new List<string>();

            foreach (string cookieName in statusMessageCookieNames)
            {
                // Get the message
                string messageAsJsonString = httpRequest.Cookies[cookieName];
                StatusMessage message = JsonConvert.DeserializeObject<StatusMessage>(messageAsJsonString);

                if (!string.IsNullOrWhiteSpace(message.Message))
                {
                    messages.Add(message.Message);
                }

                // Set a cookie to remove the message
                httpResponse.Cookies.Append(
                    cookieName,
                    "",
                    new CookieOptions
                    {
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        HttpOnly = true,
                        MaxAge = TimeSpan.FromSeconds(0),
                        Path = message.Path
                    });
            }

            return messages;
        }

    }

    internal class StatusMessage
    {
        public string Message { get; set; }
        public string Path { get; set; }
    }
}
