#!/usr/bin/env bash
set -euo pipefail

while getopts ":a:d:e:f:r:s:m:M:w:" opt; do
  case $opt in
    a) PROTECTED_APP_NAME="$OPTARG"
    ;;
    d) AGENT_TEMP_DIRECTORY="$OPTARG"
    ;;
    e) PROTECTED_APP_SPACE_NAME="$OPTARG"
    ;;
    f) DENIED_IPS_FILENAME="$OPTARG"
    ;;
    r) ROUTE_SERVICE_APP_NAME="$OPTARG"
    ;;
    s) ROUTE_SERVICE_NAME="$OPTARG"
    ;;
    m) MIN_COUNT_INSTANCES="$OPTARG"
    ;;
    M) MAX_COUNT_INSTANCES="$OPTARG"
    ;;
    w) WORKING_DIRECTORY="$OPTARG"
    ;;
    \?) echo "Invalid option -$OPTARG" >&2
    ;;
  esac
done

WORKING_DIRECTORY=${WORKING_DIRECTORY/C://c}
WORKING_DIRECTORY=${WORKING_DIRECTORY//\\//}
AGENT_TEMP_DIRECTORY=${AGENT_TEMP_DIRECTORY/C://c}
AGENT_TEMP_DIRECTORY=${AGENT_TEMP_DIRECTORY//\\//}

# Get the latest version of jq into the working directory
curl -L -o "${WORKING_DIRECTORY}/GPGIPDeny/jq.exe" https://github.com/stedolan/jq/releases/latest/download/jq-win64.exe;

# go into the working directory so that the deployment picks up the nginx.conf
cd "${WORKING_DIRECTORY}/GPGIPDeny"

# replace the instances of jq in the deploy.sh script with the fully specified path of jq
sed -i "s+jq+'${WORKING_DIRECTORY}/GPGIPDeny/jq.exe'+g" ${WORKING_DIRECTORY}/GPGIPDeny/deploy.sh

# Run the deployment
${WORKING_DIRECTORY}/GPGIPDeny/deploy.sh -a ${PROTECTED_APP_NAME} -e ${PROTECTED_APP_SPACE_NAME} -f "${AGENT_TEMP_DIRECTORY}/GPG-IP-Denylist.txt" -r ${ROUTE_SERVICE_APP_NAME} -s ${ROUTE_SERVICE_NAME} -m ${MIN_COUNT_INSTANCES} -M ${MAX_COUNT_INSTANCES}