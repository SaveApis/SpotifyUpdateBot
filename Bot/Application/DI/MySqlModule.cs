using System.Reflection;
using Autofac;
using Bot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Serilog;
using Module = Autofac.Module;

namespace Bot.Application.DI;

public class MySqlModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t =>
            {
                var interfaces = t.GetInterfaces();
                var isContextFactory = interfaces.Any(it =>
                    it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>));

                return isContextFactory;
            })
            .AsImplementedInterfaces()
            .As<IDesignTimeDbContextFactory<DbContext>>();

        builder.RegisterType<DatabaseFactory>().As<IDatabaseFactory>();

        builder.RegisterBuildCallback(scope =>
        {
            var logger = scope.Resolve<ILogger>();
            var factories = scope.Resolve<IEnumerable<IDesignTimeDbContextFactory<DbContext>>>();

            foreach (var factory in factories)
            {
                logger.Information("{Type}: Migrate", factory.GetType().Name);
                using var context = factory.CreateDbContext([]);
                context.Database.Migrate();
            }
        });
    }
}