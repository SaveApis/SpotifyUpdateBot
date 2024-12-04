using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Persistence.Sql.Migrations
{
    /// <inheritdoc />
    public partial class CreateChannelsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DiscordGuild = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    DiscordChannel = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SpotifyPlaylistId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_DiscordGuild_SpotifyPlaylistId",
                table: "Channels",
                columns: new[] { "DiscordGuild", "SpotifyPlaylistId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Channels");
        }
    }
}
