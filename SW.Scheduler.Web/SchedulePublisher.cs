using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SW.PrimitiveTypes;

namespace SW.Scheduler.Web
{
    

    [DisallowConcurrentExecution]
    public class SchedulePublisher: IJob
    {
        private readonly IServiceProvider provider;
        public SchedulePublisher(IServiceProvider provider) => this.provider = provider;
        
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = provider.CreateScope();
            var publish = scope.ServiceProvider.GetRequiredService<IPublish>();
            await publish.Publish(context.JobDetail.Key.Group, context.MergedJobDataMap.GetString("message"));
        }

    }
}