using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace FileTypeInterrogator.Test
{
    public class Program
    {
        static void Main()
        {
            new PerformanceTest().FileInterrogatorPerformanceTest();
        }
    }

    //[SimpleJob(RuntimeMoniker.NetCoreApp20, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [MarkdownExporterAttribute.GitHub]
    public class PerformanceTest
    {
        [Fact]
        public void FileInterrogatorPerformanceTest() => BenchmarkRunner.Run<FileTypeInterrogatorBenchmark>();
    }

    public class FileTypeInterrogatorBenchmark
    {
        private FileInterrogator fileInterrogator;

        [GlobalSetup]
        public void Setup()
        {
            fileInterrogator = new FileInterrogator();
        }

        [ParamsSource(nameof(Files))]
        public string CurrentFile { get; set; }

        public IEnumerable<string> Files => Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles"), $"*.");

#if NETCOREAPP3_0
        [Benchmark]
        public FileTypeInfo InterrogateSpan()
        {
            var fileBytes = File.ReadAllBytes(CurrentFile);
            var fileSpan = new ReadOnlySpan<byte>(fileBytes);

            return fileInterrogator.DetectType(fileSpan);
        }
#endif

        [Benchmark]
        public FileTypeInfo InterrogateByte()
        {
            var fileBytes = File.ReadAllBytes(CurrentFile);

            return fileInterrogator.DetectType(fileBytes);
        }

        [Benchmark]
        public FileTypeInfo InterrogateStream()
        {
            using (var fileStream = File.OpenRead(CurrentFile))
                return fileInterrogator.DetectType(fileStream);
        }
    }
}
