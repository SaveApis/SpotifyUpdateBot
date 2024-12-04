using Bot.Application.Models.Sql;
using Microsoft.EntityFrameworkCore;

namespace Bot.Persistence.Sql;

public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ChannelEntity> Channels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}