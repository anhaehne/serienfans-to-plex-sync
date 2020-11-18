using System.Text.Json.Serialization;

namespace SerienfansPlexSync.Shared.Models
{
    public class SerienfansMirror
    {
        [JsonPropertyName("mirror_id")]
        public string MirrorId { get; set; } = "<invalid>";

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;
    }
}