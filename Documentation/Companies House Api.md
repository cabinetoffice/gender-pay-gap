# Companies House API

The gender pay gap service uses the Companies House API to get information about the employers in the system such as Registered Addresses and also when searching by Company Number.

## The API

Documentation can be found here: https://developer-specs.company-information.service.gov.uk/guides/index

## How we use the API

All calls to the Companies House API are via the class CompaniesHouseApi in the WebUI project. This is used for a few processes:

* FetchCompaniesHouseDataJob - This is a job that is run every 5 minutes. It updates various properties of the Employers such as the Address, Name, SIC Codes from the API.
* The search process by Company Number - When adding an employer it searches against this API, and if searching by company number, when the user selects one it populates the employer's information from the API
* Various admin side pages relating to managing employer addresses etc. also call this API to look up latest details.

## Rate Limit

The Companies House API has a rate limit of 600 requests per 5 minutes https://developer-specs.company-information.service.gov.uk/guides/rateLimiting.

This should not be hit under normal operation, the FetchCompaniesHouseDataJob has an artificial limit set of 100 per 5 minutes on the production environment, and 10 for the rest of the environments, see MaxNumCallsCompaniesHouseApiPerFiveMins in the appsettings.json files. The Add Employer process makes 1-3 calls depending on how you search for the employer.

However, we have seen during load testing that this limit can be hit, the service will recieve 429 Too Many Requests HTTP status codes from all requests beyond the limit, and throws an exception containing this status code.