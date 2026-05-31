namespace FileTypeInterrogator.Benchmark.Nuget;

using System.Text.Json.Serialization;

public class NugetPackage
{
    [JsonPropertyName("id")] 
    public required string Id { get; set; }

    [JsonPropertyName("version")] 
    public required string Version { get; set; }
}
