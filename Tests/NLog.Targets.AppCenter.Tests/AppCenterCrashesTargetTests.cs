using System.Text;
using Microsoft.AppCenter.Crashes;
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

        [Fact]
        public void ShouldTrackError_WithAttachments_TextFile()
        {
            // Arrange
            var appCenterCrashesMock = this.autoMocker.GetMock<IAppCenterCrashes>();
            var appCenterAnalyticsMock = this.autoMocker.GetMock<IAppCenterCrashes>();
            var directoryInfoFactoryMock = this.autoMocker.GetMock<IDirectoryInfoFactory>();

            var fileInfoMock = new Mock<IFileInfo>();
            fileInfoMock.Setup(f => f.Name)
                .Returns("log-file-1");
            fileInfoMock.Setup(f => f.Extension)
                .Returns(".log");
            fileInfoMock.Setup(f => f.OpenRead())
                .Returns(() => new MemoryStream(Encoding.UTF8.GetBytes("Test file content")));

            var directoryInfoMock = new Mock<IDirectoryInfo>();
            directoryInfoMock.Setup(di => di.Exists)
                .Returns(true);
            directoryInfoMock.Setup(di => di.EnumerateFiles(It.IsAny<string>(), SearchOption.AllDirectories))
                .Returns(new[] { fileInfoMock.Object });

            directoryInfoFactoryMock.Setup(d => d.FromPath(It.IsAny<string>()))
                .Returns(directoryInfoMock.Object);

            var appCenterCrashesTarget = this.autoMocker.CreateInstance<AppCenterCrashesTarget>(enablePrivate: true);
            appCenterCrashesTarget.AttachmentsDirectory = "logs";

            var logger = CreateLoggerWithTarget(appCenterCrashesTarget);
            appCenterAnalyticsMock.Invocations.Clear();

            var exception = new InvalidOperationException("Test exception message");

            // Act
            logger.Error(exception, "Test message");

            // Assert
            directoryInfoMock.Verify(d => d.Exists, Times.Once);
            directoryInfoMock.Verify(d => d.EnumerateFiles(It.IsAny<string>(), It.IsAny<SearchOption>()), Times.Once);
            directoryInfoMock.VerifyNoOtherCalls();

            directoryInfoFactoryMock.Verify(d => d.FromPath(It.IsAny<string>()), Times.Once);
            directoryInfoFactoryMock.VerifyNoOtherCalls();

            appCenterAnalyticsMock.Verify(a => a.TrackError(
                exception,
                It.IsAny<Dictionary<string, string>>(),
                It.Is<ErrorAttachmentLog[]>(l =>
                    l.Length == 1 &&
                    l[0].FileName == "log-file-1")), Times.Once);
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