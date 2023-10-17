using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace MauiSampleApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private const string MessagePreferencesKey = "MainViewModel.Message";
        private const string ExceptionMessagePreferencesKey = "MainViewModel.ExceptionMessage";

        private static readonly IDictionary<Type, Func<string, Exception>> ExceptionFactories = new Dictionary<Type, Func<string, Exception>>
        {
            { typeof(Exception),  s => new Exception(s) },
            { typeof(ArgumentException),  s => new ArgumentException(s, "testParam") },
            { typeof(ArgumentNullException),  s => new ArgumentNullException("testParam", s) },
            { typeof(ArgumentOutOfRangeException),  s => new ArgumentOutOfRangeException("testParam", s) },
            { typeof(IndexOutOfRangeException),  s => new IndexOutOfRangeException(s) },
            { typeof(InvalidOperationException),  s => new InvalidOperationException(s) },
            { typeof(NotImplementedException),  s => new NotImplementedException(s) },
            { typeof(TaskCanceledException),  s => new TaskCanceledException(s) },
            { typeof(OperationCanceledException),  s => new OperationCanceledException(s) },
            { typeof(OutOfMemoryException),  s => new OutOfMemoryException(s) },
            { typeof(ObjectDisposedException),  s => new ObjectDisposedException(s) },
        };

        private readonly ILogger logger;
        private readonly IPreferences preferences;
        private ICommand logErrorCommand;
        private string message;
        private string exceptionMessage;
        private RelayCommand logCommand;
        private LogLevel logLevel;
        private RelayCommand throwUnhandledExceptionCommand;
        private string exceptionName;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            IPreferences preferences)
        {
            this.logger = logger;
            this.preferences = preferences;


            var lastMessage = this.preferences.Get<string>(MessagePreferencesKey, null);
            this.message = lastMessage ?? "[Track] Sample log message (1)";

            var lastExceptionMessage = this.preferences.Get<string>(ExceptionMessagePreferencesKey, null);
            this.exceptionMessage = lastExceptionMessage ?? "Sample exception message (1)";

            this.LogLevels = Enum.GetValues(typeof(LogLevel))
                .Cast<LogLevel>()
                .ToArray();
            this.LogLevel = LogLevel.Information;

            this.ExceptionNames = ExceptionFactories.Select(f => f.Key.FullName).ToArray();
            this.ExceptionName = this.ExceptionNames.FirstOrDefault();

            this.logger.LogInformation("Started");
        }

        public string Message
        {
            get => this.message;
            set
            {
                if (this.SetProperty(ref this.message, value))
                {
                    this.preferences.Set(MessagePreferencesKey, value);
                }
            }
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

            TryIncrementNumberInProperty(s => this.Message = s, () => this.Message);
        }

        public string ExceptionMessage
        {
            get => this.exceptionMessage;
            set
            {
                if (this.SetProperty(ref this.exceptionMessage, value))
                {
                    this.preferences.Set(ExceptionMessagePreferencesKey, value);
                }
            }
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
            var exception = CreateException(this.ExceptionName, this.ExceptionMessage);

            this.logger.LogError(exception, exception.Message);

            TryIncrementNumberInProperty(s => this.ExceptionMessage = s, () => this.ExceptionMessage);
        }

        private static Exception CreateException(string exceptionName, string exceptionMessage)
        {
            return ExceptionFactories
                .Single(f => f.Key.FullName == exceptionName)
                .Value(exceptionMessage);
        }

        private static void TryIncrementNumberInProperty(Action<string> setProperty, Func<string> getProperty)
        {
            if (getProperty() is string propertyValue)
            {
                var match = Regex.Match(propertyValue, "(?<Number>[0-9]+)[^0-9]*$");
                if (match.Success &&
                    match.Groups.TryGetValue("Number", out var numberGroup))
                {
                    if (int.TryParse(numberGroup.Value, out var number))
                    {
                        setProperty(propertyValue.Replace(numberGroup.Value, $"{++number}"));
                    }
                }
            }
        }

        public ICommand ThrowUnhandledExceptionCommand => this.throwUnhandledExceptionCommand ??= new RelayCommand(this.ThrowUnhandledException);

        private void ThrowUnhandledException()
        {
            var exception = CreateException(this.ExceptionName, this.ExceptionMessage);

            TryIncrementNumberInProperty(s => this.ExceptionMessage = s, () => this.ExceptionMessage);

            throw exception;
        }
    }
}
