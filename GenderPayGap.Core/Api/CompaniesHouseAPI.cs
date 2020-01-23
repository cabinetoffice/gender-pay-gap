using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.CompaniesHouse;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;

namespace GenderPayGap.Core.API
{
    public interface ICompaniesHouseAPI
    {

        Task<PagedResult<EmployerRecord>> SearchEmployersAsync(string searchText, int page, int pageSize, bool test = false);
        Task<string> GetSicCodesAsync(string companyNumber);
        Task<CompaniesHouseCompany> GetCompanyAsync(string companyNumber);

    }

    public class CompaniesHouseAPI : ICompaniesHouseAPI
    {

        private static readonly string ApiKey = Config.GetAppSetting("CompaniesHouseApiKey");
        public static readonly int MaxRecords = Config.GetAppSetting("CompaniesHouseMaxRecords").ToInt32(400);

        public static Uri BaseUri = new Uri(Config.GetAppSetting("CompaniesHouseApiServer"));

        private readonly HttpClient _httpClient;

        public CompaniesHouseAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<EmployerRecord>> SearchEmployersAsync(string searchText, int page, int pageSize, bool test = false)
        {
            if (searchText.IsNumber())
            {
                searchText = searchText.PadLeft(8, '0');
            }

            var employersPage = new PagedResult<EmployerRecord> {
                PageSize = pageSize, CurrentPage = page, Results = new List<EmployerRecord>()
            };

            if (test)
            {
                employersPage.ActualRecordTotal = 1;
                employersPage.VirtualRecordTotal = 1;

                int id = Numeric.Rand(100000, int.MaxValue - 1);
                var employer = new EmployerRecord {
                    OrganisationName = Config.GetAppSetting("TestPrefix") + "_Ltd_" + id,
                    CompanyNumber = ("_" + id).Left(10),
                    Address1 = "Test Address 1",
                    Address2 = "Test Address 2",
                    City = "Test Address 3",
                    Country = "Test Country",
                    PostCode = "Test Post Code",
                    PoBox = null
                };
                employersPage.Results.Add(employer);
                return employersPage;
            }

            //Get the first page of results and the total records, number of pages, and page size
            var tasks = new List<Task<PagedResult<EmployerRecord>>>();
            Task<PagedResult<EmployerRecord>> page1task = SearchEmployersAsync(searchText, 1, pageSize);
            await page1task;

            //Calculate the maximum page size
            var maxPages = (int) Math.Ceiling((double) MaxRecords / page1task.Result.PageSize);
            maxPages = page1task.Result.PageCount > maxPages ? maxPages : page1task.Result.PageCount;

            //Add a task for ll pages from 2 upwards to maxpages
            for (var subPage = 2; subPage <= maxPages; subPage++)
            {
                tasks.Add(SearchEmployersAsync(searchText, subPage, page1task.Result.PageSize));
            }

            //Wait for all the tasks to complete
            await Task.WhenAll(tasks);

            //Add page 1 to the list of completed tasks
            tasks.Insert(0, page1task);

            //Merge the results from each page into a single page of results
            foreach (Task<PagedResult<EmployerRecord>> task in tasks)
            {
                employersPage.Results.AddRange(task.Result.Results);
            }

            //Get the toal number of records
            employersPage.ActualRecordTotal = page1task.Result.ActualRecordTotal;
            employersPage.VirtualRecordTotal = page1task.Result.VirtualRecordTotal;

            return employersPage;
        }
        
        public async Task<string> GetSicCodesAsync(string companyNumber)
        {
            if (companyNumber.IsNumber())
            {
                companyNumber = companyNumber.PadLeft(8, '0');
            }

            var codes = new HashSet<string>();

            CompaniesHouseCompany company = await GetCompanyAsync(companyNumber);
            if (company == null)
            {
                return null;
            }

            if (company.SicCodes != null)
            {
                foreach (string code in company.SicCodes)
                {
                    codes.Add(code);
                }
            }

            return codes.ToDelimitedString();
        }

        public async Task<CompaniesHouseCompany> GetCompanyAsync(string companyNumber)
        {
            if (companyNumber.IsNumber())
            {
                companyNumber = companyNumber.PadLeft(8, '0');
            }

            // capture any serialization errors
            string json = null;
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"/company/{companyNumber}");
                // Migration to dotnet core work around return status codes until over haul of this API client
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpException(response.StatusCode);
                }

