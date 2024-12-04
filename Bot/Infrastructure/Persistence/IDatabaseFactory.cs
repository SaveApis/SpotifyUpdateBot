using Microsoft.EntityFrameworkCore;

namespace Bot.Infrastructure.Persistence;

public interface IDatabaseFactory
{
    TContext Create<TContext>() where TContext : DbContext;
}