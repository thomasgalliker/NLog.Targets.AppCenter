using System.IO;

namespace NLog.Targets.AppCenter.Crashes.Internals
{
    public interface IFileInfo
    {
        string Name { get; }
        
        string FullName { get; }

        string Extension { get; }

        Stream OpenRead();
    }
}