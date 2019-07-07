namespace FileTypeInterrogator
{
    /// <summary>
    /// Default implementation with updated definitions.
    /// </summary>
    public class FileTypeInterrogator : BaseFileTypeInterrogator, IFileTypeInterrogator
    {
        public FileTypeInterrogator() : base(Properties.Resources.definitions) { }
    }
}
