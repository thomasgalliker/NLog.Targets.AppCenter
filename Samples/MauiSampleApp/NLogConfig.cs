using NLog;
using NLog.Config;
using NLog.Filters;
using NLog.Targets;
using NLog.Targets.AppCenter.Analytics;
using NLog.Targets.AppCenter.Crashes;
using LogLevel = NLog.LogLevel;

namespace MauiSampleApp
{
    public static class NLogConfig
    {
        public static LoggingConfiguration Configure()
        {
            LogFilePath = CreateLogFile();
            return GetLoggingConfiguration(LogFilePath);
        }

        public static string LogFilePath { get; set; }

        private static string CreateLogFile()
        {
            var filename = $"{AppInfo.PackageName}.log";
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(folder))
            {
                folder = Directory.CreateDirectory(folder).FullName;
            }

            var filePath = Path.Combine(folder, filename);
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }

            return filePath;
        }

        private static LoggingConfiguration GetLoggingConfiguration(string logFilePath)
        {
            var config = new LoggingConfiguration();
            var layout = "${longdate:universalTime=True}|${level}|${logger}|${message}|${exception:format=tostring}[EOL]";

            // Console Target
            var consoleTarget = new ConsoleTarget();
            consoleTarget.Layout = layout;
            config.AddTarget("console", consoleTarget);

            var consoleRule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(consoleRule);

            // File Target
            var fileTarget = new FileTarget();
            fileTarget.FileName = logFilePath;
            fileTarget.Layout = layout;
            fileTarget.MaxArchiveFiles = 1;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
            fileTarget.ArchiveAboveSize = 10485760; // 10MB
            fileTarget.ConcurrentWrites = true;
            fileTarget.KeepFileOpen = false;
            config.AddTarget("file", fileTarget);

            var fileRule = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(fileRule);

            // AppCenterAnalyticsTarget
            var appSecret = Secrets.AppCenterApiKey;
            var serviceTypes = new[] 
            {
                typeof(Microsoft.AppCenter.Analytics.Analytics),
                typeof(Microsoft.AppCenter.Crashes.Crashes)
            };

            var appCenterAnalyticsTarget = new AppCenterAnalyticsTarget();
            appCenterAnalyticsTarget.AppSecret = appSecret;
            appCenterAnalyticsTarget.ServiceTypes = serviceTypes;
            appCenterAnalyticsTarget.ContextProperties.Add(new TargetPropertyWithContext
            {
                Name = "date",
                Layout = "${longdate:universalTime=True}"
            });
            appCenterAnalyticsTarget.ContextProperties.Add(new TargetPropertyWithContext
            {
                Name = "logger",
                Layout = "${logger}"
            });
            appCenterAnalyticsTarget.ContextProperties.Add(new TargetPropertyWithContext
            {
                Name = "loglevel",
                Layout = "${level}"
            });
            config.AddTarget("AppCenterAnalyticsTarget", appCenterAnalyticsTarget);

            var appCenterAnalyticsRule = new LoggingRule("*", LogLevel.Info, appCenterAnalyticsTarget);
            config.LoggingRules.Add(appCenterAnalyticsRule);

            // AppCenterCrashesTarget
            var appCenterCrashesTarget = new AppCenterCrashesTarget();
            appCenterCrashesTarget.AppSecret = appSecret;
            appCenterCrashesTarget.ServiceTypes = serviceTypes;
            appCenterCrashesTarget.WrapExceptionFromLevel = LogLevel.Warn;
            appCenterCrashesTarget.ContextProperties.Add(new TargetPropertyWithContext
            {
                Name = "date",
                Layout = "${longdate:universalTime=True}"
            });
            appCenterCrashesTarget.ContextProperties.Add(new TargetPropertyWithContext
            {
                Name = "logger",
                Layout = "${logger}"
            });
            appCenterCrashesTarget.ContextProperties.Add(new TargetPropertyWithContext
            {
                Name = "loglevel",
                Layout = "${level}"
            });
            appCenterCrashesTarget.ContextProperties.Add(new TargetPropertyWithContext
            {
                Name = "exception.Type",
                Layout = "${exception:format=type}"
            });
            appCenterCrashesTarget.ContextProperties.Add(new TargetPropertyWithContext
            {
                Name = "exception.Message",
                Layout = "${exception:format=message}"
            });

            config.AddTarget("AppCenterCrashesTarget", appCenterCrashesTarget);

            var ignoredExceptionTypes = new[] {
                typeof(TaskCanceledException),
            };
            var appCenterCrashesRule = new LoggingRule("*", LogLevel.Warn, appCenterCrashesTarget);
            appCenterCrashesRule.Filters.Add(
                new WhenMethodFilter((l) => l.Exception is Exception ex && ignoredExceptionTypes.Contains(ex.GetType()) 
                    ? FilterResult.IgnoreFinal 
                    : FilterResult.Log));

            config.LoggingRules.Add(appCenterCrashesRule);

            return config;
        }
    }
}