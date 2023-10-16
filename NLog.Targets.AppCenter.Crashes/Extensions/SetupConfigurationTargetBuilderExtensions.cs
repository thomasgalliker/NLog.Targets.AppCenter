using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets.AppCenter.Crashes
{
    public static class SetupConfigurationTargetBuilderExtensions
    {
        /// <summary>
        /// Write to AppCenter NLog target.
        /// </summary>
        public static ISetupConfigurationTargetBuilder WriteToAppCenterCrashes(this ISetupConfigurationTargetBuilder configBuilder, Layout layout = null, Layout appSecret = null)
        {
            var logTarget = new AppCenterCrashesTarget();
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
