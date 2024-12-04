using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Bot.Application.DI;

public class SerilogModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var collection = new ServiceCollection();

        collection.AddSerilog(configuration =>
        {
            configuration.Enrich.FromLogContext();
            configuration.WriteTo.Console(LogEventLevel.Information);
            configuration.WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);
        });

        builder.Populate(collection);
    }
}