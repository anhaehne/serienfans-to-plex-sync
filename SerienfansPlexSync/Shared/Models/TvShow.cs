using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SerienfansPlexSync.Shared.Models
{
    public class TvShow
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Unknown";

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("imdb_id")]
        public string ImdbId { get; set; } = "Unknown";

        [JsonPropertyName("seasons")]
        public IReadOnlyCollection<Season> Seasons { get; set; } = Array.Empty<Season>();

        public static TvShow Unknown = new TvShow();
    }
}