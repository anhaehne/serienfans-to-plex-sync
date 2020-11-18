using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SerienfansPlexSync.Shared.Models
{
    public class SerienfansEpisode
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("mirrors")]
        public IReadOnlyCollection<SerienfansMirror> Mirrors { get; set; } = Array.Empty<SerienfansMirror>();
    }
}