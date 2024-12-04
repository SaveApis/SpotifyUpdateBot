using System.Text.Json.Serialization;

namespace Bot.Application.Models.Dto;

public class TracksDto
{
    [JsonPropertyName("items")] public ICollection<ItemDto> Items { get; set; } = [];
}