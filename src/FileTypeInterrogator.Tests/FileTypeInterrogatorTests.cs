using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileTypeInterrogator.Tests
{
    public partial class FileTypeInterrogatorTests
    {
        [TestMethod]
        public void CanDetectAscii()
        {
            const string extension = "ascii";
            DetectType(extension, result =>
            {
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Name.StartsWith(extension, StringComparison.OrdinalIgnoreCase));
            });
        }

        [TestMethod]
        public void CanDetectUTF8()
        {
            const string extension = "utf8";
            DetectType(extension, result =>
            {
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Name.StartsWith("UTF-8", StringComparison.OrdinalIgnoreCase));
                Assert.IsTrue(result.Name.IndexOf("BOM", StringComparison.OrdinalIgnoreCase) == -1);
            });
        }

        [TestMethod]
        public void CanDetectUTF8BOM()
        {
            const string extension = "utf8bom";
            DetectType(extension, result =>
            {
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Name.StartsWith("UTF-8", StringComparison.OrdinalIgnoreCase));
                Assert.IsTrue(result.Name.IndexOf("BOM", StringComparison.OrdinalIgnoreCase) > -1);
            });
        }

        [DataTestMethod]
        [DataRow("pdf", DisplayName = "PDF Test")]
        [DataRow("fdf", DisplayName = "FDF Test")]
        public void CanDetectAdobe(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("ai", DisplayName = "AI Test")]
        [DataRow("bmp", DisplayName = "BMP Test")]
        [DataRow("gif", DisplayName = "GIF Test")]
        [DataRow("ico", DisplayName = "ICO Test")]
        [DataRow("jp2", DisplayName = "JP2 Test")]
        [DataRow("jpg", DisplayName = "JPG Test")]
        [DataRow("pcx", DisplayName = "PCX Test")]
        [DataRow("png", DisplayName = "PNG Test")]
        [DataRow("psd", DisplayName = "PSD Test")]
        [DataRow("tif", DisplayName = "TIF Test")]
        [DataRow("webp", DisplayName = "WEBP Test")]
        public void CanDetectImages(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("3gp", DisplayName = "3GP Test")]
        [DataRow("avi", DisplayName = "AVI Test")]
        [DataRow("flv", DisplayName = "FLV Test")]
        [DataRow("mid", DisplayName = "MID Test")]
        [DataRow("mkv", DisplayName = "MKV Test")]
        [DataRow("mp4", DisplayName = "MP4 Test")]
        [DataRow("webm", DisplayName = "WEBM Test")]
        [DataRow("wmv", DisplayName = "WMV Test")]
        public void CanDetectVideo(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("ac3", DisplayName = "AC3 Test")]
        [DataRow("aiff", DisplayName = "AIFF Test")]
        [DataRow("flac", DisplayName = "FLAC Test")]
        [DataRow("mp3", DisplayName = "MP3 Test")]
        [DataRow("ogg", DisplayName = "OGG Test")]
        [DataRow("ra", DisplayName = "RA Test")]
        [DataRow("wav", DisplayName = "WAV Test")]
        public void CanDetectAudio(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("doc", DisplayName = "DOC Test")]
        [DataRow("docx", DisplayName = "DOC Test")]
        [DataRow("odp", DisplayName = "ODP Test")]
        [DataRow("odt", DisplayName = "ODT Test")]
        [DataRow("ppt", DisplayName = "PPT Test")]
        [DataRow("pptx", DisplayName = "PPTX Test")]
        [DataRow("rtf", DisplayName = "RTF Test")]
        [DataRow("xls", DisplayName = "XLS Test")]
        [DataRow("xlsx", DisplayName = "XLSX Test")]
        public void CanDetectOffice(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("otf", DisplayName = "OTF Test")]
        [DataRow("ttf", DisplayName = "TTF Test")]
        [DataRow("woff", DisplayName = "WOFF Test")]
        public void CanDetectFont(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("7z", DisplayName = "7Z Test")]
        [DataRow("gz", DisplayName = "GZ Test")]
        [DataRow("rar", DisplayName = "RAR Test")]
        [DataRow("zip", DisplayName = "ZIP Test")]
        public void CanDetectCompressed(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("eml", DisplayName = "EML Test")]
        [DataRow("vcf", DisplayName = "VCF Test")]
        public void CanDetectOther(string extension)
        {
            DetectType(extension);
        }

        private void DetectType(string extension)
        {
            DetectType(extension, result =>
            {
                Assert.IsNotNull(result);
                Assert.IsTrue(
                    result.FileType.Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                    result.Alias?.Any(a => a.Equals(extension, StringComparison.OrdinalIgnoreCase)) == true,
                    "{0} and/or {1} do not equal {2}",
                    result.FileType, result.Alias?.FirstOrDefault(), extension);
            });
        }

        private void DetectType(string extension, Action<FileTypeInfo> assertionValidator)
        {
            var files = GetFilesByExtension(extension);
            foreach (var file in files)
            {
                var fileContents = File.ReadAllBytes(file);

                var result = fileTypeInterrogator.DetectType(fileContents);

                assertionValidator(result);
            }
        }

        private string GetFileByType(string type)
        {
            return Path.Combine(GetTestFileDirectory(), $"{type}.{type}");
        }

        private IEnumerable<string> GetFilesByExtension(string type)
        {
            // GetFiles with searchPattern returns 4 character extensions when
            // filtering for 3 so we'll filter ourselves
            return Directory.GetFiles(GetTestFileDirectory(), $"*.{type}")
                .Where(path => path.EndsWith(type, StringComparison.OrdinalIgnoreCase));
        }

        private string GetTestFileDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");
        }
    }
}
