# gpg-ipdeny

gpg-ipdeny is an IP denylisting nginx application based upon the IP allowing version found here: [GOV.UK PaaS IP authentication route service](https://github.com/alphagov/paas-ip-authentication-route-service)

It is designed to allow access to the Gender Pay Gap Service to all IP addresses except those specifically set, returning a 403 response to those banned IPs.

## Manual Deployment

Since the list of banned IPs should not update particularly frequently, the application is deployed manually via the Deploy.sh script.

The script relies up having the command line processor [jq](https://stedolan.github.io/jq/) available. 

When using gitbash for Windows this can simply be added by running the following command, it should also be possible to add it to the system path:
```
curl -L -o /usr/bin/jq.exe https://github.com/stedolan/jq/releases/latest/download/jq-win64.exe
```

Before running the deployment script you must be logged into GOV.UK PaaS using the Cloud Foundry CLI.

The deployment script takes the following parameters:
* -a PROTECTED_APP_NAME The application to protect
* -e PROTECTED_APP_SPACE_NAME The name of the space the apps are in
* -f DENIED_IPS_FILENAME The filename containing the list of IPs to block
* -r ROUTE_SERVICE_APP_NAME The name of the service app to create/update
* -s ROUTE_SERVICE_NAME The name of the service to create/update
* -m MIN_COUNT_INSTANCES The minimum number of instances to run
* -M MAX_COUNT_INSTANCES The maximum number of instances to run

For example to set up ip denylisting on the dev space, if you've created/placed the denylist file in the same directory as the deployment script, the command could be run (using the gitbash window set up with jq earlier) as 
```
./deploy.sh -e gpg-dev -r gpg-dev-ipdeny -s gpg-dev-ipdeny-service -a gender-pay-gap-dev -f "GPG-IP-Denylist.txt" -m 1 -M 2
```

The file listing banned IPs can be found in Zoho, and is called "GPG-IP-Denylist.txt". This should not be checked into source control, and that filename has been added to the .gitignore. Since the script accepts any file for the denylist, IP denylisting can be tested by providing a different file. Take care not to accidentally check in any differently name denylist file.

When updating this list, each IP should be listed on a new line.
If there are any errors when starting the app, these are most likely to have been caused by a syntax error in the list of IPs to deny. By checking the environment variables that have been set on the environment, it should be possible to tell where the issue is in the provided list of IPs.
There is also a copy of the GPG-IP-Denylist.txt file stored in the Library - Secure Files of Azure. If the list of IPs is updated, this should be updated along with the version in Zoho.

## Automated deployment
The IP deny app is deployed via the same Azure release pipelines using a Bash step in the Deploy to PaaS (push) task group

This relies on the copy of the GPG-IP-Denylist.txt file stored in the Library - Secure Files of Azure. If the list of IPs is updated, this should be updated along with the version in Zoho.

The deployment is run via the same deploy script as the manual deployments, so changes to the deployment script may break the automated deployments. Due to the scripts reliance on jq, we need to have this available. There doesn't appear to be a simple way to do this in Azure, so the bash step that run the deployment handles adding a version and making sure the deployment script can access it.

## Rate Limiting
The IP deny app has some rate limiting configurations documented below

```
limit_req_zone $binary_remote_addr zone=rate_limiting_ip_address_bucket:10m rate=50r/s;

...

location / {
      ... 
      
      limit_req zone=rate_limiting_ip_address_bucket;
      
      ...
}
```

IP addresses of requests to the service are stored in the `rate_limiting_ip_address_bucket` with a size of __10 Megabytes__.
We then define a maximum request rate of `rate=50r/s` meaning __50 Request per second__. The rate limiting is then applied to any route on the site matching: `/` (all pages/routes)

These restrictions are applied per IP address.

To test the rate limiting client side, you can use the "hey" package (you'll need to [download GoLang](https://golang.org/dl/) first). To install this package just run:
```
go get -u github.com/rakyll/hey
```

You can then test the rate limiting using the command `hey` like so:

```
 hey -n 100 -c 10 -q 5 https://gender-pay-gap-loadtest.london.cloudapps.digital/
```

This makes a total of __100 Requests__ to `https://gender-pay-gap-loadtest.london.cloudapps.digital/` with __10 concurrent workers__ all making __5 Queries per second__