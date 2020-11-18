using System.Text.Json.Serialization;

namespace SerienfansPlexSync.Shared.Models
{
    public class Episode
    {
        [JsonPropertyName("episode_number")]
        public int EpisodeNumber { get; set; }

        [JsonPropertyName("seasons_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = "Unknown";
    }
}