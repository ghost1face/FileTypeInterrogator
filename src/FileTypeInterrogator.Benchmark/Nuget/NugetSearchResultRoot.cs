using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FileTypeInterrogator.Benchmark.Nuget;

public class NugetSearchResultRoot
{
    [JsonPropertyName("version")] 
    public int Version { get; set; }

    [JsonPropertyName("problems")] 
    public List<object> Problems { get; set; } = new List<object>();

    [JsonPropertyName("searchResult")]
    public List<NugetSourceResult> SearchResult { get; set; } = new List<NugetSourceResult>();
}
