using Bot.Infrastructure.Bot;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Bot.Application.Bot.HostedServices;

public class BotService(
    ILogger logger,
    DiscordSocketClient discordClient,
    IConfiguration configuration,
    IEnumerable<BotCommand> commands)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = configuration["token"] ?? throw new InvalidOperationException("Token not found in configuration");
        discordClient.Log += LogAsync;
        discordClient.Ready += ReadyAsync;
        discordClient.SlashCommandExecuted += SlashCommandExecutedAsync;

        await discordClient.LoginAsync(TokenType.Bot, token);
        await discordClient.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await discordClient.StopAsync();
    }

    private Task LogAsync(LogMessage arg)
    {
        var logLevel = arg.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };

        logger.Write(logLevel, arg.Exception, arg.Message);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        var globalCommands = await discordClient.GetGlobalApplicationCommandsAsync();
        foreach (var command in globalCommands)
        {
            await command.DeleteAsync();
        }

        await RegisterCommands();
    }

    private async Task RegisterCommands()
    {
        foreach (var guild in discordClient.Guilds)
        {
            foreach (var command in commands)
            {
                logger.Information("Registering command {CommandName}", command.Name);

                var builder = new SlashCommandBuilder
                {
                    Name = command.Name.ToLowerInvariant(),
                    Description = command.Description,
                    Options = [..command.Options],
                    DefaultMemberPermissions = command.DefaultPermission
                };

                await guild.CreateApplicationCommandAsync(builder.Build());
            }
        }
    }

    private async Task SlashCommandExecutedAsync(SocketSlashCommand arg)
    {
        var command = commands.FirstOrDefault(x => x.Name.Equals(arg.Data.Name, StringComparison.OrdinalIgnoreCase));
        if (command is null)
        {
            logger.Warning("Command {CommandName} not found", arg.Data.Name);
            return;
        }

        await command.ExecuteAsync(arg);
    }
}