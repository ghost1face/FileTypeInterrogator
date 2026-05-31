using System.Collections.Generic;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace FileTypeInterrogator.Benchmark;

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig(IEnumerable<string> latestPackageVersionNumbers)
    {
        var framework = CoreRuntime.Core10_0;

        AddExporter(MarkdownExporter.GitHub);
        AddExporter(CsvExporter.Default);
        AddExporter(JsonExporter.Default);

        AddLogger(ConsoleLogger.Default);

        AddColumnProvider(DefaultColumnProviders.Instance);

        AddDiagnoser(MemoryDiagnoser.Default);

        // run benchmarks with the latest nuget package version
        foreach (var versionNumber in latestPackageVersionNumbers)
        {
            var nugetPackageJob = Job.ShortRun
                .WithRuntime(framework)
                .WithMsBuildArguments($"/p:NugetPackageVersion={versionNumber}")
                .WithId($"v{versionNumber}");

            AddJob(nugetPackageJob);
        }

        // job for locally referenced project (current build)
        var currentProjectJob = Job.ShortRun
            .WithRuntime(framework)
            .WithId("current");

        AddJob(currentProjectJob);
    }
}
