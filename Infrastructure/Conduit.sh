
# Get parameters
PAAS_ENV_SHORTNAME=$1
LOCAL_PORT=$2


# Login
./LoginToGovPaaS.sh


# Target future commands at the right space
cf target -s "gpg-${PAAS_ENV_SHORTNAME}"



# Fetch the keys that you might want to use to connect to the database and cache
echo ""
echo "======================================="
echo "HERE ARE THE KEYS YOU MIGHT WANT TO USE"
echo "======================================="
echo ""

# - Database
cf service-key "gpg-${PAAS_ENV_SHORTNAME}-db" "gpg-${PAAS_ENV_SHORTNAME}-db-developerkey"

# - Cache
cf service-key "gpg-${PAAS_ENV_SHORTNAME}-cache" "gpg-${PAAS_ENV_SHORTNAME}-cache-developerkey"



# Connect to PaaS via Conduit
echo ""
echo "==========================="
echo "IGNORE KEYS PAST THIS POINT"
echo "==========================="
echo ""

# - Setup variables
DB_NAME="gpg-${PAAS_ENV_SHORTNAME}-db"
CACHE_NAME="gpg-${PAAS_ENV_SHORTNAME}-cache"
SPACE_NAME="gpg-${PAAS_ENV_SHORTNAME}"

DEVELOPER_MACHINE_NAME=$(hostname)
CONDUIT_APP_NAME="conduit-gpg-${PAAS_ENV_SHORTNAME}-${DEVELOPER_MACHINE_NAME}"

# - Delete any old conduit apps - sometimes they don't get cleared up correctly
cf delete "${CONDUIT_APP_NAME}" -f

# - Connect
cf conduit "${DB_NAME}" "${CACHE_NAME}" --local-port $LOCAL_PORT --space "${SPACE_NAME}" --app-name "${CONDUIT_APP_NAME}"



# Wait for user input - just to make sure the window doens't close without them noticing
echo ""
echo ""
read  -n 1 -p "Press Enter to finish:" unused
