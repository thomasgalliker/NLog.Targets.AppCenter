using System.Reflection;
using Newtonsoft.Json.Linq;

namespace MauiSampleApp
{
    public static class Secrets
    {
        // There is no built-in support for user secrets in .NET MAUI.
        // The issue is tracked here:
        // https://github.com/dotnet/maui/issues/4408

        public static string AppCenterApiKey
        {
            get
            {
                return Configuration?.Value<string>(nameof(AppCenterApiKey)) ?? throw new NullReferenceException(nameof(AppCenterApiKey));
            }
        }

        private static JObject Configuration { get; } = GetConfiguration("secrets.json");

        private static JObject GetConfiguration(string filePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName().Name;
            using var fileStream = assembly.GetManifestResourceStream($"{assemblyName}.{filePath}");
            if (fileStream == null)
            {
                return null;
            }

            using StreamReader sr = new(fileStream);
            var fileContent = sr.ReadToEnd();
            return JObject.Parse(fileContent);
        }
    }
}
