using Bot.Application.Models.Dto;

namespace Bot.Infrastructure.Spotify;

public interface ISpotifyClient
{
    Task<string> LoginAsync(bool cacheFirst = true);
    Task<PlaylistDto> GetPlaylistAsync(string playlistId, bool cacheFirst = true);
}