using Bot.Application.Models.Sql;
using Bot.Infrastructure.Bot;
using Bot.Infrastructure.Persistence;
using Bot.Infrastructure.Spotify;
using Bot.Persistence.Sql;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.Bot.Commands.Bot;

public class AddCommand(DiscordSocketClient discordClient, IDatabaseFactory databaseFactory, ISpotifyClient spotifyClient) : BotCommand(discordClient, databaseFactory, spotifyClient)
{
    public override string Name => "add";
    public override string Description => "Fügt einen Playlist hinzu die getrackt werden soll!";

    protected override void Configure()
    {
        WithOption(builder => builder.WithName("channel")
            .WithDescription("Der Discord Channel wo die Nachrichten gepostet werden sollen!")
            .WithType(ApplicationCommandOptionType.Channel)
            .WithRequired(true));
        WithOption(builder => builder.WithName("playlist").WithDescription("Die Spotify Playlist ID")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)); 
        WithPermission(GuildPermission.Administrator);
    }

    protected override async Task ExecuteInternal(SocketSlashCommand command)
    {
        var guildId = command.GuildId ?? throw new InvalidOperationException("GuildId not found");
        var spotifyPlaylistId = command.Data.Options.First(it => it.Name == "playlist").Value as string ??
                                throw new InvalidOperationException("SpotifyPlaylistId not found");

        var context = DatabaseFactory.Create<DataContext>();
        var channels = await context.Channels.Where(it => it.DiscordGuild == guildId).ToListAsync();
        
        if (channels.Any(it => it.SpotifyPlaylistId == spotifyPlaylistId))
        {
            await command.RespondAsync(embed: BaseEmbed.AddField("Playlist bereits hinzugefügt",
                "Die Playlist ist bereits hinzugefügt!").Build(), ephemeral: true);
            return;
        }

        var discordChannel =
            (SocketTextChannel)command.Data.Options.First(it => it.Name == "channel").Value as
            SocketGuildChannel ?? throw new InvalidOperationException("DiscordChannel not found");
        var playlist = await SpotifyClient.GetPlaylistAsync(spotifyPlaylistId);

        var channel = ChannelEntity.Create(guildId, discordChannel.Id, spotifyPlaylistId);
        context.Channels.Add(channel);
        await context.SaveChangesAsync();

        var addedEmbed = BaseEmbed.AddField("Playlist hinzugefügt",
            $"**{playlist.Name}** / https://open.spotify.com/playlist/{playlist.Id}");

        await command.RespondAsync(embed: addedEmbed.Build(), ephemeral: true);
    }
}