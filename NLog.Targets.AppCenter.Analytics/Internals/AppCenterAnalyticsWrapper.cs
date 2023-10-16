using System.Collections.Generic;
using System.Threading.Tasks;

namespace NLog.Targets.AppCenter.Analytics.Internals
{
    internal class AppCenterAnalyticsWrapper : IAppCenterAnalytics
    {
        public Task<bool> IsEnabledAsync()
        {
            return Microsoft.AppCenter.Analytics.Analytics.IsEnabledAsync();
        }

        public Task SetEnabledAsync(bool enabled)
        {
            return Microsoft.AppCenter.Analytics.Analytics.SetEnabledAsync(enabled);
        }

        public void TrackEvent(string name, IDictionary<string, string> properties = null)
        {
            Microsoft.AppCenter.Analytics.Analytics.TrackEvent(name, properties);
        }
    }
}