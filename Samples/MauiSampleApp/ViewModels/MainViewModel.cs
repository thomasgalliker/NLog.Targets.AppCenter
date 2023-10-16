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

        public MainViewModel(ILogger<MainViewModel> logger)
        {
            this.logger = logger;

            this.Message = "[Track] Sample log message";
            this.ExceptionMessage = "Sample exception message";
            this.LogLevels = Enum.GetValues(typeof(LogLevel))
                .Cast<LogLevel>()
                .ToArray();
            this.LogLevel = LogLevel.Information;
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

        public ICommand LogErrorCommand => this.logErrorCommand ??= new RelayCommand(this.LogError);

        private void LogError()
        {
            this.logger.LogError(new Exception(this.ExceptionMessage), this.ExceptionMessage);
        }
    }
}
