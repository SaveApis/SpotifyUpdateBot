using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bot.Infrastructure.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using Module = Autofac.Module;

namespace Bot.Application.DI;

public class QuartzModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var services = new ServiceCollection();

        services.AddQuartz();
        services.AddQuartzHostedService(options =>
        {
            options.AwaitApplicationStarted = true;
            options.WaitForJobsToComplete = true;
        });

        builder.Populate(services);

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.IsAssignableTo<BaseJob>())
            .As<BaseJob>()
            .AsImplementedInterfaces();

        builder.RegisterBuildCallback(async void (scope) =>
        {
            var logger = scope.Resolve<ILogger>();
            var factory = scope.Resolve<ISchedulerFactory>();
            var scheduler = await factory.GetScheduler();

            var quartzJobs = scope.Resolve<IEnumerable<BaseJob>>();
            foreach (var quartzJob in quartzJobs)
            {
                logger.Information("{Type}: Register Job ({Group}:{Key}, {Interval})", quartzJob.GetType().Name,
                    quartzJob.Group, quartzJob.Key, quartzJob.Interval);
                var job = JobBuilder.Create(quartzJob.GetType())
                    .WithIdentity(quartzJob.Key, quartzJob.Group)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(quartzJob.Key, quartzJob.Group)
                    .WithSimpleSchedule(x => x
                        .WithInterval(quartzJob.Interval)
                        .RepeatForever())
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
        });
    }
}