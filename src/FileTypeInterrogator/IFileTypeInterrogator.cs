using System.Collections.Generic;
using System.IO;

namespace FileTypeInterrogator
{
    /// <summary>
    /// Interface for interrogating file contents to determine proper file types
    /// </summary>
    public interface IFileTypeInterrogator
    {
        /// <summary>
        /// Retrieve extensions that are supported based on the current definitions.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAvailableExtensions();

        /// <summary>
        /// Retrieve mime types that are supported based on the current definitions.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAvailableMimeTypes();

        /// <summary>
        /// Retrieve available types that are supported based on the current definitions.
        /// </summary>
        IEnumerable<FileTypeInfo> AvailableTypes { get; }

        /// <summary>
        /// Detect the file type.
        /// </summary>
        /// <param name="fileContent">The file contents to check.</param>
        /// <returns></returns>
        FileTypeInfo DetectType(byte[] fileContent);

        /// <summary>
        /// Detect the file type.
        /// </summary>
        /// <param name="inputStream">Input stream to detect file type, if the stream is seekable the stream will be reset upon detecting.</param>
        /// <returns></returns>
        FileTypeInfo DetectType(Stream inputStream);

        /// <summary>
        /// Determines if the file contents are of a specified type.
        /// </summary>
        /// <param name="fileContent">The file contents to examine.</param>
        /// <param name="fileType">The file type to validate.</param>
        /// <returns></returns>
        bool IsType(byte[] fileContent, string fileType);
    }
}