                json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CompaniesHouseCompany>(json);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"The response from the companies house api returned an invalid json object.\n\nCompanyNumber = {companyNumber}\nResponse = {json}",
                    ex.InnerException ?? ex);
            }
        }

        private async Task<PagedResult<EmployerRecord>> SearchEmployersAsync(string searchText, int page, int pageSize)
        {
            var employersPage = new PagedResult<EmployerRecord> {
                PageSize = pageSize, CurrentPage = page, Results = new List<EmployerRecord>()
            };

            string json = await GetCompaniesAsync(searchText, page, employersPage.PageSize);

            dynamic companies = JsonConvert.DeserializeObject(json);
            if (companies != null)
            {
                employersPage.ActualRecordTotal = companies.total_results;
                employersPage.VirtualRecordTotal = companies.total_results;
                employersPage.PageSize = companies.items_per_page;
                if (employersPage.ActualRecordTotal > 0)
                {
                    foreach (dynamic company in companies.items)
                    {
                        var employer = new EmployerRecord();
                        employer.OrganisationName = company.title;
                        employer.NameSource = "CoHo";
                        employer.CompanyNumber = company.company_number;
                        if (employer.CompanyNumber.IsNumber())
                        {
                            employer.CompanyNumber = employer.CompanyNumber.PadLeft(8, '0');
                        }

                        DateTime dateOfCessation = ((string) company?.date_of_cessation).ToDateTime();
                        if (dateOfCessation > DateTime.MinValue)
                        {
                            employer.DateOfCessation = dateOfCessation;
                        }

                        var company_type = (string) company?.company_type;
                        if (company.address != null)
                        {
                            string premises = null,
                                addressLine1 = null,
                                addressLine2 = null,
                                addressLine3 = null,
                                city = null,
                                county = null,
                                country = null,
                                postalCode = null,
                                poBox = null;
                            if (company.address.premises != null)
                            {
                                premises = ((string) company.address.premises).CorrectNull().TrimI(", ");
                            }

                            if (company.address.care_of != null)
                            {
                                addressLine1 = ((string) company.address.care_of).CorrectNull().TrimI(", ");
                            }

                            if (company.address.address_line_1 != null)
                            {
                                addressLine2 = ((string) company.address.address_line_1).CorrectNull().TrimI(", ");
                            }

                            if (!string.IsNullOrWhiteSpace(premises))
                            {
                                addressLine2 = premises + ", " + addressLine2;
                            }

                            if (company.address.address_line_2 != null)
                            {
                                addressLine3 = ((string) company.address.address_line_2).CorrectNull().TrimI(", ");
                            }

                            if (company.address.locality != null)
                            {
                                city = ((string) company.address.locality).CorrectNull().TrimI(", ");
                            }

                            if (company.address.region != null)
                            {
                                county = ((string) company.address.region).CorrectNull().TrimI(", ");
                            }

                            if (company.address.country != null)
                            {
                                country = ((string) company.address.country).CorrectNull().TrimI(", ");
                            }

                            if (company.address.postal_code != null)
                            {
                                postalCode = ((string) company.address.postal_code).CorrectNull().TrimI(", ");
                            }

                            if (company.address.po_box != null)
                            {
                                poBox = ((string) company.address.po_box).CorrectNull().TrimI(", ");
                            }

                            var addresses = new List<string>();
                            if (!string.IsNullOrWhiteSpace(addressLine1))
                            {
                                addresses.Add(addressLine1);
                            }

                            if (!string.IsNullOrWhiteSpace(addressLine2) && !addresses.ContainsI(addressLine2))
                            {
                                addresses.Add(addressLine2);
                            }

                            if (!string.IsNullOrWhiteSpace(addressLine3) && !addresses.ContainsI(addressLine3))
                            {
                                addresses.Add(addressLine3);
                            }

                            employer.Address1 = addresses.Count > 0 ? addresses[0] : null;
                            employer.Address2 = addresses.Count > 1 ? addresses[1] : null;
                            employer.Address3 = addresses.Count > 2 ? addresses[2] : null;

                            employer.City = city;
                            employer.County = county;
                            employer.Country = country;
                            employer.PostCode = postalCode;
                            employer.PoBox = poBox;
                            employer.AddressSource = "CoHo";
                        }

                        employersPage.Results.Add(employer);
                    }
                }
            }

            return employersPage;
        }

        private async Task<string> GetCompaniesAsync(string companyName, int page, int pageSize = 10, string httpException = null)
        {
            if (!string.IsNullOrWhiteSpace(httpException))
            {
                throw new HttpRequestException(httpException);
            }

            int startIndex = (page * pageSize) - pageSize;

            string json = await _httpClient.GetStringAsync(
                $"/search/companies/?q={companyName}&items_per_page={pageSize}&start_index={startIndex}");
            return json;
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
