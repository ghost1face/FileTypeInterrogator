namespace FileTypeInterrogator
{
    /// <summary>
    /// Default implementation with updated definitions.
    /// </summary>
    public class FileInterrogator : BaseFileTypeInterrogator, IFileInterrogator
    {
        public FileInterrogator() : base(Properties.Resources.definitions) { }
    }
}
