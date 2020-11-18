using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SerienfansPlexSync.Shared.Models
{
    public class Season
    {
        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("episodes")]
        public IReadOnlyCollection<Episode> Episodes { get; set; } = Array.Empty<Episode>();
    }
}