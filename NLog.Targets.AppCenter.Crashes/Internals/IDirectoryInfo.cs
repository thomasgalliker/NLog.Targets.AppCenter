using System.Collections.Generic;
using System.IO;

namespace NLog.Targets.AppCenter.Crashes.Internals
{
    internal interface IDirectoryInfo
    {
        /// <inheritdoc cref="DirectoryInfo.Exists"/>
        bool Exists { get; }

        /// <inheritdoc cref="DirectoryInfo.EnumerateFiles(string, SearchOption)"/>
        IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption);
    }
}