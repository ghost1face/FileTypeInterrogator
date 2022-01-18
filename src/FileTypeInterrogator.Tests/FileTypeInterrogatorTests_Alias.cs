using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace FileTypeInterrogator.Tests
{
    [TestClass]
    public partial class FileTypeInterrogatorTests
    {
        private IFileTypeInterrogator fileTypeInterrogator;

        [TestInitialize]
        public void Init()
        {
            fileTypeInterrogator = new FileTypeInterrogator();
        }

        [TestMethod]
        public void CanDetectAlias_Jpg()
        {
            var filePath = GetFileByType("jpg");
            var fileContents = File.ReadAllBytes(filePath);

            var result = fileTypeInterrogator.IsType(fileContents, "jpg");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanDetectAlias_Jpeg()
        {
            var filePath = GetFileByType("jpg");
            var fileContents = File.ReadAllBytes(filePath);

            var result = fileTypeInterrogator.IsType(fileContents, "jpeg");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanDetectJpg_By_MimeType()
        {
            var filePath = GetFileByType("jpg");
            var fileContents = File.ReadAllBytes(filePath);

            var result = fileTypeInterrogator.IsType(fileContents, "image/jpeg");

            Assert.IsTrue(result);
        }
    }
}
