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
        [DataRow("PDF", DisplayName = "PDF Test")]
        [DataRow("FDF", DisplayName = "FDF Test")]
        public void CanDetectAdobe(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("BMP", DisplayName = "BMP Test")]
        [DataRow("GIF", DisplayName = "GIF Test")]
        [DataRow("ICO", DisplayName = "ICO Test")]
        [DataRow("JP2", DisplayName = "JP2 Test")]
        [DataRow("JPG", DisplayName = "JPG Test")]
        [DataRow("PNG", DisplayName = "PNG Test")]
        [DataRow("PSD", DisplayName = "PSD Test")]
        [DataRow("TIF", DisplayName = "TIF Test")]
        public void CanDetectImages(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("3GP", DisplayName = "3GP Test")]
        [DataRow("AVI", DisplayName = "AVI Test")]
        [DataRow("FLV", DisplayName = "FLV Test")]
        [DataRow("MID", DisplayName = "MID Test")]
        [DataRow("MP4", DisplayName = "MP4 Test")]
        [DataRow("WMV", DisplayName = "WMV Test")]
        public void CanDetectVideo(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("AC3", DisplayName = "AC3 Test")]
        [DataRow("AIFF", DisplayName = "AIFF Test")]
        [DataRow("FLAC", DisplayName = "FLAC Test")]
        [DataRow("MP3", DisplayName = "MP3 Test")]
        [DataRow("OGG", DisplayName = "OGG Test")]
        [DataRow("RA", DisplayName = "RA Test")]
        public void CanDetectAudio(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("DOC", DisplayName = "DOC Test")]
        [DataRow("DOCX", DisplayName = "DOC Test")]
        [DataRow("PPT", DisplayName = "PPT Test")]
        [DataRow("PPTX", DisplayName = "PPTX Test")]
        [DataRow("XLS", DisplayName = "XLS Test")]
        [DataRow("XLSX", DisplayName = "XLSX Test")]
        public void CanDetectOffice(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("OTF", DisplayName = "OTF Test")]
        [DataRow("TTF", DisplayName = "TTF Test")]
        [DataRow("WOFF", DisplayName = "WOFF Test")]
        public void CanDetectFont(string extension)
        {
            DetectType(extension);
        }

        [DataTestMethod]
        [DataRow("7Z", DisplayName = "7Z Test")]
        [DataRow("RAR", DisplayName = "RAR Test")]
        [DataRow("ZIP", DisplayName = "ZIP Test")]
        public void CanDetectCompressed(string extension)
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
                    result.Alias?.Any(a => a.Equals(extension, StringComparison.OrdinalIgnoreCase)) == true);
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
