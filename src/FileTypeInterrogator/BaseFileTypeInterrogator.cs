using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileTypeInterrogator
{
    /// <summary>
    /// Base for identifying files by magic number.
    /// </summary>
    public abstract class BaseFileTypeInterrogator : IFileTypeInterrogator
    {
        private static readonly UTF8Encoding utf8WithBomEncoding = new UTF8Encoding(true, true);
        private static readonly UTF8Encoding utf8WithoutBomEncoding = new UTF8Encoding(false, true);
        private static readonly byte[] utf8Bom = utf8WithBomEncoding.GetPreamble();
        private readonly Lazy<IEnumerable<FileTypeInfo>> lazyFileTypes;
        private readonly FileTypeInfo asciiFileType = new FileTypeInfo("ASCII Text", "txt", "text/plain", null);
        private readonly FileTypeInfo utf8FileType = new FileTypeInfo("UTF-8 Text", "txt", "text/plain", null);
        private readonly FileTypeInfo utf8FileTypeWithBOM = new FileTypeInfo("UTF-8 Text with BOM", "txt", "text/plain", null);

        /// <summary>
        /// Initializes a <see cref="BaseFileTypeInterrogator"/> with the provided json definition.
        /// </summary>
        /// <param name="jsonDefinition">The json definition file.</param>
        internal BaseFileTypeInterrogator(string jsonDefinition)
        {
            lazyFileTypes = new Lazy<IEnumerable<FileTypeInfo>>(() => LoadFileTypes(jsonDefinition).ToList());
        }

        /// <summary>
        /// Retrieve available types that are supported based on the current definitions.
        /// </summary>
        public IEnumerable<FileTypeInfo> AvailableTypes => lazyFileTypes.Value;

        /// <summary>
        /// Detect the file type.
        /// </summary>
        /// <param name="inputStream">Input stream to detect file type, if the stream is seekable the stream will be reset upon detecting.</param>
        /// <returns></returns>
        public FileTypeInfo DetectType(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            if (inputStream.CanSeek)
                inputStream.Position = 0;

            int bufferSize = checked((int)inputStream.Length);
            byte[] byteBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                int bytesRead = 0;
                while (bytesRead < bufferSize)
                {
                    int read = inputStream.Read(byteBuffer, bytesRead, bufferSize - bytesRead);
                    if (read == 0)
                        break;

                    bytesRead += read;
                }

                return DetectType(byteBuffer, bytesRead);
            }
            finally
            {
                if (inputStream.CanSeek)
                    inputStream.Position = 0;

                ArrayPool<byte>.Shared.Return(byteBuffer);
            }
        }

        /// <summary>
        /// Detect the file type.
        /// </summary>
        /// <param name="fileContent">The file contents to check.</param>
        /// <returns></returns>
        public FileTypeInfo DetectType(byte[] fileContent)
        {
            return DetectType(fileContent, fileContent?.Length ?? 0);
        }

        private FileTypeInfo DetectType(byte[] fileContent, int length)
        {
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));

            if (length == 0)
                throw new ArgumentException("input must not be empty");

            ReadOnlySpan<byte> input = fileContent.AsSpan(0, length);

            // iterate over each type and determine if we have a match based on file signature.
            foreach (var fileTypeInfo in AvailableTypes)
            {
                // if we found a match return the matching filetypeinfo
                if (IsMatchingType(input, fileTypeInfo))
                    return fileTypeInfo;
            }

            if (IsAscii(input))
                return asciiFileType;

            if (IsUTF8(fileContent, length, out bool hasBOM))
                return hasBOM ? utf8FileTypeWithBOM : utf8FileType;

            return null;
        }

        /// <summary>
        /// Retrieve extensions that are supported based on the current definitions.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAvailableExtensions()
        {
            return AvailableTypes.Select(t => t.FileType).Distinct();
        }

        /// <summary>
        /// Retrieve mime types that are supported based on the current definitions.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAvailableMimeTypes()
        {
            return AvailableTypes.Select(t => t.MimeType).Distinct();
        }

        /// <summary>
        /// Determines if the file contents are of a specified type.
        /// </summary>
        /// <param name="fileContent">The file contents to examine.</param>
        /// <param name="extensionAliasOrMimeType">The file type to validate.</param>
        /// <returns></returns>
        public bool IsType(byte[] fileContent, string extensionAliasOrMimeType)
        {
            foreach (var fileTypeInfo in AvailableTypes)
            {
                if (!(fileTypeInfo.FileType.Equals(extensionAliasOrMimeType, StringComparison.OrdinalIgnoreCase) ||
                    fileTypeInfo.MimeType.Equals(extensionAliasOrMimeType, StringComparison.OrdinalIgnoreCase) ||
                    (fileTypeInfo.Alias != null && fileTypeInfo.Alias.Contains(extensionAliasOrMimeType, StringComparer.OrdinalIgnoreCase))))
                {
                    continue;
                }

                if (IsMatchingType(fileContent, fileTypeInfo))
                    return true;
            }

            if (extensionAliasOrMimeType.Equals("txt", StringComparison.OrdinalIgnoreCase) ||
                extensionAliasOrMimeType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
                return IsText(fileContent, out bool hasBOM);

            return false;
        }

        private static bool IsMatchingType(ReadOnlySpan<byte> input, FileTypeInfo type)
        {
            // find an initial match based on the header and offset
            var isMatch = FindMatch(input, type.Header, type.Offset);

            // some file types have the same header
            // but different signature in another location, if its one of these determine what the true file type is
            if (isMatch && type.SubHeader != null && type.SubHeader.Length > 0)
            {
                isMatch = false;
                for (int i = 0; i <= input.Length - type.SubHeader.Length; i++)
                {
                    if (input[i] == type.SubHeader[0])
                    {
                        isMatch = FindMatch(input, type.SubHeader, i);
                        if (isMatch)
                            break;
                    }
                }
            }

            return isMatch;
        }

        private static bool FindMatch(ReadOnlySpan<byte> input, ReadOnlySpan<byte> searchArray, int offset = 0)
        {
            // file isn't long enough to even search the proper index, not a match
            if (input.Length <= offset)
                return false;

            int matchingCount = 0;
            for (var i = 0; i < searchArray.Length; i++)
            {
                // set the offset location
                var calculatedOffset = i + offset;

                if (input.Length <= calculatedOffset)
                    break;

                // if file offset is not set to zero, we need to take this into account when comparing.
                // if byte in searchArray is set to null, means this byte is variable, ignore it
                if (searchArray[i] != input[calculatedOffset])
                {
                    // if one of the bytes do not match, move on
                    matchingCount = 0;
                    break;
                }
                matchingCount++;
            }
            return matchingCount == searchArray.Length;
        }

        private static IEnumerable<FileTypeInfo> LoadFileTypes(string flatFileData)
        {
            using (var stringReader = new StringReader(flatFileData))
            {
                string line = null;
                while ((line = stringReader.ReadLine()) != null)
                {
                    var segments = line.Split('\t');
                    int offset = int.Parse(segments[0]);
                    // segment[1] = type
                    string signature = segments[2];
                    string additional = segments[3];
                    string name = segments[4];
                    string extension = segments[5];
                    string mimeType = segments[6];
                    string alias = segments.Length == 8 ? segments[7] : null;

                    byte[] sigBytes = HexStringToByteArray(signature);
                    byte[] additionalBytes = string.IsNullOrWhiteSpace(additional) ? null : HexStringToByteArray(additional);
                    string[] aliases = string.IsNullOrWhiteSpace(alias) ? null : alias.Split('|');

                    yield return new FileTypeInfo(
                        name,
                        extension,
                        mimeType,
                        header: sigBytes,
                        alias: aliases,
                        offset: offset,
                        subHeader: additionalBytes
                    );
                }
            }
        }

        private static byte[] HexStringToByteArray(string hexString)
        {
            int numberOfCharacters = hexString.Length;
            byte[] byteArray = new byte[numberOfCharacters / 2];
            for (int i = 0; i < numberOfCharacters; i += 2)
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return byteArray;
        }

        private static bool IsText(byte[] input, out bool hasBOM)
        {
            hasBOM = false;

            bool isAscii = IsAscii(input);

            return isAscii || IsUTF8(input, input.Length, out hasBOM);
        }

        private static bool IsAscii(ReadOnlySpan<byte> input)
        {
            const byte maxAscii = 0x7F;
            foreach (var b in input)
            {
                if (b > maxAscii)
                    return false;
            }
            return true;
        }

        private static bool IsUTF8(byte[] input, int length, out bool hasBOM)
        {
            bool isUTF8 = true;
            int bomLength = utf8Bom.Length;

            hasBOM = false;

            ReadOnlySpan<byte> inputSpan = input.AsSpan(0, length);
            if (length >= bomLength && inputSpan.Slice(0, bomLength).SequenceEqual(utf8Bom))
            {
                try
                {
                    utf8WithBomEncoding.GetString(input, bomLength, length - bomLength);
                    hasBOM = true;
                }
                catch (ArgumentException)
                {
                    // not utf8 due to exception
                    isUTF8 = false;
                }
            }

            if (isUTF8 && !hasBOM)
            {
                try
                {
                    utf8WithoutBomEncoding.GetString(input, 0, length);
                    isUTF8 = true;
                }
                catch (ArgumentException)
                {
                    isUTF8 = false;
                }
            }

            return isUTF8;
        }
    }
}
