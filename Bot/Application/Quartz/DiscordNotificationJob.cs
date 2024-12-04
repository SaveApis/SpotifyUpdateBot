using System.Collections.ObjectModel;
using Bot.Application.Models.Dto;
using Bot.Infrastructure.Persistence;
using Bot.Infrastructure.Quartz;
using Bot.Infrastructure.Spotify;
using Bot.Persistence.Sql;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

namespace Bot.Application.Quartz;

[DisallowConcurrentExecution]
public class DiscordNotificationJob(
    ILogger logger,
    DiscordSocketClient discordClient,
    ISpotifyClient spotifyClient,
    IDatabaseFactory databaseFactory) : BaseJob(logger)
{
    public override string Group => "discord";
    public override string Key => "notification";
    public override TimeSpan Interval => TimeSpan.FromMinutes(2);

    public override async Task ExecuteInternalAsync(IJobExecutionContext context)
    {
        var databaseContext = databaseFactory.Create<DataContext>();

        foreach (var guild in discordClient.Guilds)
        {
            var channels = await databaseContext.Channels
                .Where(it => it.DiscordGuild == guild.Id).ToListAsync();

            foreach (var channel in channels)
            {
                var cachedPlaylist = await spotifyClient.GetPlaylistAsync(channel.SpotifyPlaylistId);
                var spotifyPlaylist = await spotifyClient.GetPlaylistAsync(channel.SpotifyPlaylistId, false);
                if (string.Equals(cachedPlaylist.SnapshotId, spotifyPlaylist.SnapshotId,
                        StringComparison.InvariantCultureIgnoreCase)) continue;

                var cachedTracks = cachedPlaylist.Tracks.Items.Select(it => it.Track.Id).ToList();
                var spotifyTracks = spotifyPlaylist.Tracks.Items.Select(it => it.Track.Id).ToList();

                var addedTracks = spotifyTracks.Except(cachedTracks).ToList();
                var removedTracks = cachedTracks.Except(spotifyTracks).ToList();
                
                Logger.Verbose("Tracks changes ({GuildName}: {AddedTracks} added, {RemovedTracks} removed",
                    guild.Name, addedTracks.Count, removedTracks.Count);

                var addedTracksDetails = spotifyPlaylist.Tracks.Items
                    .Where(it => addedTracks.Contains(it.Track.Id))
                    .Select(it => it.Track)
                    .ToList();
                var removedTracksDetails = cachedPlaylist.Tracks.Items
                    .Where(it => removedTracks.Contains(it.Track.Id))
                    .Select(it => it.Track)
                    .ToList();

                var channelObject = guild.GetTextChannel(channel.DiscordChannel);
                var embeds = GenerateEmbed(addedTracksDetails, removedTracksDetails);
                await channelObject.SendMessageAsync(embeds: [..embeds]);
            }
        }
    }

    private Collection<Embed> GenerateEmbed(List<TrackDto> addedTracksDetails,
        List<TrackDto> removedTracksDetails)
    {
        var embeds = new Collection<Embed>();

        if (addedTracksDetails.Count > 0)
        {
            var addedEmbedBuilder = new EmbedBuilder()
                .WithAuthor(discordClient.CurrentUser.GlobalName, discordClient.CurrentUser.GetAvatarUrl())
                .WithColor(new Color(30, 215, 96))
                .WithFooter(builder => builder.WithText("Hinzugefügte Tracks").WithIconUrl(
                    "https://upload.wikimedia.org/wikipedia/commons/thumb/1/19/Spotify_logo_without_text.svg/200px-Spotify_logo_without_text.svg.png"));
            foreach (var addedTrack in addedTracksDetails)
            {
                addedEmbedBuilder.AddField("Hinzugefügter Track",
                    $"**{addedTrack.Name}** / https://open.spotify.com/track/{addedTrack.Id} / {string.Join(", ", addedTrack.Artists.Select(it => it.Name))}",
                    true);
            }

            embeds.Add(addedEmbedBuilder.Build());
        }

        if (removedTracksDetails.Count <= 0) return embeds;

        var removedEmbedBuilder = new EmbedBuilder()
            .WithAuthor(discordClient.CurrentUser.GlobalName, discordClient.CurrentUser.GetAvatarUrl())
            .WithColor(new Color(30, 215, 96))
            .WithFooter(builder => builder.WithText("Entfernte Tracks").WithIconUrl(
                "https://upload.wikimedia.org/wikipedia/commons/thumb/1/19/Spotify_logo_without_text.svg/200px-Spotify_logo_without_text.svg.png"));
        foreach (var removedTrack in removedTracksDetails)
        {
            removedEmbedBuilder.AddField("Entfernter Track",
                $"**{removedTrack.Name}** / https://open.spotify.com/track/{removedTrack.Id} / {string.Join(", ", removedTrack.Artists.Select(it => it.Name))}",
                true);
        }

        embeds.Add(removedEmbedBuilder.Build());

        return embeds;
    }
}