using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;

namespace NLog.Targets.AppCenter.Crashes.Internals
{
    /// <inheritdoc cref="Microsoft.AppCenter.Crashes.Crashes"/>
    internal interface IAppCenterCrashes
    {
        /// <inheritdoc cref="Microsoft.AppCenter.Crashes.Crashes.IsEnabledAsync()"/>
        Task<bool> IsEnabledAsync();

        /// <inheritdoc cref="Microsoft.AppCenter.Crashes.Crashes.SetEnabledAsync(bool)"/>
        Task SetEnabledAsync(bool enabled);

        /// <inheritdoc cref="Microsoft.AppCenter.Crashes.Crashes.TrackError(Exception, IDictionary{string, string}, ErrorAttachmentLog[])"/>
        void TrackError(Exception exception, IDictionary<string, string> properties = null, params ErrorAttachmentLog[] attachments);
    }
}