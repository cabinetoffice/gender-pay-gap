#!/usr/bin/env bash
set -euo pipefail

if ! command -v jq >/dev/null; then
  echo "Must have JQ installed"
  exit 1
fi

while getopts ":a:e:f:r:s:" opt; do
  case $opt in
    a) PROTECTED_APP_NAME="$OPTARG"
    ;;
    e) PROTECTED_APP_SPACE_NAME="$OPTARG"
    ;;
    f) DENIED_IPS_FILENAME="$OPTARG"
    ;;
    r) ROUTE_SERVICE_APP_NAME="$OPTARG"
    ;;
    s) ROUTE_SERVICE_NAME="$OPTARG"
    ;;
    \?) echo "Invalid option -$OPTARG" >&2
    ;;
  esac
done

if [ -z "${ROUTE_SERVICE_APP_NAME+set}" ]; then
  echo "Must provide ROUTE_SERVICE_APP_NAME parameter -r"
  exit 1
fi

if [ -z "${ROUTE_SERVICE_NAME+set}" ]; then
  echo "Must provide ROUTE_SERVICE_NAME parameter -s"
  exit 1
fi

if [ -z "${PROTECTED_APP_NAME+set}" ]; then
  echo "Must provide PROTECTED_APP_NAME parameter -a"
  exit 1
fi

if [ -z "${PROTECTED_APP_SPACE_NAME+set}" ]; then
  echo "Must provide PROTECTED_APP_SPACE_NAME parameter -e"
  exit 1
fi

if [ -z "${DENIED_IPS_FILENAME+set}" ]; then
  echo "Must provide DENIED_IPS_FILENAME parameter -f"
  exit 1
fi

readarray -t IPS < "$DENIED_IPS_FILENAME"

NGINX_DENY_STATEMENTS=""
for addr in "${IPS[@]}";
  do NGINX_DENY_STATEMENTS="$NGINX_DENY_STATEMENTS deny ${addr//[$'\r']};"; true;
done;

APPS_DOMAIN=$(cf curl "v3/domains" | jq -r '[.resources[] | select(.name|endswith("apps.digital"))][0].name')

cf target -s "${PROTECTED_APP_SPACE_NAME}"
cf push "${ROUTE_SERVICE_APP_NAME}" --no-start --var app-name="${ROUTE_SERVICE_APP_NAME}"
cf set-env "${ROUTE_SERVICE_APP_NAME}" DENIED_IPS "$(printf "%s" "${NGINX_DENY_STATEMENTS}")"
cf start "${ROUTE_SERVICE_APP_NAME}"

ROUTE_SERVICE_DOMAIN="$(cf curl "v3/apps/$(cf app "${ROUTE_SERVICE_APP_NAME}" --guid)/routes" | jq -r --arg APPS_DOMAIN "${APPS_DOMAIN}" '[.resources[] | select(.url | endswith($APPS_DOMAIN))][0].url')"

if cf curl "v3/service_instances?type=user-provided&names=${ROUTE_SERVICE_NAME}" | jq -e '.pagination.total_results == 0' > /dev/null; then
  cf create-user-provided-service \
    "${ROUTE_SERVICE_NAME}" \
    -r "https://${ROUTE_SERVICE_DOMAIN}";
else
  cf update-user-provided-service \
    "${ROUTE_SERVICE_NAME}" \
    -r "https://${ROUTE_SERVICE_DOMAIN}";
fi

PROTECTED_APP_HOSTNAME="$(cf curl "v3/apps/$(cf app "${PROTECTED_APP_NAME}" --guid)/routes" | jq -r --arg APPS_DOMAIN "${APPS_DOMAIN}" '[.resources[] | select(.url | endswith($APPS_DOMAIN))][0].host')"

cf bind-route-service "${APPS_DOMAIN}" "${ROUTE_SERVICE_NAME}" --hostname "${PROTECTED_APP_HOSTNAME}";
