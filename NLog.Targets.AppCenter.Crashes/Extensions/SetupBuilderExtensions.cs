using NLog.Config;

namespace NLog.Targets.AppCenter.Crashes
{
    public static class SetupBuilderExtensions
    {
        /// <summary>
        /// Register the NLog.Target for Microsoft.AppCenter.Crashes.
        /// </summary>
        public static ISetupBuilder RegisterAppCenterCrashes(this ISetupBuilder setupBuilder)
        {
            return setupBuilder.SetupExtensions(e => e.RegisterAppCenterCrashes());
        }
    }
}
