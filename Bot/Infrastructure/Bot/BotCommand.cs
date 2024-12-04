using Bot.Infrastructure.Persistence;
using Bot.Infrastructure.Spotify;
using Discord;
using Discord.WebSocket;

namespace Bot.Infrastructure.Bot;

public abstract class BotCommand
{
    protected BotCommand(DiscordSocketClient discordClient,
        IDatabaseFactory databaseFactory,
        ISpotifyClient spotifyClient)
    {
        DiscordClient = discordClient;
        DatabaseFactory = databaseFactory;
        SpotifyClient = spotifyClient;
        Configure();
    }

    public abstract string Name { get; }
    public abstract string Description { get; }

    public ICollection<SlashCommandOptionBuilder> Options { get; } = [];
    public GuildPermission? DefaultPermission { get; private set; }

    protected DiscordSocketClient DiscordClient { get; }
    protected IDatabaseFactory DatabaseFactory { get; }
    protected ISpotifyClient SpotifyClient { get; }

    protected EmbedBuilder BaseEmbed => new EmbedBuilder()
        .WithAuthor(DiscordClient.CurrentUser.GlobalName, DiscordClient.CurrentUser.GetAvatarUrl())
        .WithColor(new Color(30, 215, 96)).WithFooter(builder =>
            builder.WithIconUrl(
                "https://upload.wikimedia.org/wikipedia/commons/thumb/1/19/Spotify_logo_without_text.svg/200px-Spotify_logo_without_text.svg.png"));

    protected abstract void Configure();
    protected abstract Task ExecuteInternal(SocketSlashCommand command);

    public async Task ExecuteAsync(SocketSlashCommand command)
    {
        if (command.Data.Name != Name) return;

        await ExecuteInternal(command);
    }

    protected void WithOption(Action<SlashCommandOptionBuilder> configure)
    {
        var option = new SlashCommandOptionBuilder();
        configure(option);
        Options.Add(option);
    }

    protected void WithPermission(GuildPermission permission)
    {
        DefaultPermission = permission;
    }
}