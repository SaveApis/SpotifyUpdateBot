using Quartz;
using Serilog;

namespace Bot.Infrastructure.Quartz;

public abstract class BaseJob(ILogger logger) : IJob
{
    public abstract string Group { get; }
    public abstract string Key { get; }
    public abstract TimeSpan Interval { get; }
    
    protected ILogger Logger => logger.ForContext(GetType());

    public abstract Task ExecuteInternalAsync(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        logger.Information("{Group}.{Key}: Execute", Group, Key);
        await ExecuteInternalAsync(context);
    }
}