using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bot.Application.Bot.HostedServices;
using Bot.Infrastructure.Bot;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Module = Autofac.Module;

namespace Bot.Application.DI;

public class BotModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var collection = new ServiceCollection();

        collection.AddHostedService<BotService>();

        builder.Populate(collection);

        var options = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Debug,
            GatewayIntents = GatewayIntents.AllUnprivileged,
            AlwaysDownloadUsers = true,
            DefaultRetryMode = RetryMode.RetryTimeouts
        };

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).Where(t => t.IsAssignableTo<BotCommand>())
            .As<BotCommand>();

        builder.Register<DiscordSocketConfig>(_ => options).AsSelf().SingleInstance();

        builder.RegisterType<DiscordSocketClient>().AsSelf().SingleInstance();
    }
}