using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.AppCenter.Analytics.Internals;
using NLog.Targets.AppCenter.Internals;

namespace NLog.Targets.AppCenter.Analytics
{
    /// <summary>
    /// NLog.Target for Microsoft.AppCenter.Analytics.
    /// </summary>
    [Target("AppCenterAnalytics")]
    public class AppCenterAnalyticsTarget : TargetWithContext
    {
        private readonly IAppCenter appCenter;
        private readonly IAppCenterAnalytics appCenterAnalytics;

        /// <summary>
        /// The app secret for starting AppCenter, e.g. "android={Your Android app secret here};ios={Your iOS app secret here}".
        /// </summary>
        /// <remarks>
        /// If you don't provide an app secret, AppCenter is not started automatically.
        /// In order to use this log target, you'll be required to call AppCenter.Start at the startup.
        /// </remarks>
        public Layout AppSecret { get; set; }

        /// <inheritdoc cref="ServiceTypes"/>
        /// <remarks>
        /// Use comma-separated type names, e.g. "Analytics, Crashes" 
        /// or fully-qualified type names "Microsoft.AppCenter.Analytics.Analytics, Microsoft.AppCenter.Analytics.Crashes".
        /// </remarks>
        public Layout ServiceTypesString { get; set; }

        /// <summary>
        /// The service type(s) to be used when calling <see cref="Microsoft.AppCenter.AppCenter.Start"/>.
        /// AppCenter can only be started once, so pass all service types needed during the lifetime of the process.
        /// Default: typeof(Microsoft.AppCenter.Analytics.Analytics)
        /// </summary>
        public Type[] ServiceTypes { get; set; }

        /// <inheritdoc cref="Microsoft.AppCenter.AppCenter.SetUserId(string)"/>
        public Layout UserId { get; set; }

        /// <inheritdoc cref="Microsoft.AppCenter.AppCenter.SetLogUrl"/>
        public Layout LogUrl { get; set; }

        /// <inheritdoc cref="Microsoft.AppCenter.AppCenter.SetCountryCode(string)"/>
        public Layout CountryCode { get; set; }

