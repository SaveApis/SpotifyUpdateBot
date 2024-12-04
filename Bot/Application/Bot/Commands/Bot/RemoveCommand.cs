using Bot.Infrastructure.Bot;
using Bot.Infrastructure.Persistence;
using Bot.Infrastructure.Spotify;
using Bot.Persistence.Sql;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.Bot.Commands.Bot;

public class RemoveCommand(
    DiscordSocketClient discordClient,
    IDatabaseFactory databaseFactory,
    ISpotifyClient spotifyClient)
    : BotCommand(discordClient, databaseFactory, spotifyClient)
{
    public override string Name => "remove";
    public override string Description => "Entfernt eine Playlist die nicht mehr getrackt werden soll!";
    protected override void Configure()
    {
        WithOption(builder => builder.WithName("playlist")
            .WithDescription("Die Playlist die nicht mehr getrackt werden soll!")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true));
        WithPermission(GuildPermission.Administrator);
    }

    protected override async Task ExecuteInternal(SocketSlashCommand command)
    {
        var guildId = command.GuildId ?? throw new InvalidOperationException("GuildId not found");
        var playlist = command.Data.Options.First(option => option.Name == "playlist").Value as string ??
                       throw new InvalidOperationException("Playlist not found");

        var context = DatabaseFactory.Create<DataContext>();
        var channels = await context.Channels.Where(it => it.DiscordGuild == guildId).ToListAsync();
        if (channels.All(entity => entity.SpotifyPlaylistId != playlist))
        {
            await command.RespondAsync(embed: BaseEmbed.AddField("Playlist nicht gefunden",
                "Playlist wurde nicht gefunden!").Build(), ephemeral: true);
            return;
        }
        
        var channel = channels.First(entity => entity.SpotifyPlaylistId == playlist);
        
        context.Channels.Remove(channel);
        await context.SaveChangesAsync();
        
        var spotifyPlaylist = await SpotifyClient.GetPlaylistAsync(playlist);

        await command.RespondAsync(
            embed: BaseEmbed.AddField("Playlist entfernt",
                $"**{spotifyPlaylist.Name}** / https://open.spotify.com/playlist/{spotifyPlaylist.Id}").Build(),
            ephemeral: true);
    }
}