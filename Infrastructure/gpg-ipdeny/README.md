# gpg-ipdeny

gpg-ipdeny is an IP denylisting nginx application based upon the IP allowing version found here: [GOV.UK PaaS IP authentication route service](https://github.com/alphagov/paas-ip-authentication-route-service)

It is designed to allow access to the Gender Pay Gap Service to all IP addresses except those specifically set, returning a 403 response to those banned IPs.

## Deployment

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
* -i DENIED_IPS A comma separated list of IP addresses to be banned
* -r ROUTE_SERVICE_APP_NAME The name of the service app to create/update
* -s ROUTE_SERVICE_NAME The name of the service to create/update

For example to set up ip denylisting on the dev space the command could be run (using the gitbash window set up with jq earlier) as 
```
./deploy.sh -e gpg-dev -r gpg-dev-ipdeny -s gpg-dev-ipdeny-service -a gender-pay-gap-dev -i "1.2.3.4,2.3.4.5"
```

The comma separated list of banned IPs can be found in Zoho, when updating this list, newlines should be avoided as they can cause syntax errors within the nginx.conf. This can be seen by an error when deploying, and then if you try and run
```
cf start ROUTE_SERVICE_APP_NAME
```
an error similar to the following will display
```
2021/01/25 15:25:07 [emerg] 29#0: unknown directive "
   " in /tmp/conf410448399/nginx.conf:73
   nginx: configuration file /tmp/conf410448399/nginx.conf test failed
          **ERROR** Could not validate nginx.conf: nginx.conf contains syntax errors: exit status 1
   Failed to compile droplet: Failed to run all supply scripts: exit status 14
```
By checking the environment variables that have been set on the environment, it should be possible to tell where the issue is in the provided list of IPs