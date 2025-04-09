using System.IO;
using Xunit;

namespace FileTypeInterrogator.Tests
{
    public partial class FileTypeInterrogatorTests
    {
        private IFileTypeInterrogator fileTypeInterrogator;

        public FileTypeInterrogatorTests()
        {
            fileTypeInterrogator = new FileTypeInterrogator();
        }

        [Fact]
        public void CanDetectAlias_Jpg()
        {
            var filePath = GetFileByType("jpg");
            var fileContents = File.ReadAllBytes(filePath);

            var result = fileTypeInterrogator.IsType(fileContents, "jpg");

            Assert.True(result);
        }

        [Fact]
        public void CanDetectAlias_Jpeg()
        {
            var filePath = GetFileByType("jpg");
            var fileContents = File.ReadAllBytes(filePath);

            var result = fileTypeInterrogator.IsType(fileContents, "jpeg");

            Assert.True(result);
        }

        [Fact]
        public void CanDetectJpg_By_MimeType()
        {
            var filePath = GetFileByType("jpg");
            var fileContents = File.ReadAllBytes(filePath);

            var result = fileTypeInterrogator.IsType(fileContents, "image/jpeg");

            Assert.True(result);
        }
    }
}
