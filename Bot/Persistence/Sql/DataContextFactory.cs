using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Bot.Persistence.Sql;

public class DataContextFactory(IConfiguration configuration) : IDesignTimeDbContextFactory<DataContext>
{
    public DataContextFactory() : this(new ConfigurationBuilder().AddInMemoryCollection().Build())
    {
    }

    public DataContext CreateDbContext(string[] args)
    {
        var host = configuration["db_host"] ?? "localhost";
        var port = configuration["db_port"] ?? "3306";
        var user = configuration["db_user"] ?? "root";
        var password = configuration["db_password"] ?? "root";
        var database = configuration["db_database"] ?? "bot";

        var connectionStringBuilder = new MySqlConnectionStringBuilder
        {
            Database = database,
            Pooling = true,
            Port = uint.Parse(port),
            Server = host,
            Password = password,
            Pipelining = true,
            ApplicationName = "Bot",
            UseCompression = true,
            AllowUserVariables = true,
            BrowsableConnectionString = true,
            UserID = user
        };

        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseMySql(connectionStringBuilder.ToString(), MySqlServerVersion.LatestSupportedServerVersion);
        return new DataContext(builder.Options);
    }
}