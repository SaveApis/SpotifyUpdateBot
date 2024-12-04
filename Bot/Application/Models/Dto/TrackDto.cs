using System.Text.Json.Serialization;

namespace Bot.Application.Models.Dto;

public class TrackDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("artists")] public List<ArtistDto> Artists { get; set; } = [];
}