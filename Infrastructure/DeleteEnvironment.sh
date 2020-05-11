
########################
# Start of configuration
 
PAAS_ENV_SHORTNAME="mynewtestenv" # This is the bit after gpg- (e.g. for gpg-dev, PAAS_ENV_SHORTNAME would just be 'dev')



#################
# Start of script

if [ "${PAAS_ENV_SHORTNAME}" == "dev" ] || [ "${PAAS_ENV_SHORTNAME}" == "test" ] || [ "${PAAS_ENV_SHORTNAME}" == "preprod" ] || [ "${PAAS_ENV_SHORTNAME}" == "prod" ];
then
	echo "You probably don't want to delete one of the main 4 environment (${PAAS_ENV_SHORTNAME})"
	exit 1
fi
echo "Deleting ${PAAS_ENV_SHORTNAME}"



#------
# Login
./LoginToGovPaas.sh

# Target future commands at this space
cf target -s "gpg-${PAAS_ENV_SHORTNAME}"



#-----------
# Delete App
# - Un-bind app from file storage
#cf unbind-service "gender-pay-gap-${PAAS_ENV_SHORTNAME}" "gpg-${PAAS_ENV_SHORTNAME}-filestorage"

# - Un-bind app from database
#cf unbind-service "gender-pay-gap-${PAAS_ENV_SHORTNAME}" "gpg-${PAAS_ENV_SHORTNAME}-db"

# - Un-bind app from Redis cache
#cf unbind-service "gender-pay-gap-${PAAS_ENV_SHORTNAME}" "gpg-${PAAS_ENV_SHORTNAME}-cache"

# - Delete the app itself
cf v3-delete "gender-pay-gap-${PAAS_ENV_SHORTNAME}" -f



#------------------------------
# Delete AWS S3 backing service
# - Delete the service key
cf delete-service-key "gpg-${PAAS_ENV_SHORTNAME}-filestorage" "gpg-${PAAS_ENV_SHORTNAME}-filestorage-key" -f

# - Delete the service
cf delete-service "gpg-${PAAS_ENV_SHORTNAME}-filestorage" -f



#-----------------------------------------
# Delete Postgres database backing service
# - Delete the service key
cf delete-service-key "gpg-${PAAS_ENV_SHORTNAME}-db" "gpg-${PAAS_ENV_SHORTNAME}-db-developerkey" -f

# - Delete the service
cf delete-service "gpg-${PAAS_ENV_SHORTNAME}-db" -f

echo "Waiting for the Postgres database to be deleted (normally takes 5-10 mins)"

RESULT=$(cf service "gpg-${PAAS_ENV_SHORTNAME}-db")
while [ -z "${RESULT##*delete in progress*}" ]
do
	echo -n "."
	sleep 5s
	RESULT=$(cf service "gpg-${PAAS_ENV_SHORTNAME}-db")
done
echo "" # To create a new-line

cf service "gpg-${PAAS_ENV_SHORTNAME}-db"



#-----------------------------------
# Delete Redis cache backing service
# - Delete the service key
cf delete-service-key "gpg-${PAAS_ENV_SHORTNAME}-cache" "gpg-${PAAS_ENV_SHORTNAME}-cache-developerkey" -f

# - Delete the service
cf delete-service "gpg-${PAAS_ENV_SHORTNAME}-cache" -f

echo "Waiting for the Redis cache to be deleted (normally takes 5-10 mins)"

RESULT=$(cf service "gpg-${PAAS_ENV_SHORTNAME}-cache")
while [ -z "${RESULT##*delete in progress*}" ]
do
	echo -n "."
	sleep 5s
	RESULT=$(cf service "gpg-${PAAS_ENV_SHORTNAME}-cache")
done
echo "" # To create a new-line

cf service "gpg-${PAAS_ENV_SHORTNAME}-cache"



#-----------------
# Delete the space
cf delete-space "gpg-${PAAS_ENV_SHORTNAME}" -o geo-gender-pay-gap-service -f



# Wait for user input - just to make sure the window doens't close without them noticing
echo ""
echo ""
read  -n 1 -p "Press Enter to finish:" unused
