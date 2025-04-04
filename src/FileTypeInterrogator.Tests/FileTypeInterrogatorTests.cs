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
        [InlineData("pdf")]
        [InlineData("fdf")]
        public void CanDetectAdobe(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("ai")]
        [InlineData("bmp")]
        [InlineData("gif")]
        [InlineData("ico")]
        [InlineData("jp2")]
        [InlineData("jpg")]
        [InlineData("pcx")]
        [InlineData("png")]
        [InlineData("psd")]
        [InlineData("tif")]
        [InlineData("webp")]
        public void CanDetectImages(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("3gp")]
        [InlineData("avi")]
        [InlineData("flv")]
        [InlineData("mid")]
        [InlineData("mkv")]
        [InlineData("mp4")]
        [InlineData("webm")]
        [InlineData("wmv")]
        public void CanDetectVideo(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("ac3")]
        [InlineData("aiff")]
        [InlineData("flac")]
        [InlineData("mp3")]
        [InlineData("ogg")]
        [InlineData("ra")]
        [InlineData("wav")]
        public void CanDetectAudio(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("doc")]
        [InlineData("docx")]
        [InlineData("odp")]
        [InlineData("odt")]
        [InlineData("ppt")]
        [InlineData("pptx")]
        [InlineData("rtf")]
        [InlineData("xls")]
        [InlineData("xlsx")]
        public void CanDetectOffice(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("otf")]
        [InlineData("ttf")]
        [InlineData("woff")]
        public void CanDetectFont(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("7z")]
        [InlineData("gz")]
        [InlineData("rar")]
        [InlineData("zip")]
        public void CanDetectCompressed(string extension)
        {
            DetectType(extension);
        }

        [Theory]
        [InlineData("eml")]
        [InlineData("vcf")]
        public void CanDetectOther(string extension)
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
                    result.Alias?.Any(a => a.Equals(extension, StringComparison.OrdinalIgnoreCase)) == true,
                    string.Format("{0} and/or {1} do not equal {2}",
                    result.FileType, result.Alias?.FirstOrDefault(), extension));
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
