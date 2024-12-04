using Bot.Application.Models.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bot.Persistence.Sql.Configurations;

public class ChannelEntityConfiguration : IEntityTypeConfiguration<ChannelEntity>
{
    public void Configure(EntityTypeBuilder<ChannelEntity> builder)
    {
        builder.ToTable("Channels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiscordGuild).IsRequired();
        builder.Property(x => x.DiscordChannel).IsRequired();
        builder.Property(x => x.SpotifyPlaylistId).IsRequired();

        builder.HasIndex(x => new { x.DiscordGuild, x.SpotifyPlaylistId }).IsUnique();
    }
}