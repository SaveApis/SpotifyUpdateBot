using System.Text.Json.Serialization;

namespace Bot.Application.Models.Dto;

public class SpotifyLoginDto
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; } = 0;
}