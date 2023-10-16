using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets.AppCenter.Analytics
{
    public static class SetupConfigurationTargetBuilderExtensions
    {
        /// <summary>
        /// Write to AppCenter NLog target.
        /// </summary>
        public static ISetupConfigurationTargetBuilder WriteToAppCenterAnalytics(this ISetupConfigurationTargetBuilder configBuilder, Layout layout = null, Layout appSecret = null)
        {
            var logTarget = new AppCenterAnalyticsTarget();
            if (layout != null)
            {
                logTarget.Layout = layout;
            }

            if (appSecret != null)
            {
                logTarget.AppSecret = appSecret;
            }

            return configBuilder.WriteTo(logTarget);
        }
    }
}
