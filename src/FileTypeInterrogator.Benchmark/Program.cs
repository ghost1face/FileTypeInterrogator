using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using FileTypeInterrogator.Benchmark.Nuget;

namespace FileTypeInterrogator.Benchmark;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var latestVersions = await GetLatestPackageVersionNumbers("FileTypeInterrogator", topN: 1);

        _ = BenchmarkRunner.Run<FileTypeInterrogatorBenchmark>(new BenchmarkConfig(latestVersions));
    }

    private static async Task<IEnumerable<string>> GetLatestPackageVersionNumbers(string packageName, int topN = 1,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"package search {packageName} --format json --exact-match",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using Process? process = Process.Start(startInfo);

        if (process == null) throw new InvalidOperationException("Failed to start dotnet process");

        await process.WaitForExitAsync(cancellationToken);

        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

        var packages = JsonSerializer.Deserialize<NugetSearchResultRoot>(output);

        var latestPackages = packages?.SearchResult.SelectMany(sr => sr.Packages)
            .OrderByDescending(p => Version.Parse(p.Version))
            .Take(topN);

        return latestPackages?.Select(p => p.Version).ToList() ?? new List<string>();
    }
}
