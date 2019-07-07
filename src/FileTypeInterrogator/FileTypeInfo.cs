namespace FileTypeInterrogator
{
    /// <summary>
    /// Information regarding the file type, including name, extension, mime type and signature.
    /// </summary>
    public class FileTypeInfo
    {
        internal FileTypeInfo(string name, string fileType, string mimeType, byte[] header, string[] alias = null, int offset = 0, byte[] subHeader = null)
        {
            Name = name;
            MimeType = mimeType;
            FileType = fileType;
            Header = header;
            Offset = offset;
            SubHeader = subHeader;
            Alias = alias;
        }

        /// <summary>
        /// Gets the name of the file type.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the extension which is related to this type
        /// </summary>
        public string FileType { get; private set; }

        /// <summary>
        /// Gets the MimeType of this type
        /// </summary>
        public string MimeType { get; private set; }

        /// <summary>
        /// Other names for this file type
        /// </summary>
        public string[] Alias { get; private set; }

        /// <summary>
        /// Gets unique header 'Magic Numbers' to identifiy this file type
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public byte[] Header { get; private set; }

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
        public byte[] SubHeader { get; private set; }
    }

}
