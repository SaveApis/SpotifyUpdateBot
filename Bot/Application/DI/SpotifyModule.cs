using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bot.Application.Spotify;
using Bot.Infrastructure.Spotify;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Application.DI;

public class SpotifyModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var collection = new ServiceCollection();

        collection.AddHttpClient();

        builder.Populate(collection);

        builder.RegisterType<SpotifyClient>().As<ISpotifyClient>().InstancePerLifetimeScope();
    }
}