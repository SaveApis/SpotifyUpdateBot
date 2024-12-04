using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Bot.Application.Models.Dto;
using Bot.Infrastructure.Spotify;
using EasyCaching.Core;
using Microsoft.Extensions.Configuration;

namespace Bot.Application.Spotify;

public class SpotifyClient(
    IConfiguration configuration,
    IHybridCachingProvider provider,
    IHttpClientFactory factory)
    : ISpotifyClient
{
    public async Task<string> LoginAsync()
    {
        var cacheValue = await provider.GetAsync<string>("spotify_access_token");
        if (cacheValue.HasValue) return cacheValue.Value;

        var clientId = configuration["spotify_client_id"] ??
                       throw new InvalidOperationException("Spotify ClientId not found");
        var clientSecret = configuration["spotify_client_secret"] ??
                           throw new InvalidOperationException("Spotify ClientSecret not found");

        using var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        request.Content = new StringContent(
            $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}",
            Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SpotifyLoginDto>() ??
                     throw new InvalidOperationException("Login failed!");

        await provider.SetAsync("spotify_access_token", result.AccessToken, TimeSpan.FromSeconds(result.ExpiresIn));

        return result.AccessToken;
    }

    public async Task<PlaylistDto> GetPlaylistAsync(string playlistId, bool cacheFirst = true)
    {
        if (cacheFirst)
        {
            var cacheValue = await provider.GetAsync<PlaylistDto>($"spotify_playlist-{playlistId}");
            if (cacheValue.HasValue) return cacheValue.Value;
        }

        var accessToken = await LoginAsync();

        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/playlists/{playlistId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await LoginAsync();
            return await GetPlaylistAsync(playlistId, cacheFirst);
        }

        response.EnsureSuccessStatusCode();

        var stringContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PlaylistDto>(stringContent) ??
                     throw new InvalidOperationException("Playlist not found!");

        await provider.SetAsync($"spotify_playlist-{playlistId}", result, TimeSpan.FromHours(1));

        return result;
    }
}