        /// <summary>
        /// Tracks messages only if they're start with this prefix, e.g. [Track].
        /// </summary>
        public string TrackOnlyIfMessageStartsWith { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterAnalyticsTarget" /> class.
        /// </summary>
        public AppCenterAnalyticsTarget()
            : this(new AppCenterWrapper(), new AppCenterAnalyticsWrapper())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterAnalyticsTarget" /> class.
        /// </summary>
        public AppCenterAnalyticsTarget(string name)
            : this(new AppCenterWrapper(), new AppCenterAnalyticsWrapper())
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterAnalyticsTarget" /> class.
        /// </summary>
        internal AppCenterAnalyticsTarget(IAppCenter appCenter, IAppCenterAnalytics appCenterAnalytics)
        {
            this.appCenter = appCenter;
            this.appCenterAnalytics = appCenterAnalytics;
            this.Layout = "${message}";          // MaxEventNameLength = 256 (Automatically truncated by Analytics)
            this.IncludeEventProperties = true;  // maximum item count = 20 (Automatically truncated by Analytics)
        }

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            try
            {
                if (!this.appCenter.Configured)
                {
                    var appSecret = this.RenderLogEvent(this.AppSecret, LogEventInfo.CreateNullEvent());
                    if (!string.IsNullOrEmpty(appSecret))
                    {
                        var serviceTypes = this.ServiceTypes;
                        if (serviceTypes == null || !serviceTypes.Any())
                        {
                            serviceTypes = this.RenderLogEvent(this.ServiceTypesString, LogEventInfo.CreateNullEvent())
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s =>
                                {
                                    if (s.EndsWith("Analytics", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        return Constants.AppCenterAnalyticsType;
                                    }

                                    if (s.EndsWith("Crashes", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        return Constants.AppCenterCrashesType;
                                    }

                                    return null;
                                })
                                .Where(t => t != null)
                                .ToArray();
                        }

                        if (!serviceTypes.Any())
                        {
                            serviceTypes = new[] { Constants.AppCenterAnalyticsType };
                        }

                        InternalLogger.Debug($"AppCenterAnalyticsTarget(Name={this.Name}): Starting service types [{serviceTypes.Select(t => t.FullName)}]");
                        Microsoft.AppCenter.AppCenter.Start(appSecret, serviceTypes);
                    }
                    else
                    {
                        InternalLogger.Debug("AppCenterAnalyticsTarget(Name={0}): AppSecret for Microsoft.AppCenter.Analytics.Analytics is not configured", this.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AppCenterAnalyticsTarget(Name={0}): Failed to start Microsoft.AppCenter.Analytics.Analytics", this.Name);
                throw;
            }

            try
            {
                if (!this.appCenterAnalytics.IsEnabledAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    this.appCenterAnalytics.SetEnabledAsync(true).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AppCenterAnalyticsTarget(Name={0}): Failed to enable AppCenter.Analytics", this.Name);
                throw;
            }

            var userId = this.RenderLogEvent(this.UserId, LogEventInfo.CreateNullEvent());
            if (!string.IsNullOrEmpty(userId))
            {
                Microsoft.AppCenter.AppCenter.SetUserId(userId);
            }

            var logUrl = this.RenderLogEvent(this.LogUrl, LogEventInfo.CreateNullEvent());
            if (!string.IsNullOrEmpty(logUrl))
            {
                Microsoft.AppCenter.AppCenter.SetLogUrl(logUrl);
            }

            var countryCode = this.RenderLogEvent(this.CountryCode, LogEventInfo.CreateNullEvent());
            if (!string.IsNullOrEmpty(countryCode))
            {
                Microsoft.AppCenter.AppCenter.SetCountryCode(countryCode);
            }
        }

        /// <inheritdoc />
        protected override void Write(LogEventInfo logEvent)
        {
            if (this.TrackOnlyIfMessageStartsWith is not string messageStartsWith ||
                logEvent.Message.StartsWith(messageStartsWith, StringComparison.InvariantCultureIgnoreCase))
            {
                var message = this.RenderLogEvent(this.Layout, logEvent);
                if (string.IsNullOrWhiteSpace(message))
                {
                    // Avoid event being discarded when name is null or empty
                    if (logEvent.Exception != null)
                    {
                        message = logEvent.Exception.GetType().Name;
                    }
                    else
                    {
                        message = nameof(AppCenterAnalyticsTarget);
                    }
                }

                var properties = this.BuildProperties(logEvent);
                this.appCenterAnalytics.TrackEvent(message, properties);
            }
        }

        /// <remarks>
        ///     The name parameter can not be null or empty, Maximum allowed length = 256.
        ///     The properties parameter maximum item count = 20.
        ///     The properties keys/names can not be null or empty, maximum allowed key length = 125.
        ///     The properties values can not be null, maximum allowed value length = 125.
        /// </remarks>
        private IDictionary<string, string> BuildProperties(LogEventInfo logEvent)
        {
            IDictionary<string, string> properties;

            if (this.ShouldIncludeProperties(logEvent))
            {
                properties = this.GetAllProperties(logEvent).ToDictionary(d => d.Key, d => $"{d.Value}");
            }
            else if (this.ContextProperties?.Count > 0)
            {
                properties = new Dictionary<string, string>(this.ContextProperties.Count, StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < this.ContextProperties.Count; ++i)
                {
                    var contextProperty = this.ContextProperties[i];
                    if (string.IsNullOrEmpty(contextProperty.Name))
                    {
                        continue;
                    }

                    var contextValue = this.RenderLogEvent(contextProperty.Layout, logEvent);
                    if (!contextProperty.IncludeEmptyValue && string.IsNullOrEmpty(contextValue))
                    {
                        continue;
                    }

                    properties[contextProperty.Name] = contextValue;
                }
            }
            else
            {
                properties = new Dictionary<string, string>(0);
            }

            return properties;
        }
    }
}
