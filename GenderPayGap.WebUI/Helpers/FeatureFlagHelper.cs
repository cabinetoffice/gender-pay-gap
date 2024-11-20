using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebUI.Helpers
{
    public static class FeatureFlagHelper
    {
        public static bool IsFeatureEnabled(FeatureFlag featureFlag)
        {
            string appSettingName = $"FeatureFlag{featureFlag}";
            return Config.GetAppSettingBool(appSettingName, defaultValue: false);
        }

    }

    public enum FeatureFlag
    {
        PrivateManualRegistration,
        SendRegistrationReviewEmails
    }
}
