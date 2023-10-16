using System.Collections.Generic;
using System.Threading.Tasks;

namespace NLog.Targets.AppCenter.Analytics.Internals
{
    /// <inheritdoc cref="Microsoft.AppCenter.Analytics.Analytics"/>
    internal interface IAppCenterAnalytics
    {
        /// <inheritdoc cref="Microsoft.AppCenter.Analytics.Analytics.IsEnabledAsync()"/>
        Task<bool> IsEnabledAsync();

        /// <inheritdoc cref="Microsoft.AppCenter.Analytics.Analytics.SetEnabledAsync(bool)"/>
        Task SetEnabledAsync(bool enabled);

        /// <inheritdoc cref="Microsoft.AppCenter.Analytics.Analytics.TrackEvent(string, IDictionary{string, string})"/>
        void TrackEvent(string name, IDictionary<string, string> properties = null);
    }
}