namespace NLog.Targets.AppCenter.Internals
{
    internal class AppCenterWrapper : IAppCenter
    {
        public bool Configured => Microsoft.AppCenter.AppCenter.Configured;
    }
}