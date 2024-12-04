using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bot.Infrastructure.Persistence;

public class DatabaseFactory(ILifetimeScope scope) : IDatabaseFactory
{
    public TContext Create<TContext>() where TContext : DbContext
    {
        var factory = scope.Resolve<IDesignTimeDbContextFactory<TContext>>();
        var context = factory.CreateDbContext([]);

        return context;
    }
}