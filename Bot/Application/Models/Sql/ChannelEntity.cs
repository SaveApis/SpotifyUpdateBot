namespace Bot.Application.Models.Sql;

public class ChannelEntity
{
    private ChannelEntity(Guid id, ulong discordGuild, ulong discordChannel, string spotifyPlaylistId)
    {
        Id = id;
        DiscordGuild = discordGuild;
        DiscordChannel = discordChannel;
        SpotifyPlaylistId = spotifyPlaylistId;
    }

    public Guid Id { get; private set; }
    public ulong DiscordGuild { get; private set; }
    public ulong DiscordChannel { get; private set; }
    public string SpotifyPlaylistId { get; private set; }

    public static ChannelEntity Create(ulong discordGuild, ulong discordChannel, string spotifyPlaylistId)
    {
        return new ChannelEntity(Guid.NewGuid(), discordGuild, discordChannel, spotifyPlaylistId);
    }
}