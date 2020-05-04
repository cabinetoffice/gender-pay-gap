
########################
# Start of configuration

# Name - This is the bit after gpg- - e.g. for gpg-dev, PAAS_ENV_SHORTNAME would just be 'dev'
PAAS_ENV_SHORTNAME="mynewtestenv"

# Database size - we normally use 'small-11' for testing and 'small-ha-11' for production
DATABSE_SIZE="small-11"

# Redis cache size - we normally use 'tiny-4.x' for all environments
REDIS_SIZE="tiny-4.x"



#################
# Start of script

#------
# Login
./LoginToGovPaas.sh


#-----------------
# Create the space
cf create-space "gpg-${PAAS_ENV_SHORTNAME}" -o geo-gender-pay-gap-service

# - Target future commands at this space
cf target -s "gpg-${PAAS_ENV_SHORTNAME}"


#---------------------------
# Add AWS S3 backing service
# - Create the service
cf create-service aws-s3-bucket default "gpg-${PAAS_ENV_SHORTNAME}-filestorage"

# - Create a key to access the service
#   Note: this is only needed to access the S3 bucket from outside of Gov.UK PaaS (e.g. from Azure)
cf create-service-key "gpg-${PAAS_ENV_SHORTNAME}-filestorage" "gpg-${PAAS_ENV_SHORTNAME}-filestorage-key" -c '{"allow_external_access": true}'

# - Get the key (and print to the console)
cf service-key "gpg-${PAAS_ENV_SHORTNAME}-filestorage" "gpg-${PAAS_ENV_SHORTNAME}-filestorage-key"


#-----------------------------------------
# Create Postgres database backing service
# - Create the database itself
cf create-service postgres "${DATABSE_SIZE}" "gpg-${PAAS_ENV_SHORTNAME}-db"

echo "Waiting for the Postgres database to be created (normally takes 5-10 mins)"

RESULT=$(cf service "gpg-${PAAS_ENV_SHORTNAME}-db")
while [ -z "${RESULT##*create in progress*}" ]
do
	echo -n "."
	sleep 5s
	RESULT=$(cf service "gpg-${PAAS_ENV_SHORTNAME}-db")
done
echo "" # To create a new-line

if [ -z "${RESULT##*create succeeded*}" ]
then
	echo "Create successful"
	cf service "gpg-${PAAS_ENV_SHORTNAME}-db"
else
	echo "Create failed"
	cf service "gpg-${PAAS_ENV_SHORTNAME}-db"
	exit 1
fi

# - Create a key to access the service
#   Note: this is only needed to access the database from outside of Gov.UK PaaS (e.g. from a developer machine)
cf create-service-key "gpg-${PAAS_ENV_SHORTNAME}-db" "gpg-${PAAS_ENV_SHORTNAME}-db-developerkey"

# - Get the key (and print to the console)
cf service-key "gpg-${PAAS_ENV_SHORTNAME}-db" "gpg-${PAAS_ENV_SHORTNAME}-db-developerkey"


#-----------------------------------
# Create Redis cache backing service
# -Create the Redis cache itself
cf create-service redis "${REDIS_SIZE}" "gpg-${PAAS_ENV_SHORTNAME}-cache"

echo "Waiting for the Redis cache to be created (normally takes 5-10 mins)"

RESULT=$(cf service "gpg-${PAAS_ENV_SHORTNAME}-cache")
while [ -z "${RESULT##*create in progress*}" ]
do
	echo -n "."
	sleep 5s
	RESULT=$(cf service "gpg-${PAAS_ENV_SHORTNAME}-cache")
done
echo "" # To create a new-line

if [ -z "${RESULT##*create succeeded*}" ]
then
	echo "Create successful"
	cf service "gpg-${PAAS_ENV_SHORTNAME}-cache"
else
	echo "Create failed"
	cf service "gpg-${PAAS_ENV_SHORTNAME}-cache"
	exit 1
fi

# - Create a key to access the service
#   Note: this is only needed to access the redis cache from outside of Gov.UK PaaS (e.g. from a developer machine)
cf create-service-key "gpg-${PAAS_ENV_SHORTNAME}-cache" "gpg-${PAAS_ENV_SHORTNAME}-cache-developerkey"

# - Get the key (and print to the console)
cf service-key "gpg-${PAAS_ENV_SHORTNAME}-cache" "gpg-${PAAS_ENV_SHORTNAME}-cache-developerkey"



# Wait for user input - just to make sure the window doens't close without them noticing
echo ""
echo ""
read  -n 1 -p "Press Enter to finish:" unused
