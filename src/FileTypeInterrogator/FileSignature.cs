namespace FileTypeInterrogator
{
    class FileSignature
    {
        // File Signatures taken from: 
        //      http://www.filesignatures.net/index.php?page=all
        //      http://www.garykessler.net/library/file_sigs.html
        // Xlsx, Pptx, Docx all have the same header, so use the additionalIdentifier for unique information in the file (i.e. workbook.xml/document.xml/presentation.xml
        public static readonly FileTypeInfo Unknown = new FileTypeInfo("Unknown", FileType.Invalid, "application/octet-stream", null);
        public static readonly FileTypeInfo Text = new FileTypeInfo("Text", FileType.Txt, "text/plain", null);
        public static readonly FileTypeInfo TabDelimited = new FileTypeInfo("Tab Delimited", FileType.Tsv, "text/tab-separated-values", null);
        public static readonly FileTypeInfo CommaSeparated = new FileTypeInfo("Comma Separated", FileType.Csv, "text/csv", null);
        public static readonly FileTypeInfo Excel2007 = new FileTypeInfo("Excel 2007+", FileType.Xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new byte?[] { 0x50, 0x4b, 0x03, 0x04 }, additionalIdentifier: new byte?[] { 0x77, 0x6f, 0x72, 0x6b, 0x62, 0x6f, 0x6f, 0x6b, 0x2e, 0x78, 0x6d, 0x6c });
        public static readonly FileTypeInfo Word2007 = new FileTypeInfo("Word 2007+", FileType.Docx, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new byte?[] { 0x50, 0x4b, 0x03, 0x04 }, additionalIdentifier: new byte?[] { 0x64, 0x6f, 0x63, 0x75, 0x6d, 0x65, 0x6e, 0x74, 0x2e, 0x78, 0x6d, 0x6c });
        public static readonly FileTypeInfo Powerpoint2007 = new FileTypeInfo("Powerpoint 2007+", FileType.Pptx, "application/vnd.openxmlformats-officedocument.presentationml.presentation", new byte?[] { 0x50, 0x4b, 0x03, 0x04 }, additionalIdentifier: new byte?[] { 0x70, 0x72, 0x65, 0x73, 0x65, 0x6e, 0x74, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x2e, 0x78, 0x6d, 0x6c });
        public static readonly FileTypeInfo Pdf = new FileTypeInfo("PDF", FileType.Pdf, "application/pdf", new byte?[] { 0x25, 0x50, 0x44, 0x46 });
        public static readonly FileTypeInfo Word = new FileTypeInfo("Word", FileType.Doc, "application/msword", new byte?[] { 0xEC, 0xA5, 0xC1, 0x00 }, 512);
        public static readonly FileTypeInfo Excel = new FileTypeInfo("Excel", FileType.Xls, "application/vnd.ms-excel", new byte?[] { 0x09, 0x08, 0x10, 0x00, 0x00, 0x06, 0x05, 0x00 }, 512);
        public static readonly FileTypeInfo Powerpoint = new FileTypeInfo("Powerpoint", FileType.Ppt, "application/vnd.ms-powerpoint", new byte?[] { 0xFD, 0xFF, 0xFF, 0xFF, null, 0x00, 0x00, 0x00 }, 512);
        public static readonly FileTypeInfo Powerpoint2 = new FileTypeInfo("Powerpoint", FileType.Ppt, "application/vnd.ms-powerpoint", new byte?[] { 0xD0, 0xCF, 0X11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, additionalIdentifier: new byte?[] { 0x50, 0x6F, 0x77, 0x65, 0x72, 0x50, 0x6F, 0x69, 0x6E, 0x74 });
        public static readonly FileTypeInfo Jpeg = new FileTypeInfo("Jpeg", FileType.Jpg, "image/jpg", new byte?[] { 0xFF, 0xD8, 0xFF });
        public static readonly FileTypeInfo Png = new FileTypeInfo("Png", FileType.Png, "image/png", new byte?[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        public static readonly FileTypeInfo Gif = new FileTypeInfo("Gif", FileType.Gif, "image/gif", new byte?[] { 0x47, 0x49, 0x46, 0x38, null, 0x61 });
        public static readonly FileTypeInfo Bmp = new FileTypeInfo("Bmp", FileType.Bmp, "image/bmp", new byte?[] { 0x42, 0x4D });
        public static readonly FileTypeInfo Ico = new FileTypeInfo("Icon", FileType.Ico, "image/x-icon", new byte?[] { 0x00, 0x00, 0x01, 0x00 });
        public static readonly FileTypeInfo Tif1 = new FileTypeInfo("Tiff", FileType.Tiff, "image/tiff", new byte?[] { 0x49, 0x20, 0x49 });
        public static readonly FileTypeInfo Tif2 = new FileTypeInfo("Tiff", FileType.Tiff, "image/tiff", new byte?[] { 0x49, 0x49, 0x2A, 0x00 });
        public static readonly FileTypeInfo Tif3 = new FileTypeInfo("Tiff", FileType.Tiff, "image/tiff", new byte?[] { 0x4D, 0x4D, 0x00, 0x2A });
        public static readonly FileTypeInfo Tif4 = new FileTypeInfo("Tiff", FileType.Tiff, "image/tiff", new byte?[] { 0x4D, 0x4D, 0x00, 0x2B });
        public static readonly FileTypeInfo Zip1 = new FileTypeInfo("Zip", FileType.Zip, "application/zip", new byte?[] { 0x50, 0x4B, 0x03, 0x04 }); // archive
        public static readonly FileTypeInfo Zip2 = new FileTypeInfo("Zip", FileType.Zip, "application/zip", new byte?[] { 0x50, 0x4B, 0x05, 0x06 }); // empty archive
        public static readonly FileTypeInfo Zip3 = new FileTypeInfo("Zip", FileType.Zip, "application/zip", new byte?[] { 0x50, 0x4B, 0x07, 0x08 }); // spanned archive
        public static readonly FileTypeInfo Avi = new FileTypeInfo("Avi", FileType.Avi, "video/avi", new byte?[] { 0x52, 0x49, 0x46, 0x46 });
        public static readonly FileTypeInfo Mp4_MP42 = new FileTypeInfo("Mp4", FileType.Mp4, "video/mp4", new byte?[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 });
        public static readonly FileTypeInfo Mp4_ISOM = new FileTypeInfo("Mp4", FileType.Mp4, "video/mp4", new byte?[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D });
        public static readonly FileTypeInfo Wmv = new FileTypeInfo("Wmv", FileType.Wmv, "video/x-ms-asf", new byte?[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C });
        public static readonly FileTypeInfo Zip7Z = new FileTypeInfo("7z", FileType.Zip7Z, "application/x-7z-compressed", new byte?[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C });
    }
}
