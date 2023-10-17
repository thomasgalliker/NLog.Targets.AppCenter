using System.Diagnostics;
using System.IO;

namespace NLog.Targets.AppCenter.Crashes.Internals
{
    [DebuggerDisplay("{this.Name}{this.Extension}")]
    internal class FileInfoWrapper : IFileInfo
    {
        private readonly FileInfo fileInfo;

        public FileInfoWrapper(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public string Name => this.fileInfo.Name;

        public string FullName => this.fileInfo.FullName;

        public string Extension => this.fileInfo.Extension;

        public Stream OpenRead()
        {
            return this.fileInfo.OpenRead();
        }
    }
}