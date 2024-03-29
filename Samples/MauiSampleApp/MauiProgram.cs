﻿using MauiSampleApp.Utils;
using MauiSampleApp.ViewModels;
using MauiSampleApp.Views;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
                configure.SetMinimumLevel(LogLevel.Trace);
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
            builder.Services.AddSingleton(_ => Preferences.Default);

            var mauiApp = builder.Build();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            return mauiApp;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = ServiceLocator.Current.GetRequiredService<ILogger<MauiApp>>();
            logger.LogError(e.ExceptionObject as Exception, "CurrentDomain_UnhandledException");

            NLog.LogManager.Shutdown();
        }
    }
}