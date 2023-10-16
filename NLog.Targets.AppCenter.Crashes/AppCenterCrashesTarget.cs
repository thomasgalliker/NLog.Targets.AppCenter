using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.AppCenter.Crashes.Internals;
using NLog.Targets.AppCenter.Internals;

namespace NLog.Targets.AppCenter.Crashes
{
    /// <summary>
    /// NLog.Target for Microsoft.AppCenter.Crashes.
    /// </summary>
    [Target("AppCenterCrashes")]
    public class AppCenterCrashesTarget : TargetWithContext
    {
        private readonly IAppCenter appCenter;
        private readonly IAppCenterCrashes appCenterCrashes;

        /// <summary>
        /// The app secret for starting AppCenter, e.g. "android={your Android app secret here};ios={your iOS app secret here}".
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
        /// Default: typeof(Microsoft.AppCenter.Crashes.Crashes)
        /// </summary>
        public Type[] ServiceTypes { get; set; }

        /// <summary>
        /// Get or set the application UserId to register in AppCenter (optional)
        /// </summary>
        public Layout UserId { get; set; }

        /// <summary>
        /// Get or set the base URL (scheme + authority + port only) used to communicate with the backend (optional)
        /// </summary>
        /// <remarks>
        /// Example "http://nginx:port"
        /// </remarks>
        public Layout LogUrl { get; set; }

        /// <summary>
        /// Get or set two-letter ISO country code to send to the backend (optional)
        /// </summary>
        public Layout CountryCode { get; set; }

        /// <summary>
        /// Tracks all log messages which don't contain exception parameter
        /// but have a log level equal or higher than the configured value.
        /// </summary>
        public LogLevel WrapExceptionFromLevel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterCrashesTarget" /> class.
        /// </summary>
        public AppCenterCrashesTarget()
            : this(new AppCenterWrapper(), new AppCenterCrashesWrapper())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterCrashesTarget" /> class.
        /// </summary>
        public AppCenterCrashesTarget(string name)
            : this(new AppCenterWrapper(), new AppCenterCrashesWrapper())
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterCrashesTarget" /> class.
        /// </summary>
        internal AppCenterCrashesTarget(IAppCenter appCenter, IAppCenterCrashes appCenterCrashes)
        {
            this.appCenter = appCenter;
            this.appCenterCrashes = appCenterCrashes;
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
                            serviceTypes = new[] { Constants.AppCenterCrashesType };
                        }

                        InternalLogger.Debug($"AppCenterCrashesTarget(Name={this.Name}): Starting service types [{serviceTypes.Select(t => t.FullName)}]");
                        Microsoft.AppCenter.AppCenter.Start(appSecret, serviceTypes);
                    }
                    else
                    {
                        InternalLogger.Debug("AppCenterCrashesTarget(Name={0}): AppSecret for Microsoft.AppCenter.Analytics.Analytics is not configured", this.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AppCenterCrashesTarget(Name={0}): Failed to start AppCenter", this.Name);
                throw;
            }

            try
            {
                if (!this.appCenterCrashes.IsEnabledAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    this.appCenterCrashes.SetEnabledAsync(true).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AppCenterCrashesTarget(Name={0}): Failed to enable Microsoft.AppCenter.Crashes.Crashes", this.Name);
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
            if (logEvent.Exception is Exception exception)
            {
                this.TrackError(logEvent, exception);
            }
            else if (this.WrapExceptionFromLevel is LogLevel wrapLevel && logEvent.Level >= wrapLevel)
            {
                exception = new Exception(logEvent.Message);
                this.TrackError(logEvent, exception);
            }
        }

        private void TrackError(LogEventInfo logEvent, Exception exception)
        {
            var properties = this.BuildProperties(logEvent);
            this.appCenterCrashes.TrackError(exception, properties);
        }

        /// <remarks>
        ///     The name parameter can not be null or empty, Maximum allowed length = 256.
        ///     The properties parameter maximum item count, see <see cref="MaxPropertyCount"/>.
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
