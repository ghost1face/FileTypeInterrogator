using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace FileTypeInterrogator.Benchmark;

[Orderer(SummaryOrderPolicy.Method)]
public class FileTypeInterrogatorBenchmark
{
    private readonly global::FileTypeInterrogator.FileTypeInterrogator _fileTypeInterrogator = new();

    [Benchmark]
    public void DetectTypes_WithBytes()
    {
        DetectAll(false);
    }

    [Benchmark]
    public void DetectTypes_WithStream()
    {
        DetectAll(true);
    }

    private void DetectAll(bool useStream)
    {
        DetectType("bmp", useStream);
        DetectType("doc", useStream);
        DetectType("mkv", useStream);
        DetectType("pdf", useStream);
        DetectType("png", useStream);
        DetectType("txt", useStream);
        DetectType("wav", useStream);
        DetectType("wmv", useStream);
        DetectType("xls", useStream);
        DetectType("xml", useStream);
        DetectType("zip", useStream);
    }

    private void DetectType(string extension, bool useStream = false)
    {
        DetectType(extension, result =>
        {
            ArgumentNullException.ThrowIfNull(result);

            if (!(result.FileType.Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                  result.Alias?.Any(a => a.Equals(extension, StringComparison.OrdinalIgnoreCase)) == true))
            {
                throw new InvalidOperationException(string.Format("{0} and/or {1} do not equal {2}",
                    result.FileType, result.Alias?.FirstOrDefault(), extension));
            }
        }, useStream);
    }

    private void DetectType(string extension, Action<FileTypeInfo> assertionValidator, bool useStream = false)
    {
        var files = GetFilesByExtension(extension);
        foreach (var file in files)
        {
            if (!useStream)
            {
                var fileContents = File.ReadAllBytes(file);

                var result = _fileTypeInterrogator.DetectType(fileContents);

                assertionValidator(result);
            }
            else
            {
                using var stream = File.OpenRead(file);

                var result = _fileTypeInterrogator.DetectType(stream);

                assertionValidator(result);
            }
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
