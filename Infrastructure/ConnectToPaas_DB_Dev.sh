
########################
# Start of configuration

# Name - This is the bit after gpg- - e.g. for gpg-dev, PAAS_ENV_SHORTNAME would just be 'dev'
PAAS_ENV_SHORTNAME="dev"

# Service - DB or cache
PAAS_SERVICE="db"

# The local port to use
LOCAL_PORT=7100



#################
# Start of script

# Run the Conduit.sh command with the above environment variables set
./Conduit.sh $PAAS_ENV_SHORTNAME $PAAS_SERVICE $LOCAL_PORT
