using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;

namespace GenderPayGap.WebUI.ExternalServices.CompaniesHouse
{
    public interface ICompaniesHouseAPI
    {

        CompaniesHouseCompany GetCompany(string companyNumber);
        List<CompaniesHouseSearchResultCompany> SearchCompanies(string query);

    }

    public class CompaniesHouseAPI : ICompaniesHouseAPI
    {

        private static readonly string ApiKey = Global.CompaniesHouseApiKey;

        private const string CompaniesHouseApiServer = "https://api.companieshouse.gov.uk";
        public static Uri BaseUri = new Uri(CompaniesHouseApiServer);

        private readonly HttpClient _httpClient;

        public CompaniesHouseAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public CompaniesHouseCompany GetCompany(string companyNumber)
        {
            if (companyNumber.IsNumber())
            {
                companyNumber = companyNumber.PadLeft(8, '0');
            }

            // capture any serialization errors
            string json = null;
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync($"/company/{companyNumber}").Result;
                // Migration to dotnet core work around return status codes until over haul of this API client
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Companies House API responded with an error: {response.StatusCode}, message: {response.Content}");
                }

                json = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<CompaniesHouseCompany>(json);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"The response from the companies house api returned an invalid json object.\n\nCompanyNumber = {companyNumber}\nResponse = {json}",
                    ex.InnerException ?? ex);
            }
        }

        public List<CompaniesHouseSearchResultCompany> SearchCompanies(string query)
        {
            // capture any serialization errors
            string json = null;
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync($"/search/companies?q={query}&items_per_page=100").Result;
                // Migration to dotnet core work around return status codes until over haul of this API client
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Companies House API responded with an error: {response.StatusCode}, message: {response.Content}");
                }

                json = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<CompaniesHouseSearchResult>(json).Results;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"The response from the companies house api returned an invalid json object.\n\nQuery = {query}\nResponse = {json}",
                    ex.InnerException ?? ex);
            }
        }

        public static void SetupHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = BaseUri;

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(ApiKey, "");
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            ServicePointManager.FindServicePoint(httpClient.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt =>
                        TimeSpan.FromMilliseconds(new Random().Next(1, 1000)) + TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

    }
}
