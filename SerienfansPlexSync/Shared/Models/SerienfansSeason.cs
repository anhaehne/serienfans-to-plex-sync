using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SerienfansPlexSync.Shared.Models
{
    public class SerienfansSeason
    {
        public static SerienfansSeason Empty = new SerienfansSeason();

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("episodes")]
        public IReadOnlyCollection<SerienfansEpisode> Episodes { get; set; } = Array.Empty<SerienfansEpisode>();
    }
}