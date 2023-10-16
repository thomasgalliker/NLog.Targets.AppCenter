using Moq;
using Moq.AutoMock;
using NLog.Targets.AppCenter.Crashes;
using NLog.Targets.AppCenter.Crashes.Internals;
using NLog.Targets.AppCenter.Internals;

namespace NLog.Targets.AppCenter.Tests
{
    public class AppCenterCrashesTargetTests
    {
        private readonly AutoMocker autoMocker;

        public AppCenterCrashesTargetTests()
        {
            this.autoMocker = new AutoMocker();
        }

        [Fact]
        public void ShouldTrackError_WithoutProperties()
        {
            // Arrange
            var appCenterMock = this.autoMocker.GetMock<IAppCenter>();
            var appCenterAnalyticsMock = this.autoMocker.GetMock<IAppCenterCrashes>();

            var exception = new InvalidOperationException("Test exception message");

            var appCenterCrashesTarget = this.autoMocker.CreateInstance<AppCenterCrashesTarget>(enablePrivate: true);
            var logger = CreateLoggerWithTarget(appCenterCrashesTarget);

            // Act
            logger.Error(exception, "Test message");

            // Assert
            appCenterMock.VerifyGet(a => a.Configured, Times.Once);
            appCenterMock.VerifyNoOtherCalls();

            appCenterAnalyticsMock.Verify(a => a.IsEnabledAsync(), Times.Once);
            appCenterAnalyticsMock.Verify(a => a.SetEnabledAsync(true), Times.Once);
            appCenterAnalyticsMock.Verify(a => a.TrackError(
                exception,
                It.Is<Dictionary<string, string>>(d => d.Count == 0)), Times.Once);
            appCenterAnalyticsMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldTrackError_WithProperties()
        {
            // Arrange
            var appCenterMock = this.autoMocker.GetMock<IAppCenter>();
            var appCenterCrashesMock = this.autoMocker.GetMock<IAppCenterCrashes>();
            var appCenterAnalyticsMock = this.autoMocker.GetMock<IAppCenterCrashes>();

            var appCenterCrashesTarget = this.autoMocker.CreateInstance<AppCenterCrashesTarget>(enablePrivate: true);
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

            var logger = CreateLoggerWithTarget(appCenterCrashesTarget);
            var exception = new InvalidOperationException("Test exception message");

            // Act
            logger.Error(exception, "Test message");

            // Assert
            appCenterMock.VerifyGet(a => a.Configured, Times.Once);
            appCenterMock.VerifyNoOtherCalls();

            appCenterAnalyticsMock.Verify(a => a.IsEnabledAsync(), Times.Once);
            appCenterAnalyticsMock.Verify(a => a.SetEnabledAsync(true), Times.Once);
            appCenterAnalyticsMock.Verify(a => a.TrackError(
                exception,
                It.Is<Dictionary<string, string>>(d =>
                    d.Count == 2 &&
                    d["exception.Type"] == exception.GetType().FullName &&
                    d["exception.Message"] == exception.Message)), Times.Once);
            appCenterAnalyticsMock.VerifyNoOtherCalls();
        }

        private static ILogger CreateLoggerWithTarget(Target target)
        {
            var logFactory = new LogFactory().Setup().LoadConfiguration(c =>
            {
                c.ForLogger().WriteTo(target);
            }).LogFactory;

            var logger = logFactory.GetCurrentClassLogger();
            return logger;
        }
    }
}