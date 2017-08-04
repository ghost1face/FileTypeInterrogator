
namespace FileTypeInterrogator
{
    /// <summary>
    /// Class that identifies characteristics of a specific filetype, namely the name of the type, extensions of the same filetype, header signature, etc.
    /// </summary>
    public class FileTypeInfo
    {
        internal FileTypeInfo(string name, FileType fileType, string mimeType, byte?[] header, int offset = 0, byte?[] additionalIdentifier = null)
        {
            Name = name;
            MimeType = mimeType;
            FileType = fileType;
            Header = header;
            Offset = offset;
            AdditionalIdentifier = additionalIdentifier ?? new byte?[0];
        }

        /// <summary>
        /// Gets the name of the file type.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the <see cref="FileType" /> extension which is related to this type
        /// </summary>
        /// <value>
        /// The supported types.
        /// </value>
        public FileType FileType { get; private set; }

        /// <summary>
        /// Gets the MimeType of this type
        /// </summary>
        public string MimeType { get; private set; }

        /// <summary>
        /// Gets unique header 'Magic Numbers' to identifiy this file type
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public byte?[] Header { get; private set; }

        /// <summary>
        /// Gets the offset location of the Header details
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the additional identifier to guarantee uniqueness of the file type
        /// </summary>
        /// <value>
        /// The additional identifier.
        /// </value>
        public byte?[] AdditionalIdentifier { get; private set; }
    }
}
