using System.Reflection;
using MauiSampleApp.ViewModels;
using MauiSampleApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Targets.AppCenter.Analytics;
using NLog.Targets.AppCenter.Crashes;

namespace MauiSampleApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder()
                .UseMauiApp<App>();

            builder.Services.AddLogging(configure =>
            {
                configure.ClearProviders();
                configure.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                configure.AddNLog();
            });

            // Setup NLog programmatically
            NLog.LogManager.Configuration = NLogConfig.Configure();

            // Alternatively, setup NLog using fluent API
            //NLog.LogManager.Setup()
            //    .LoadConfigurationFromAssemblyResource(Assembly.GetExecutingAssembly(), "NLog.config")
            //    //.RegisterAppCenterAnalytics()
            //    //.RegisterAppCenterCrashes()
            //    ;

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();

            return builder.Build();
        }
    }
}