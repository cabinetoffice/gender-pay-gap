using Newtonsoft.Json;

namespace GenderPayGap.WebUI.ExternalServices
{
    public class PostcodesIoApi
    {

        public static bool IsValidPostcode(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
            {
                // We used to see quite a lot of error in the dependency logs saying "GET /postcodes//validate returned a 404"
                // This isn't wrong, but it makes the error logs a bit noisy
                // To make this a bit less noisy, we can reject any null / empty postcodes as being invalid
                return false;
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("https://api.postcodes.io");

                    string path = $"/postcodes/{postcode}/validate";

                    HttpResponseMessage response = httpClient.GetAsync(path).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string bodyString = response.Content.ReadAsStringAsync().Result;
                        var body = JsonConvert.DeserializeObject<PostcodesIoApiValidateResponse>(bodyString);
                        return body.result;
                    }

                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }

    internal class PostcodesIoApiValidateResponse
    {

        public bool result { get; set; }

    }
}
