using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileTypeInterrogator.Tests
{
    [TestClass]
    public class FileTypeInterrogatorTests
    {
        private IFileTypeInterrogator fileTypeInterrogator;

        [TestInitialize]
        public void Init()
        {
            fileTypeInterrogator = new FileTypeInterrogator();
        }

        [DataTestMethod]
        //[DataRow("3GP", DisplayName = "3GP Test")]
        //[DataRow("7Z", DisplayName = "7Z Test")]
        //[DataRow("BMP", DisplayName = "BMP Test")]
        //[DataRow("DOC", DisplayName = "DOC Test")]
        //[DataRow("GIF", DisplayName = "GIF Test")]
        //[DataRow("JP2", DisplayName = "JP2 Test")]
        //[DataRow("JPG", DisplayName = "JPG Test")]
        //[DataRow("MP3", DisplayName = "MP3 Test")]
        //[DataRow("MP4", DisplayName = "MP4 Test")]
        //[DataRow("PDF", DisplayName = "PDF Test")]
        //[DataRow("PNG", DisplayName = "PNG Test")]
        [DataRow("PPT", DisplayName = "PPT Test")]
        [DataRow("PPTX", DisplayName = "PPTX Test")]
        [DataRow("PSD", DisplayName = "PSD Test")]
        [DataRow("TIF", DisplayName = "TIF Test")]
        [DataRow("XLS", DisplayName = "XLS Test")]
        [DataRow("XLSX", DisplayName = "XLSX Test")]
        [DataRow("ZIP", DisplayName = "ZIP Test")]
        public void CanDetectType(string fileType)
        {
            var filePath = GetFileByType(fileType);
            var fileContents = File.ReadAllBytes(filePath);

            var result = fileTypeInterrogator.DetectType(fileContents);

            Assert.IsNotNull(result);
            Assert.IsTrue(
                result.FileType.Equals(fileType, StringComparison.OrdinalIgnoreCase) ||
                result.Alias?.Any(a => a.Equals(fileType, StringComparison.OrdinalIgnoreCase)) == true);
        }

        private string GetFileByType(string type)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", $"{type}.{type}");
        }
    }
}
