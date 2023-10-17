using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
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
        private readonly IDirectoryInfoFactory directoryInfoFactory;

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
        /// The service type(s) to be used when calling <see cref="Microsoft.AppCenter.AppCenter.Start(string, Type[])"/>.
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
        /// The path of the attachments directory.
        /// If set, files in this directory will automatically be attached
        /// in the error report sent to AppCenter.
        /// 
        /// Get or set the path to a directory to zip and attach to AppCenter-Crashes
        /// No more than 10 files will be attached
        /// Files bigger than 1mb after (compressed) will be ignored
        /// </summary>
        /// <remarks>
        /// Limitations by AppCenter:
        /// - Maximum 10 files can be attached per report.
        /// - If a file exceeds 1MB in size, it will be ignored.
        /// </remarks>
        public Layout AttachmentsDirectory { get; set; }

        /// <summary>
        /// File search pattern within the configured directory <see cref="AttachmentsDirectory"/>.
        /// Default: "*" (All files included)
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="bullet">
        /// <item>
        /// <term>"*.log"</term>
        /// <description>Includes all files with file extension .log.</description>
        /// </item>
        /// <item>
        /// <term>"*"</term>
        /// <description>Includes all files.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public Layout AttachmentsFilePattern { get; set; }

        /// <summary>
        /// Enables zip compression of each attachment file in <see cref="AttachmentsDirectory"/>
        /// before they're sent to AppCenter.
        /// </summary>
        ////public Layout<bool> ZipAttachments { get; set; }

        /// <summary>
        /// If set, NLog.LogManager.Flush() is called before files are attached and sent to TrackError method.
        /// WARNING: This is an experimental feature. If your log files in AppCenter do not contain the stacktraces
        /// from exceptions which caused the TrackError call, try to flush the NLog.LogManager by setting a timeout value.
        /// </summary>
        public TimeSpan? LogManagerFlushTimeout { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterCrashesTarget" /> class.
        /// </summary>
        public AppCenterCrashesTarget()
            : this(new AppCenterWrapper(), new AppCenterCrashesWrapper(), new DirectoryInfoFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterCrashesTarget" /> class.
        /// </summary>
        public AppCenterCrashesTarget(string name)
            : this(new AppCenterWrapper(), new AppCenterCrashesWrapper(), new DirectoryInfoFactory())
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterCrashesTarget" /> class.
        /// </summary>
        internal AppCenterCrashesTarget(
            IAppCenter appCenter,
            IAppCenterCrashes appCenterCrashes,
            IDirectoryInfoFactory directoryInfoFactory)
        {
            this.appCenter = appCenter;
            this.appCenterCrashes = appCenterCrashes;
            this.directoryInfoFactory = directoryInfoFactory;

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

                        Microsoft.AppCenter.Crashes.Crashes.ShouldProcessErrorReport = errorReport =>
                        {
                            return true;
                        };

                        Microsoft.AppCenter.Crashes.Crashes.ShouldAwaitUserConfirmation = () =>
                        {
                            return false;
                        };

                        Microsoft.AppCenter.Crashes.Crashes.GetErrorAttachments = errorReport =>
                        {
                            var logEventInfo = LogEventInfo.Create(LogLevel.Error, nameof(AppCenterCrashesTarget), nameof(Microsoft.AppCenter.Crashes.Crashes.GetErrorAttachments));
                            return this.GetErrorAttachmentWithFromLogFiles(logEventInfo);
                        };

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
            var attachments = this.GetErrorAttachmentWithFromLogFiles(logEvent).ToArray();
            var properties = this.BuildProperties(logEvent);
            this.appCenterCrashes.TrackError(exception, properties, attachments);
        }

        private IEnumerable<ErrorAttachmentLog> GetErrorAttachmentWithFromLogFiles(LogEventInfo logEvent)
        {
            var path = this.RenderLogEvent(this.AttachmentsDirectory, logEvent);
            if (string.IsNullOrEmpty(path))
            {
                yield break;
            }

            var directoryInfo = this.directoryInfoFactory.FromPath(path);
            if (!directoryInfo.Exists)
            {
                yield break;
            }

            var searchPattern = this.RenderLogEvent(this.AttachmentsFilePattern, logEvent);
            if (string.IsNullOrWhiteSpace(searchPattern))
            {
                searchPattern = "*";
            }

            var files = directoryInfo.EnumerateFiles(searchPattern, SearchOption.AllDirectories).ToArray();
            if (files.Any())
            {
                // Experimental:
                // Before we can get any log files, we have to make sure
                // the file log target(s) have been flushed.
                if (this.LogManagerFlushTimeout is TimeSpan timeout && timeout > TimeSpan.Zero)
                {
                    LogManager.Flush(timeout);
                }
            }

            foreach (var file in files)
            {
                yield return this.GetErrorAttachmentWithFromLogFile(file);
            }
        }

        private static readonly MimeTypeMapping DefaultMimeTypeMapping = new MimeTypeMapping("*", "application/octet-stream", false);

        private static readonly MimeTypeMapping[] DefaultMimeTypeMappings = new[]
        {
            new MimeTypeMapping(".txt", "text/plain", true),
            new MimeTypeMapping(".log", "text/plain", true),
            new MimeTypeMapping(".zip", "application/zip", false),
            new MimeTypeMapping(".gz", "application/x-zip-compressed", false),
            new MimeTypeMapping(".rar", "application/x-rar-compressed", false),
        };

        private MimeTypeMapping GetMimeTypeMapping(IFileInfo fileInfo)
        {
            foreach (var mapping in DefaultMimeTypeMappings)
            {
                if (mapping.FileExtension == fileInfo.Extension)
                {
                    return mapping;
                }
            }

            return DefaultMimeTypeMapping;
        }

        private ErrorAttachmentLog GetErrorAttachmentWithFromLogFile(IFileInfo fileInfo)
        {
            try
            {
                var fileContent = GetFileContent(fileInfo);
                var mimeTypeMapping = this.GetMimeTypeMapping(fileInfo);
                if (mimeTypeMapping.IsText)
                {
                    var logFileContent = Encoding.UTF8.GetString(fileContent);
                    return ErrorAttachmentLog.AttachmentWithText(logFileContent, fileInfo.Name);
                }
                else
                {
                    return ErrorAttachmentLog.AttachmentWithBinary(fileContent, fileInfo.Name, mimeTypeMapping.MimeType);
                }
            }
            catch (Exception ex)
            {
                return ErrorAttachmentLog.AttachmentWithText(
                    $"{nameof(GetErrorAttachmentWithFromLogFile)} for file '{fileInfo.FullName}' " +
                    $"failed with exception: {ex}", fileInfo.Name);
            }
        }

        private static byte[] GetFileContent(IFileInfo fileInfo)
        {
            byte[] data;
            using (var originalFileStream = fileInfo.OpenRead())
            {
                using (var memoryStream = new MemoryStream())
                {
                    originalFileStream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }
            }

            return data;
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
