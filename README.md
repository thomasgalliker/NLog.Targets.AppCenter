# NLog.Targets for Microsoft AppCenter
[![Version](https://img.shields.io/nuget/v/NLog.Targets.AppCenter.Analytics.svg)](https://www.nuget.org/packages/NLog.Targets.AppCenter.Analytics)  [![Downloads](https://img.shields.io/nuget/dt/NLog.Targets.AppCenter.Analytics.svg)](https://www.nuget.org/packages/NLog.Targets.AppCenter.Analytics)
[![Downloads](https://img.shields.io/nuget/dt/NLog.Targets.AppCenter.Crashes.svg)](https://www.nuget.org/packages/NLog.Targets.AppCenter.Crashes)

NLog targets for Microsoft AppCenter.

### Download and Install
 There are two separate libraries: NLog.Targets.AppCenter.Analytics for user tracking via NLog loggers and NLog.Targets.AppCenter.Crashes for error logging and crash reporting. The libraries are available on NuGet.org.
Use the following command to install the NuGet packages using NuGet package manager console:

    PM> Install-Package NLog.Targets.AppCenter.Analytics
    PM> Install-Package NLog.Targets.AppCenter.Crashes

You can use these libraries in any .NET project compatible to .NET Standard 2.0 and higher.

### Setup NLog AppCenter Analytics/Crashes in .NET MAUI
1) In your `MauiProgram.cs`, redirect Microsoft.Extensions.Logging loggers to NLog by adding `AddNLog();`. The rest of the logging configuration may remain the same as it is.
    ```csharp
    builder.Services.AddLogging(configure =>
    {
        configure.ClearProviders();
        configure.SetMinimumLevel(LogLevel.Trace);
        configure.AddNLog();
    });
    ```
2) Configure NLog targets either via C# code or with an NLog.config file. For both ways you can find a sample configuration in the sample MAUI app provided in this repository (see Samples/MauiSampleApp/MauiProgram.cs).
   You can configure both NLog targets, AppCenterAnalyticsTarget and AppCenterCrashesTarget, independently. Create a new instance of the target, set all required configuration properties and add the target to the NLog `LoggingConfiguration`.
   Finally, set a logging rule for the new NLog target with an appropriate minimum log level. 

    ```csharp
    var appCenterCrashesTarget = new AppCenterCrashesTarget();
    appCenterCrashesTarget.AppSecret = "android=....;ios=....";
    appCenterCrashesTarget.ServiceTypes = new[] 
    {
        typeof(Microsoft.AppCenter.Crashes.Crashes)
    };
    // ...
    appCenterCrashesTarget.ContextProperties.Add(new TargetPropertyWithContext
    {
        Name = "date",
        Layout = "${longdate:universalTime=True}"
    });
    // ...

    config.AddTarget("AppCenterCrashesTarget", appCenterCrashesTarget);
    var appCenterCrashesRule = new LoggingRule("*", LogLevel.Warn, appCenterCrashesTarget);
    config.LoggingRules.Add(appCenterCrashesRule);
    ```
    
### Configuration options for `AppCenterAnalyticsTarget`
- `AppSecret`: The app secret for starting AppCenter, e.g. "android={Your Android app secret here};ios={Your iOS app secret here}".
- `ServiceTypes`: The service type(s) to be used when calling. Default: typeof(Microsoft.AppCenter.Crashes.Crashes).
- `UserId`: Application UserId to register in AppCenter (optional).
- `LogUrl`: Base URL (scheme + authority + port only) to the AppCenter-backend (optional).
- `CountryCode`: Two-letter ISO country code to send to the AppCenter-backend (optional).
- `TrackOnlyIfMessageStartsWith`: Tracks messages only if they're start with this prefix, e.g. "[Track]" (optional).

### Configuration options for `AppCenterCrashesTarget`
- `AppSecret`: The app secret for starting AppCenter, e.g. "android={Your Android app secret here};ios={Your iOS app secret here}".
- `ServiceTypes`: The service type(s) to be used when calling. Default: typeof(Microsoft.AppCenter.Crashes.Crashes).
- `UserId`: Application UserId to register in AppCenter (optional).
- `LogUrl`: Base URL (scheme + authority + port only) to the AppCenter-backend (optional).
- `CountryCode`: Two-letter ISO country code to send to the AppCenter-backend (optional).
- `WrapExceptionFromLevel`: Tracks all log messages which don't contain exception parameter but have a log level equal or higher than the configured value.
- `AttachmentsDirectory`: The path of the attachments directory. If set, files in this directory will automatically be attached in the error report sent to AppCenter.
- `AttachmentsFilePattern`: File search pattern within the configured AttachmentsDirectory. Default: "*" (All files included).

### Good to know
- If you struggle to get things up and running, have a look at the sample app in this repository. I contains a very *close-to-production* configuration.
- If you don't want these NLog targets to start AppCenter automatically, just don't set an AppSecret in the configurations. It will be in your hands to start AppCenter with the correct app secret then.
- If you only want to use one of the NLog targets (e.g. `AppCenterCrashesTarget`) but you want to use AppCenter user tracking elsewhere in your app, configure ServiceTypes with both service types (`typeof(Microsoft.AppCenter.Analytics.Analytics)` and `typeof(Microsoft.AppCenter.Crashes.Crashes`) so that AppCenter is started with both services. AppCenter cannot be reconfigured with new service types after it's been started.

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a new issue on github.com.
