using System;
using System.Collections.Generic;
using System.IO;

namespace FileTypeInterrogator
{
    public static class FileTypeInterrogator
    {
        // Unknown and Text objects do NOT belong in the fileTypeInfoCollection
        private static readonly FileTypeInfo[] fileTypeInfoCollection =
        {
            // Zip Files go last due to OpenXML formats being Zipped, compare those first, zip last
            FileSignature.Pdf, FileSignature.Excel2007, FileSignature.Word2007, FileSignature.Powerpoint2007, FileSignature.Word, FileSignature.Excel, FileSignature.Powerpoint, FileSignature.Powerpoint2,
            FileSignature.Jpeg, FileSignature.Png, FileSignature.Gif, FileSignature.Bmp, FileSignature.Ico, FileSignature.Tif1, FileSignature.Tif2, FileSignature.Tif3, FileSignature.Tif4,
            FileSignature.Zip1, FileSignature.Zip2, FileSignature.Zip3, FileSignature.Zip7Z, FileSignature.Mp4_MP42, FileSignature.Mp4_ISOM, FileSignature.Avi, FileSignature.Wmv
        };

        public static FileTypeInfo InterrogateFileContent(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            inputStream.Position = 0;
            byte[] byteBuffer = new byte[Math.Min(inputStream.Length, 4096)]; // read smallest of 4kb or file length
            inputStream.Read(byteBuffer, 0, byteBuffer.Length);
            inputStream.Position = 0;

            return InterrogateFileContent(byteBuffer);
        }

        public static FileTypeInfo InterrogateFileContent(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Length == 0)
                throw new ArgumentException("input must not be empty");

            // iterate over each type and determine if we have a match based on file signature.
            foreach (var fileTypeInfo in fileTypeInfoCollection)
            {
                // if we found a match return the matching filetypeinfo
                if (IsMatchingType(input, fileTypeInfo))
                    return fileTypeInfo;
            }

            // check if text type otherwise no match found, return unknown
            return IsText(input) ? FileSignature.Text : FileSignature.Unknown;
        }

        #region Private Methods

        private static bool IsText(IEnumerable<byte> input)
        {
            int zeroBytesFound = 0;
            foreach (var b in input)
            {
                if (zeroBytesFound > 1) // return if the count is greater than 1
                    return false;

                zeroBytesFound += (b == 0x00 ? 1 : 0); // text files shouldn't have 0x00 byte records
            }
            return true;
        }

        private static bool IsMatchingType(IList<byte> input, FileTypeInfo type)
        {
            // find an initial match based on the header and offset
            bool isMatch = FindMatch(input, type.Header, type.Offset);

            // some file types (Microsoft) have the same header
            // but different signature in another location, if its one of these determine what the true file type is
            if (isMatch && type.AdditionalIdentifier.Length > 0)
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

        private static bool FindMatch(IList<byte> input, IList<byte?> searchArray, int offset = 0)
        {
            // file isn't long enough to even search the proper index, not a match
            if (input.Count < offset)
                return false;

            int matchingCount = 0;
            for (var i = 0; i < searchArray.Count; i++)
            {
                // set the offset location
                int calculatedOffset = i + offset;

                // if file offset is not set to zero, we need to take this into account when comparing.
                // if byte in searchArray is set to null, means this byte is variable, ignore it
                if (searchArray[i] != null && searchArray[i] != input[calculatedOffset])
                {
                    // if one of the bytes do not match, move on
                    matchingCount = 0;
                    break;
                }
                matchingCount++;
            }
            return matchingCount == searchArray.Count;
        }

        #endregion
    }
}
