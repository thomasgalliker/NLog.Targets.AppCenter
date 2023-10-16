using NLog.Config;

namespace NLog.Targets.AppCenter.Crashes
{
    public static class SetupExtensionsBuilderExtensions
    {
        /// <summary>
        /// Register the NLog.Target for Microsoft.AppCenter.Crashes.
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAppCenterCrashes(this ISetupExtensionsBuilder setupBuilder)
        {
            return setupBuilder.RegisterTarget<AppCenterCrashesTarget>("AppCenterCrashes");
        }
    }
}
