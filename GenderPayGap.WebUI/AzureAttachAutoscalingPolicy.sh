while getopts ":a:m:M:" opt; do
    case $opt in
a) PROTECTED_APP_NAME="$OPTARG"
    ;;
m) MIN_COUNT_INSTANCES="$OPTARG"
    ;;
M) MAX_COUNT_INSTANCES="$OPTARG"
    ;;
\?) echo "Invalid option -$OPTARG" >&2
    ;;
esac
    done

echo "{\"instance_min_count\":${MIN_COUNT_INSTANCES},\"instance_max_count\":${MAX_COUNT_INSTANCES},\"scaling_rules\":[{\"metric_type\":\"throughput\",\"breach_duration_secs\":60,\"threshold\":10,\"operator\":\"<\",\"cool_down_secs\":60,\"adjustment\":\"-1\"},{\"metric_type\":\"throughput\",\"breach_duration_secs\":60,\"threshold\":10,\"operator\":\">=\",\"cool_down_secs\":60,\"adjustment\":\"+1\"}]}" > autoscaling_policy.json

cf install-plugin -f -r CF-Community app-autoscaler-plugin
cf create-service autoscaler autoscaler-free-plan "scale-${PROTECTED_APP_NAME}"
cf bind-service "${PROTECTED_APP_NAME}" "scale-${PROTECTED_APP_NAME}"
cf attach-autoscaling-policy "${PROTECTED_APP_NAME}" autoscaling_policy.json
