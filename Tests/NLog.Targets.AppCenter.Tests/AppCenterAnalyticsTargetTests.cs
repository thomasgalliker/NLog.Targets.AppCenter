using Moq;
using Moq.AutoMock;
using NLog.Targets.AppCenter.Analytics;
using NLog.Targets.AppCenter.Analytics.Internals;
using NLog.Targets.AppCenter.Internals;

namespace NLog.Targets.AppCenter.Tests
{
    public class AppCenterAnalyticsTargetTests
    {
        private readonly LogFactory logFactory;
        private readonly AutoMocker autoMocker;

        public AppCenterAnalyticsTargetTests()
        {
            this.autoMocker = new AutoMocker();
            var appCenterMock = this.autoMocker.GetMock<IAppCenter>();
            var appCenterAnalyticsMock = this.autoMocker.GetMock<IAppCenterAnalytics>();

            this.logFactory = new LogFactory().Setup().LoadConfiguration(c =>
            {
                c.ForLogger().WriteTo(new AppCenterAnalyticsTarget(appCenterMock.Object, appCenterAnalyticsMock.Object));
            }).LogFactory;
        }
        
        [Fact]
        public void ShouldTrackEvent_WithoutProperties()
        {
            // Arrange
            var appCenterMock = this.autoMocker.GetMock<IAppCenter>();
            var appCenterAnalyticsMock = this.autoMocker.GetMock<IAppCenterAnalytics>();

            var logger = this.logFactory.GetCurrentClassLogger();

            // Act
            logger.Info("Test message");

            // Assert
            appCenterMock.VerifyGet(a => a.Configured, Times.Once);
            appCenterMock.VerifyNoOtherCalls();

            appCenterAnalyticsMock.Verify(a => a.IsEnabledAsync(), Times.Once);
            appCenterAnalyticsMock.Verify(a => a.SetEnabledAsync(true), Times.Once);
            appCenterAnalyticsMock.Verify(a => a.TrackEvent("Test message", It.Is<Dictionary<string, string>>(d => d.Count == 0)), Times.Once);
            appCenterAnalyticsMock.VerifyNoOtherCalls();
        }
    }
}