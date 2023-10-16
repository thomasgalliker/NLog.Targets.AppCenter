using Moq;
using Moq.AutoMock;
using NLog.Targets.AppCenter.Crashes;
using NLog.Targets.AppCenter.Crashes.Internals;
using NLog.Targets.AppCenter.Internals;

namespace NLog.Targets.AppCenter.Tests
{
    public class AppCenterCrashesTargetTests
    {
        private readonly LogFactory logFactory;
        private readonly AutoMocker autoMocker;

        public AppCenterCrashesTargetTests()
        {
            this.autoMocker = new AutoMocker();
            var appCenterMock = this.autoMocker.GetMock<IAppCenter>();
            var appCenterCrashesMock = this.autoMocker.GetMock<IAppCenterCrashes>();

            this.logFactory = new LogFactory().Setup().LoadConfiguration(c =>
            {
                c.ForLogger().WriteTo(new AppCenterCrashesTarget(appCenterMock.Object, appCenterCrashesMock.Object));
            }).LogFactory;
        }

        [Fact]
        public void ShouldTrackError_WithoutProperties()
        {
            // Arrange
            var appCenterMock = this.autoMocker.GetMock<IAppCenter>();
            var appCenterAnalyticsMock = this.autoMocker.GetMock<IAppCenterCrashes>();

            var exception = new InvalidOperationException("Test exception message");

            var logger = this.logFactory.GetCurrentClassLogger();

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
                    d.Count == 1 &&
                    d["Message"] == "Test message")), Times.Once);
            appCenterAnalyticsMock.VerifyNoOtherCalls();
        }
    }
}