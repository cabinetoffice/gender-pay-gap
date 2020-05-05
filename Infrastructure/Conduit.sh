
# Get parameters
PAAS_ENV_SHORTNAME=$1
PAAS_SERVICE=$2
LOCAL_PORT=$3


# Login
./LoginToGovPaaS.sh


# Target future commands at the right space
cf target -s "gpg-${PAAS_ENV_SHORTNAME}"



# Fetch the keys that you might want to use to connect to the database or cache
echo ""
echo "======================================="
echo "HERE ARE THE KEYS YOU MIGHT WANT TO USE"
echo "======================================="
echo ""

echo "Host: 127.0.0.1"
echo "Port: ${LOCAL_PORT}"
echo "Username & Password: see below"
echo ""

cf service-key "gpg-${PAAS_ENV_SHORTNAME}-${PAAS_SERVICE}" "gpg-${PAAS_ENV_SHORTNAME}-${PAAS_SERVICE}-developerkey"



# Connect to PaaS via Conduit
echo ""
echo "==========================="
echo "IGNORE KEYS PAST THIS POINT"
echo "==========================="
echo ""

# - Setup variables
SERVICE_NAME="gpg-${PAAS_ENV_SHORTNAME}-${PAAS_SERVICE}"
SPACE_NAME="gpg-${PAAS_ENV_SHORTNAME}"

DEVELOPER_MACHINE_NAME=$(hostname)
CONDUIT_APP_NAME="conduit-gpg-${PAAS_ENV_SHORTNAME}-${PAAS_SERVICE}-${DEVELOPER_MACHINE_NAME}"

# - Delete any old conduit apps - sometimes they don't get cleared up correctly
cf delete "${CONDUIT_APP_NAME}" -f

# - Connect
cf conduit "${SERVICE_NAME}" --local-port $LOCAL_PORT --space "${SPACE_NAME}" --app-name "${CONDUIT_APP_NAME}"



# Wait for user input - just to make sure the window doens't close without them noticing
echo ""
echo ""
read  -n 1 -p "Press Enter to finish:" unused
