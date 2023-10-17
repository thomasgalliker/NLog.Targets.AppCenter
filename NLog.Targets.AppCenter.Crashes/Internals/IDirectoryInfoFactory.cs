namespace NLog.Targets.AppCenter.Crashes.Internals
{
    internal interface IDirectoryInfoFactory
    {
        IDirectoryInfo FromPath(string path);
    }
}
