using System.IO;
using System.Text;

namespace FileTypeInterrogator
{
    /// <summary>
    /// Wrapper for creating an instance of <see cref="BaseFileTypeInterrogator" /> with the provided definitions.
    /// </summary>
    public class CustomFileTypeInterrogator : BaseFileTypeInterrogator, IFileTypeInterrogator
    {
        /// <summary>
        /// Initializes a <see cref="CustomFileTypeInterrogator"/> with the provided definitions file contents.
        /// </summary>
        /// <param name="definitionsFile">The json object representing the definitions file.</param>
        public CustomFileTypeInterrogator(string definitionsFile) : base(definitionsFile)
        {

        }

        /// <summary>
        /// Initializes a <see cref="CustomFileTypeInterrogator"/> with the definitions at the provided file path.
        /// </summary>
        /// <param name="filePath">Definitions file path.</param>
        /// <param name="encoding">File encoding.</param>
        public CustomFileTypeInterrogator(string filePath, Encoding encoding) : base(File.ReadAllText(filePath, encoding))
        {

        }

        /// <summary>
        /// Initializes a <see cref="CustomFileTypeInterrogator"/> with the definitions from the provided stream.
        /// </summary>
        /// <param name="definitionStream">Definitions stream.</param>
        public CustomFileTypeInterrogator(Stream definitionStream) : base(new StreamReader(definitionStream).ReadToEnd())
        {

        }
    }

}
