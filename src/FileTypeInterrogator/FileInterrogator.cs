namespace FileTypeInterrogator
{
    /// <summary>
    /// Default implementation with updated definitions.
    /// </summary>
    public class FileInterrogator : BaseFileTypeInterrogator, IFileTypeInterrogator
    {
        public FileInterrogator() : base(Properties.Resources.definitions) { }
    }
}
