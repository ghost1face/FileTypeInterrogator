using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileTypeInterrogator
{
    /// <summary>
    /// Base for interacting files by magic number
    /// </summary>
    public abstract class BaseFileTypeInterrogator : IFileTypeInterrogator
    {
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

            byte[] byteBuffer = new byte[inputStream.Length];
            inputStream.Read(byteBuffer, 0, byteBuffer.Length);

            if (inputStream.CanSeek)
                inputStream.Position = 0;

            return DetectType(byteBuffer);
        }

        /// <summary>
        /// Detect the file type.
        /// </summary>
        /// <param name="fileContent">The file contents to check.</param>
        /// <returns></returns>
        public FileTypeInfo DetectType(byte[] fileContent)
        {
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));

            if (fileContent.Length == 0)
                throw new ArgumentException("input must not be empty");

            // iterate over each type and determine if we have a match based on file signature.
            foreach (var fileTypeInfo in AvailableTypes)
            {
                // if we found a match return the matching filetypeinfo
                if (IsMatchingType(fileContent, fileTypeInfo))
                    return fileTypeInfo;
            }

            if (IsAscii(fileContent))
                return asciiFileType;

            if (IsUTF8(fileContent, out bool hasBOM))
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
            foreach (var fileTypeInfo in AvailableTypes.Where(t =>
                t.FileType.Equals(extensionAliasOrMimeType, StringComparison.OrdinalIgnoreCase) ||
                t.MimeType.Equals(extensionAliasOrMimeType, StringComparison.OrdinalIgnoreCase) ||
                (t.Alias != null && t.Alias.Contains(extensionAliasOrMimeType, StringComparer.OrdinalIgnoreCase))))
            {
                if (IsMatchingType(fileContent, fileTypeInfo))
                    return true;
            }

            if (extensionAliasOrMimeType.Equals("txt", StringComparison.OrdinalIgnoreCase) ||
                extensionAliasOrMimeType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
                return IsText(fileContent, out bool hasBOM);

            return false;
        }

        private static bool IsMatchingType(IList<byte> input, FileTypeInfo type)
        {
            // find an initial match based on the header and offset
            var isMatch = FindMatch(input, type.Header, type.Offset);

            // some file types have the same header
            // but different signature in another location, if its one of these determine what the true file type is
            if (isMatch && type.SubHeader != null)
            {
                // find all indices of matching the 1st byte of the additional sequence
                var matchingIndices = new List<int>();
                for (int i = 0; i < input.Count; i++)
                {
                    if (input[i] == type.SubHeader[0])
                        matchingIndices.Add(i);
                }

                // investigate all of them for a match
                foreach (int potentialMatchingIndex in matchingIndices)
                {
                    isMatch = FindMatch(input, type.SubHeader, potentialMatchingIndex);

                    if (isMatch)
                        break;
                }
            }

            return isMatch;
        }

        private static bool FindMatch(IList<byte> input, IList<byte> searchArray, int offset = 0)
        {
            // file isn't long enough to even search the proper index, not a match
            if (input.Count <= offset)
                return false;

            int matchingCount = 0;
            for (var i = 0; i < searchArray.Count; i++)
            {
                // set the offset location
                var calculatedOffset = i + offset;

                if (input.Count <= calculatedOffset)
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
            return matchingCount == searchArray.Count;
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

            return isAscii || IsUTF8(input, out hasBOM);
        }

        private static bool IsAscii(byte[] input)
        {
            const byte maxAscii = 0x7F;
            foreach (var b in input)
            {
                if (b > maxAscii)
                    return false;
            }
            return true;
        }

        private static bool IsUTF8(byte[] input, out bool hasBOM)
        {
            UTF8Encoding utf8WithBOM = new UTF8Encoding(true, true);
            bool isUTF8 = true;
            byte[] bom = utf8WithBOM.GetPreamble();
            int bomLength = bom.Length;

            hasBOM = false;

            if (input.Length >= bomLength && bom.SequenceEqual(input.Take(bomLength)))
            {
                try
                {
                    utf8WithBOM.GetString(input, bomLength, input.Length - bomLength);
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
                UTF8Encoding utf8WithoutBOM = new UTF8Encoding(false, true);
                try
                {
                    utf8WithoutBOM.GetString(input, 0, input.Length);
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
