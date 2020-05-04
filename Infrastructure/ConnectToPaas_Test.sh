
########################
# Start of configuration

# Name - This is the bit after gpg- - e.g. for gpg-dev, PAAS_ENV_SHORTNAME would just be 'dev'
PAAS_ENV_SHORTNAME="test"

# The local port to use
# - the database will use this port
# - the cache will use port LOCAL_PORT+1
LOCAL_PORT=7200



#################
# Start of script

# Run the Conduit.sh command with the above environment variables set
./Conduit.sh $PAAS_ENV_SHORTNAME $LOCAL_PORT
