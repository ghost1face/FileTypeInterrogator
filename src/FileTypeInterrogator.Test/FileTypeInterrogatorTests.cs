using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace FileTypeInterrogator.Tests
{
    public partial class FileTypeInterrogatorTests
    {
        [Fact]
        public void CanDetectAscii()
        {
            const string extension = "ascii";
            DetectType(extension, result =>
            {
                Assert.NotNull(result);
                Assert.StartsWith(extension, result.Name, StringComparison.OrdinalIgnoreCase);
            });
        }

        [Fact]
        public void CanDetectUTF8()
        {
            const string extension = "utf8";
            DetectType(extension, result =>
            {
                Assert.NotNull(result);
                Assert.StartsWith("UTF-8", result.Name, StringComparison.OrdinalIgnoreCase);
                Assert.True(result.Name.IndexOf("BOM", StringComparison.OrdinalIgnoreCase) == -1);
            });
        }

        [Fact]
        public void CanDetectUTF8BOM()
        {
            const string extension = "utf8bom";
            DetectType(extension, result =>
            {
                Assert.NotNull(result);
                Assert.StartsWith("UTF-8", result.Name, StringComparison.OrdinalIgnoreCase);
                Assert.True(result.Name.IndexOf("BOM", StringComparison.OrdinalIgnoreCase) > -1);
            });
        }

        [Theory]
        [InlineData("PDF")]
        [InlineData("FDF")]
        public void CanDetectAdobe(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("BMP")]
        [InlineData("GIF")]
        [InlineData("ICO")]
        [InlineData("JP2")]
        [InlineData("JPG")]
        [InlineData("PNG")]
        [InlineData("PSD")]
        [InlineData("TIF")]
        public void CanDetectImages(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("3GP")]
        [InlineData("AVI")]
        [InlineData("FLV")]
        [InlineData("MID")]
        [InlineData("MP4")]
        [InlineData("WMV")]
        public void CanDetectVideo(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("AC3")]
        [InlineData("AIFF")]
        [InlineData("FLAC")]
        [InlineData("MP3")]
        [InlineData("OGG")]
        [InlineData("RA")]
        public void CanDetectAudio(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("DOC")]
        [InlineData("DOCX")]
        [InlineData("PPT")]
        [InlineData("PPTX")]
        [InlineData("XLS")]
        [InlineData("XLSX")]
        public void CanDetectOffice(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("OTF")]
        [InlineData("TTF")]
        [InlineData("WOFF")]
        public void CanDetectFont(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("7Z")]
        [InlineData("RAR")]
        [InlineData("ZIP")]
        public void CanDetectCompressed(string extension)
        {
            DetectType(extension);
        }

        private void DetectType(string extension)
        {
            DetectType(extension, result =>
            {
                Assert.NotNull(result);
                Assert.True(
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
