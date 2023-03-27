using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.Core.Helpers
{
    public static class FeatureFlagHelper
    {
        public static bool IsFeatureEnabled(FeatureFlag featureFlag)
        {
            bool? flagValue = GetFeatureFlagValue(featureFlag);

            return flagValue.HasValue && flagValue.Value;
        }

        public static bool? GetFeatureFlagValue(FeatureFlag featureFlag)
        {
            string appSettingName = $"FeatureFlag{featureFlag}";

            string appSettingValue = Config.GetAppSetting(appSettingName);

            if (string.IsNullOrEmpty(appSettingValue))
            {
                return null;
            }

            return appSettingValue.ToBoolean();
        }
    }

    public enum FeatureFlag
    {
        PrivateManualRegistration,
        SendRegistrationReviewEmails
    }
}
