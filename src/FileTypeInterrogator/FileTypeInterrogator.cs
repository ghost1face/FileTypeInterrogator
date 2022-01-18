namespace FileTypeInterrogator
{
    /// <summary>
    /// Default implementation with updated definitions.
    /// </summary>
    public class FileTypeInterrogator : BaseFileTypeInterrogator, IFileTypeInterrogator
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FileTypeInterrogator"/> with the default definitions.
        /// </summary>
        public FileTypeInterrogator() : base(Properties.Resources.definitions) { }
    }
}
