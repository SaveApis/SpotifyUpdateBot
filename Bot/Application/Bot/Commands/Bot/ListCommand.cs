using Bot.Infrastructure.Bot;
using Bot.Infrastructure.Persistence;
using Bot.Infrastructure.Spotify;
using Bot.Persistence.Sql;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.Bot.Commands.Bot;

public class ListCommand(
    DiscordSocketClient discordClient,
    IDatabaseFactory databaseFactory,
    ISpotifyClient spotifyClient)
    : BotCommand(discordClient, databaseFactory, spotifyClient)
{
    public override string Name => "list";
    public override string Description => "Listet alle Playlists auf die getrackt werden!";

    protected override void Configure()
    {
        WithPermission(GuildPermission.Administrator);
    }

    protected override async Task ExecuteInternal(SocketSlashCommand command)
    {
        var guildId = command.GuildId ?? throw new InvalidOperationException("GuildId not found");
        
        var context = DatabaseFactory.Create<DataContext>();
        var channels = await context.Channels.Where(it => it.DiscordGuild == guildId).ToListAsync();
        
        var embed = BaseEmbed.WithTitle("Playlists");
        
        if (channels.Count == 0)
        {
            embed.AddField("Keine Playlists gefunden", "Es wurden keine Playlists gefunden!");
            await command.RespondAsync(embed: embed.Build(), ephemeral: true);
            return;
        }

        foreach (var channel in channels)
        {
            var playlist = await SpotifyClient.GetPlaylistAsync(channel.SpotifyPlaylistId);
            embed.AddField($"**{playlist.Name}**", $"https://open.spotify.com/playlist/{playlist.Id}");
        }

        await command.RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}