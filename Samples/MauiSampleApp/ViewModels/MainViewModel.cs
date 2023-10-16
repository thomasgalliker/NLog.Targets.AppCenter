using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace MauiSampleApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ILogger logger;

        private ICommand logErrorCommand;
        private string message;
        private string exceptionMessage;
        private RelayCommand logCommand;
        private LogLevel logLevel;
        private RelayCommand throwUnhandledExceptionCommand;
        private string exceptionName;

        public MainViewModel(ILogger<MainViewModel> logger)
        {
            this.logger = logger;

            this.Message = "[Track] Sample log message";
            this.ExceptionMessage = "Sample exception message";
            this.LogLevels = Enum.GetValues(typeof(LogLevel))
                .Cast<LogLevel>()
                .ToArray();
            this.LogLevel = LogLevel.Information;

            this.ExceptionNames = new[]
            {
                nameof(Exception),
                nameof(InvalidOperationException),
                nameof(NotImplementedException),
                nameof(TaskCanceledException),
            };
            this.ExceptionName = null;
        }

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        public IEnumerable<LogLevel> LogLevels { get; private set; }

        public LogLevel LogLevel
        {
            get => this.logLevel;
            set => this.SetProperty(ref this.logLevel, value);
        }

        public ICommand LogCommand => this.logCommand ??= new RelayCommand(this.Log);

        private void Log()
        {
            this.logger.Log(this.LogLevel, this.Message);
        }

        public string ExceptionMessage
        {
            get => this.exceptionMessage;
            set => this.SetProperty(ref this.exceptionMessage, value);
        }

        public IEnumerable<string> ExceptionNames { get; private set; }

        public string ExceptionName
        {
            get => this.exceptionName;
            set => this.SetProperty(ref this.exceptionName, value);
        }

        public ICommand LogErrorCommand => this.logErrorCommand ??= new RelayCommand(this.LogError);

        private void LogError()
        {
            Exception exception = null;
            switch (this.ExceptionName)
            {
                case nameof(InvalidOperationException):
                    exception = new InvalidOperationException(this.ExceptionMessage);
                    break;
                case nameof(NotImplementedException):
                    exception = new NotImplementedException(this.ExceptionMessage);
                    break;
                case nameof(TaskCanceledException):
                    exception = new TaskCanceledException(this.ExceptionMessage);
                    break;
                default:
                    exception = new Exception(this.ExceptionMessage);
                    break;
            }

            this.logger.LogError(exception, this.ExceptionMessage);
        }

        public ICommand ThrowUnhandledExceptionCommand => this.throwUnhandledExceptionCommand ??= new RelayCommand(this.ThrowUnhandledException);

        private void ThrowUnhandledException()
        {
            throw new InvalidOperationException(this.ExceptionMessage);
        }
    }
}
