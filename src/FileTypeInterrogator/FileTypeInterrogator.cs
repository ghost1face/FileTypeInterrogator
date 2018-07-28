using System;
using System.Collections.Generic;
using System.IO;

namespace FileTypeInterrogator
{
    public static class FileTypeInterrogator
    {
        // File Signatures taken from: 
        //      http://www.filesignatures.net/index.php?page=all
        //      http://www.garykessler.net/library/file_sigs.html
        // Xlsx, Pptx, Docx all have the same header, so use the additionalIdentifier for unique information in the file (i.e. workbook.xml/document.xml/presentation.xml
        // Unknown and Text objects do NOT belong in the fileTypeInfoCollection
        private static readonly FileTypeInfo Unknown = new FileTypeInfo("Unknown", FileType.Invalid, "application/octet-stream", null);
        private static readonly FileTypeInfo Text = new FileTypeInfo("Text", FileType.Txt, "text/plain", null);
        private static readonly FileTypeInfo TabDelimited = new FileTypeInfo("Tab Delimited", FileType.Tsv, "text/tab-separated-values", null);
        private static readonly FileTypeInfo CommaSeparated = new FileTypeInfo("Comma Separated", FileType.Csv, "text/csv", null);
        private static readonly FileTypeInfo Excel2007 = new FileTypeInfo("Excel 2007+", FileType.Xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new byte?[] { 0x50, 0x4b, 0x03, 0x04 }, additionalIdentifier: new byte?[] { 0x77, 0x6f, 0x72, 0x6b, 0x62, 0x6f, 0x6f, 0x6b, 0x2e, 0x78, 0x6d, 0x6c });
        private static readonly FileTypeInfo Word2007 = new FileTypeInfo("Word 2007+", FileType.Docx, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new byte?[] { 0x50, 0x4b, 0x03, 0x04 }, additionalIdentifier: new byte?[] { 0x64, 0x6f, 0x63, 0x75, 0x6d, 0x65, 0x6e, 0x74, 0x2e, 0x78, 0x6d, 0x6c });
        private static readonly FileTypeInfo Powerpoint2007 = new FileTypeInfo("Powerpoint 2007+", FileType.Pptx, "application/vnd.openxmlformats-officedocument.presentationml.presentation", new byte?[] { 0x50, 0x4b, 0x03, 0x04 }, additionalIdentifier: new byte?[] { 0x70, 0x72, 0x65, 0x73, 0x65, 0x6e, 0x74, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x2e, 0x78, 0x6d, 0x6c });
        private static readonly FileTypeInfo Pdf = new FileTypeInfo("PDF", FileType.Pdf, "application/pdf", new byte?[] { 0x25, 0x50, 0x44, 0x46 });
        private static readonly FileTypeInfo Word = new FileTypeInfo("Word", FileType.Doc, "application/msword", new byte?[] { 0xEC, 0xA5, 0xC1, 0x00 }, 512);
        private static readonly FileTypeInfo Excel = new FileTypeInfo("Excel", FileType.Xls, "application/vnd.ms-excel", new byte?[] { 0x09, 0x08, 0x10, 0x00, 0x00, 0x06, 0x05, 0x00 }, 512);
        private static readonly FileTypeInfo Powerpoint = new FileTypeInfo("Powerpoint", FileType.Ppt, "application/vnd.ms-powerpoint", new byte?[] { 0xFD, 0xFF, 0xFF, 0xFF, null, 0x00, 0x00, 0x00 }, 512);
        private static readonly FileTypeInfo Powerpoint2 = new FileTypeInfo("Powerpoint", FileType.Ppt, "application/vnd.ms-powerpoint", new byte?[] { 0xD0, 0xCF, 0X11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, additionalIdentifier: new byte?[] { 0x50, 0x6F, 0x77, 0x65, 0x72, 0x50, 0x6F, 0x69, 0x6E, 0x74 });
        private static readonly FileTypeInfo Jpeg = new FileTypeInfo("Jpeg", FileType.Jpg, "image/jpg", new byte?[] { 0xFF, 0xD8, 0xFF });
        private static readonly FileTypeInfo Png = new FileTypeInfo("Png", FileType.Png, "image/png", new byte?[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        private static readonly FileTypeInfo Gif = new FileTypeInfo("Gif", FileType.Gif, "image/gif", new byte?[] { 0x47, 0x49, 0x46, 0x38, null, 0x61 });
        private static readonly FileTypeInfo Bmp = new FileTypeInfo("Bmp", FileType.Bmp, "image/bmp", new byte?[] { 0x42, 0x4D });
        private static readonly FileTypeInfo Ico = new FileTypeInfo("Icon", FileType.Ico, "image/x-icon", new byte?[] { 0x00, 0x00, 0x01, 0x00 });
        private static readonly FileTypeInfo Tif1 = new FileTypeInfo("Tiff", FileType.Tiff, "image/tiff", new byte?[] { 0x49, 0x20, 0x49 });
        private static readonly FileTypeInfo Tif2 = new FileTypeInfo("Tiff", FileType.Tiff, "image/tiff", new byte?[] { 0x49, 0x49, 0x2A, 0x00 });
        private static readonly FileTypeInfo Tif3 = new FileTypeInfo("Tiff", FileType.Tiff, "image/tiff", new byte?[] { 0x4D, 0x4D, 0x00, 0x2A });
        private static readonly FileTypeInfo Tif4 = new FileTypeInfo("Tiff", FileType.Tiff, "image/tiff", new byte?[] { 0x4D, 0x4D, 0x00, 0x2B });
        private static readonly FileTypeInfo Zip1 = new FileTypeInfo("Zip", FileType.Zip, "application/zip", new byte?[] { 0x50, 0x4B, 0x03, 0x04 }); // archive
        private static readonly FileTypeInfo Zip2 = new FileTypeInfo("Zip", FileType.Zip, "application/zip", new byte?[] { 0x50, 0x4B, 0x05, 0x06 }); // empty archive
        private static readonly FileTypeInfo Zip3 = new FileTypeInfo("Zip", FileType.Zip, "application/zip", new byte?[] { 0x50, 0x4B, 0x07, 0x08 }); // spanned archive
        private static readonly FileTypeInfo Avi = new FileTypeInfo("Avi", FileType.Avi, "video/avi", new byte?[] { 0x52, 0x49, 0x46, 0x46 });
        private static readonly FileTypeInfo Mp4_MP42 = new FileTypeInfo("Mp4", FileType.Mp4, "video/mp4", new byte?[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 });
        private static readonly FileTypeInfo Mp4_ISOM = new FileTypeInfo("Mp4", FileType.Mp4, "video/mp4", new byte?[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D });
        private static readonly FileTypeInfo Wmv = new FileTypeInfo("Wmv", FileType.Wmv, "video/x-ms-asf", new byte?[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C });

        private static readonly FileTypeInfo[] fileTypeInfoCollection =
        {
            // Zip Files go last due to OpenXML formats being Zipped, compare those first, zip last
            Pdf, Excel2007, Word2007, Powerpoint2007, Word, Excel, Powerpoint, Powerpoint2,
            Jpeg, Png, Gif, Bmp, Ico, Tif1, Tif2, Tif3, Tif4, Zip1, Zip2, Zip3, Mp4_MP42, Mp4_ISOM, Avi, Wmv
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
            return IsText(input) ? Text : Unknown;
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
