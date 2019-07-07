using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;

namespace FileTypeInterrogator
{
    /// <summary>
    /// Base for interacting files by magic number
    /// </summary>
    public abstract class BaseFileTypeInterrogator : IFileTypeInterrogator
    {
        private readonly Lazy<IEnumerable<FileTypeInfo>> lazyFileTypes;

        /// <summary>
        /// Initializes a <see cref="BaseFileTypeInterrogator"/> with the provided json definition.
        /// </summary>
        /// <param name="jsonDefinition">The json definition file.</param>
        internal BaseFileTypeInterrogator(string jsonDefinition)
        {
            lazyFileTypes = new Lazy<IEnumerable<FileTypeInfo>>(() => LoadFileTypes(jsonDefinition));
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

            // read smallest of 4kb or file length
            byte[] byteBuffer = new byte[Math.Min(inputStream.Length, 4096)];
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
        /// <param name="fileType">The file type to validate.</param>
        /// <returns></returns>
        public bool IsType(byte[] fileContent, string fileType)
        {
            foreach (var fileTypeInfo in AvailableTypes.Where(t => t.FileType.Equals(fileType, StringComparison.OrdinalIgnoreCase)))
            {
                if (IsMatchingType(fileContent, fileTypeInfo))
                    return true;
            }

            return false;
        }

        private static bool IsMatchingType(IList<byte> input, FileTypeInfo type)
        {
            // find an initial match based on the header and offset
            var isMatch = FindMatch(input, type.Header, type.Offset);

            // some file types have the same header
            // but different signature in another location, if its one of these determine what the true file type is
            if (isMatch && type.AdditionalIdentifier != null)
            {
                // find all indices of matching the 1st byte of the additional sequence
                var matchingIndices = new List<int>();
                for (int i = 0; i < input.Count; i++)
                {
                    if (input[i] == type.AdditionalIdentifier[0])
                        matchingIndices.Add(i);
                }

                // investigate all of them for a match
                foreach (int potentialMatchingIndex in matchingIndices)
                {
                    isMatch = FindMatch(input, type.AdditionalIdentifier, potentialMatchingIndex);

                    if (isMatch)
                        break;
                }
            }

            return isMatch;
        }

        private static bool FindMatch(IList<byte> input, IList<byte> searchArray, int offset = 0)
        {
            // file isn't long enough to even search the proper index, not a match
            if (input.Count < offset)
                return false;

            int matchingCount = 0;
            for (var i = 0; i < searchArray.Count; i++)
            {
                // set the offset location
                var calculatedOffset = i + offset;

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

        private static IEnumerable<FileTypeInfo> LoadFileTypes(string jsonData)
        {
            var fileTypeInfos = new List<FileTypeInfo>();
            var jsonValue = JsonValue.Parse(jsonData);
            var jsonObject = jsonValue as JsonObject;

            foreach (var fileExtension in jsonObject.Keys)
            {
                if (!jsonObject.TryGetValue(fileExtension, out JsonValue fileTypeInfo))
                    continue;

                var fileTypeInfoObject = fileTypeInfo as JsonObject;
                var signatures = fileTypeInfoObject["signs"].Cast<string>();
                var mimeType = (string)fileTypeInfoObject["mime"];
                var name = (string)fileTypeInfoObject["name"];

                string[] alias = null;
                if (fileTypeInfoObject.TryGetValue("alias", out JsonValue aliasArray))
                    alias = aliasArray.Cast<string>().ToArray();

                foreach (var signature in signatures)
                {
                    var sigParts = signature.Split(',');

                    var offset = int.Parse(sigParts[0]);
                    byte[] sigHeader = HexStringToByteArray(sigParts[1]);
                    byte[] sigAdditional = null;

                    fileTypeInfos.Add(
                        new FileTypeInfo(name, fileExtension, mimeType, header: sigHeader, offset: offset, additionalIdentifier: sigAdditional)
                    );
                }
            }

            return fileTypeInfos;
        }

        private static byte[] HexStringToByteArray(string hexString)
        {
            int numberOfCharacters = hexString.Length;
            byte[] byteArray = new byte[numberOfCharacters / 2];
            for (int i = 0; i < numberOfCharacters; i += 2)
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return byteArray;
        }
    }
}
