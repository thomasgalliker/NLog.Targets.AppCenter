namespace NLog.Targets.AppCenter.Crashes.Internals
{
    internal class MimeTypeMapping
    {
        public MimeTypeMapping(string fileExtension, string mimeType, bool isText)
        {
            this.FileExtension = fileExtension;
            this.MimeType = mimeType;
            this.IsText = isText;
        }

        public string FileExtension { get; }

        public string MimeType { get; }

        public bool IsText { get; }
    }
}
