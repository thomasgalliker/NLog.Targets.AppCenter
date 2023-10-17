using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NLog.Targets.AppCenter.Crashes.Internals
{
    [DebuggerDisplay("{this.directoryInfo.FullName}")]
    internal class DirectoryInfoWrapper : IDirectoryInfo
    {
        private readonly DirectoryInfo directoryInfo;

        public DirectoryInfoWrapper(string path)
        {
            this.directoryInfo = new DirectoryInfo(path);
        }

        public bool Exists => this.directoryInfo.Exists;

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            foreach (var fileInfo in this.directoryInfo.EnumerateFiles(searchPattern, searchOption))
            {
                yield return new FileInfoWrapper(fileInfo);
            }
        }
    }
}