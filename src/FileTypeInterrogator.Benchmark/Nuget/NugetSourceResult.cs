using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FileTypeInterrogator.Benchmark.Nuget;

public class NugetSourceResult
{
    [JsonPropertyName("sourceName")] 
    public required string SourceName { get; set; }

    [JsonPropertyName("packages")] 
    public List<NugetPackage> Packages { get; set; } = new List<NugetPackage>();
}
