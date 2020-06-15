using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GenderPayGap.Core.Api
{
    public class PostcodesIoApi
    {

        public static async Task<bool> IsValidPostcode(string postcode)
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

                    HttpResponseMessage response = await httpClient.GetAsync(path);

                    if (response.IsSuccessStatusCode)
                    {
                        string bodyString = await response.Content.ReadAsStringAsync();
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
