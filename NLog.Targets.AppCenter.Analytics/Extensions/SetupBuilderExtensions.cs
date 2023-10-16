using NLog.Config;

namespace NLog.Targets.AppCenter.Analytics
{
    public static class SetupBuilderExtensions
    {
        /// <summary>
        /// Register the NLog.Target for Microsoft.AppCenter.Analytics.
        /// </summary>
        public static ISetupBuilder RegisterAppCenterAnalytics(this ISetupBuilder setupBuilder)
        {
            return setupBuilder.SetupExtensions(e => e.RegisterAppCenterAnalytics());
        }
    }
}
