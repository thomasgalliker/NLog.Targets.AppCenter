using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;

namespace NLog.Targets.AppCenter.Crashes.Internals
{
    internal class AppCenterCrashesWrapper : IAppCenterCrashes
    {
        public Task<bool> IsEnabledAsync()
        {
            return Microsoft.AppCenter.Crashes.Crashes.IsEnabledAsync();
        }

        public Task SetEnabledAsync(bool enabled)
        {
            return Microsoft.AppCenter.Crashes.Crashes.SetEnabledAsync(enabled);
        }

        public void TrackError(Exception exception, IDictionary<string, string> properties = null, params ErrorAttachmentLog[] attachments)
        {
            Microsoft.AppCenter.Crashes.Crashes.TrackError(exception, properties, attachments);
        }
    }
}