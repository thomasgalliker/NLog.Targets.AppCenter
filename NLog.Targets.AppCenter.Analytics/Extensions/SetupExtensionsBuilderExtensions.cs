using NLog.Config;

namespace NLog.Targets.AppCenter.Analytics
{
    public static class SetupExtensionsBuilderExtensions
    {
        /// <summary>
        /// Register the NLog.Target for Microsoft.AppCenter.Analytics.
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAppCenterAnalytics(this ISetupExtensionsBuilder setupBuilder)
        {
            return setupBuilder.RegisterTarget<AppCenterAnalyticsTarget>("AppCenterAnalytics");
        }
    }
}
