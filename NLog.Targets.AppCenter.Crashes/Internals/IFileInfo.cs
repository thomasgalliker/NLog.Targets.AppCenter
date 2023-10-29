using System.IO;

namespace NLog.Targets.AppCenter.Crashes.Internals
{
    internal interface IFileInfo
    {
        string Name { get; }
        
        string FullName { get; }

        string Extension { get; }

        Stream OpenRead();
    }
}