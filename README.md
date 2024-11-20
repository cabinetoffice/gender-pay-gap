![Build Status](https://dev.azure.com/govtequalitiesoffice/Gender%20Pay%20Gap/_apis/build/status/Build%20all%20branches)

# Gender Pay Gap Service 

This is the code for the [Gender Pay Gap service](https://gender-pay-gap.service.gov.uk).

From 2017, any organisation that has 250 or more employees must publish and report specific figures about their gender pay gap. 
The gender pay gap is the difference between the average earnings of men and women, expressed relative to men’s earnings. 
For example, ‘women earn 15% less than men per hour’.

This service allows organisations to report their gender pay gap data. It then makes this data available to the general public. 
It is managed by the Office for Equality and Opportunity (OEO) which forms part of the Cabinet Office.


## Technical documentation

This is a C# solution that has a user interface for public viewing pages, organisation reporting pages and an admin portal. There are also
regular jobs for pulling information from Companies House and sending reminder emails. We use Identity Server for authentication 
and SQL Server for the database.

### Dependencies

- [GOV.UK Notify](https://www.notifications.service.gov.uk/) - to send emails and letters
- [Companies House API](https://developer.companieshouse.gov.uk/api/docs/index.html) - to look up information for organisations

## Licence

[MIT License](LICENCE)
