using Autofac;
using Autofac.Extensions.DependencyInjection;
using EasyCaching.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Application.DI;

public class CachingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var host = Environment.GetEnvironmentVariable("redis_host") ?? "localhost";
        var port = int.Parse(Environment.GetEnvironmentVariable("redis_port") ?? "6379");
        var collection = new ServiceCollection();

        collection.AddEasyCaching(options =>
        {
            options.WithJson("json");
            options.UseInMemory("memory");

            options.UseRedis(redisOptions =>
            {
                redisOptions.DBConfig.Endpoints.Add(new ServerEndPoint(host, port));
                redisOptions.DBConfig.Database = 0;
                redisOptions.SerializerName = "json";
            }, "redis");

            options.UseHybrid(cachingOptions =>
            {
                cachingOptions.TopicName = "bot";
                cachingOptions.EnableLogging = true;

                cachingOptions.LocalCacheProviderName = "memory";

                cachingOptions.DistributedCacheProviderName = "redis";
            }).WithRedisBus(busOptions =>
            {
                busOptions.Endpoints.Add(new ServerEndPoint(host, port));
                busOptions.SerializerName = "json";
                busOptions.Database = 1;
            });
        });

        builder.Populate(collection);
    }
}