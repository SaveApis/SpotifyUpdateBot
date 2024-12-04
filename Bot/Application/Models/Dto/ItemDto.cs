using System.Text.Json.Serialization;

namespace Bot.Application.Models.Dto;

public class ItemDto
{
    [JsonPropertyName("added_at")] public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("track")] public TrackDto Track { get; set; } = new();
}