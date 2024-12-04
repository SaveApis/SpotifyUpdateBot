using System.Text.Json.Serialization;

namespace Bot.Application.Models.Dto;

public class ArtistDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}