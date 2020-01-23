using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GenderPayGap.Extensions
{
    public static class Web
    {

        public enum HttpMethods
        {

            Get,
            Post,
            Delete,
            Patch,
            Put

        }

        public static async Task<string> CallApiAsync(HttpMethods httpMethod,
            string url,
            string username = null,
            string password = null,
            string body = null,
            Dictionary<string, string> headers = null)
        {
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(password))
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Basic",
                        Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
                }

                if (headers != null)
                {
                    foreach (string key in headers.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key, headers[key]);
                    }
                }

                HttpContent httpContent = null;
                if (body != null)
                {
                    if (!httpMethod.IsAny(HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch))
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(httpMethod),
                            "HttpMethod must be Post, Put or Patch when a body is specified");
                    }

                    if (string.IsNullOrWhiteSpace(body))
                    {
                        throw new ArgumentNullException("body", "body is empty");
                    }

                    httpContent = new StringContent(
                        body,
                        Encoding.UTF8,
                        httpMethod == HttpMethods.Patch ? "application/json-patch+json" : "application/json");
                }
                else if (httpMethod.IsAny(HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch) && (headers == null || headers.Count == 0))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(body),
                        "You must supply a body (or additional headers) when Post, Put or Patch when a body is specified");
                }


                using (HttpResponseMessage response = httpMethod == HttpMethods.Get ? await client.GetAsync(url) :
                    httpMethod == HttpMethods.Delete ? await client.DeleteAsync(url) :
                    httpMethod == HttpMethods.Post ? await client.PostAsync(url, httpContent) :
                    httpMethod == HttpMethods.Put ? await client.PutAsync(url, httpContent) :
                    httpMethod == HttpMethods.Patch ? await client.PatchAsync(url, httpContent) :
                    throw new ArgumentOutOfRangeException(nameof(httpMethod), "HttpMethod must be Get, Delete, Post or Put"))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
                        {
                            headers[header.Key] = header.Value.Distinct().ToDelimitedString();
                        }
                    }

                    return responseBody;
                }
            }
        }

    }
}
