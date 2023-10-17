namespace NLog.Targets.AppCenter.Crashes.Internals
{
    internal class DirectoryInfoFactory : IDirectoryInfoFactory
    {
        public IDirectoryInfo FromPath(string path)
        {
            return new DirectoryInfoWrapper(path);
        }
    }
}
