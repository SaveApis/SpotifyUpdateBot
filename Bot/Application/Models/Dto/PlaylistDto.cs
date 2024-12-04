using System.Text.Json.Serialization;

namespace Bot.Application.Models.Dto;

public class PlaylistDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("snapshot_id")] public string SnapshotId { get; set; } = string.Empty;
    [JsonPropertyName("tracks")] public TracksDto Tracks { get; set; } = new();
}