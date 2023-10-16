using System;

namespace NLog.Targets.AppCenter
{
    internal static class Constants
    {
        internal const int MaxPropertyCount = 20;

        internal static readonly Type AppCenterCrashesType = Type.GetType("Microsoft.AppCenter.Crashes.Crashes, Microsoft.AppCenter.Crashes");
        internal static readonly Type AppCenterAnalyticsType = Type.GetType("Microsoft.AppCenter.Analytics.Analytics, Microsoft.AppCenter.Analytics");
    }
}
