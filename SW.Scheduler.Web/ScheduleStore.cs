using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using SW.PrimitiveTypes;
using SW.Scheduler.Model;

namespace SW.Scheduler.Web
{
    public class ScheduleStore : IConsume<ScheduleMessage>
    {
        readonly ISchedulerFactory factory;
        public ScheduleStore(ISchedulerFactory factory)
        {
            this.factory = factory;
        }

        public Task Process(ScheduleMessage message) =>
            message.Delete ? Delete(message) : Update(message);

        private static IJobDetail BuildJob(ScheduleMessage request) =>
            JobBuilder.Create<SchedulePublisher>()
                .WithIdentity(request.Id, request.MessageTypeName)
                .UsingJobData("message",request.MessageSerialized)
                .UsingJobData("cron", request.Schedule)
                .StoreDurably()
                .Build();
        
        private async Task Add(ScheduleMessage request)
        {
            var triggerKey = Guid.NewGuid().ToString();

            var job = BuildJob(request);
            job.JobDataMap.Put("trigger-key" , triggerKey);
            
            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey, request.MessageTypeName)
                .WithCronSchedule(request.Schedule, x => x
                    .WithMisfireHandlingInstructionFireAndProceed())
                .ForJob(request.Id, request.MessageTypeName)
                .Build();

            var scheduler = await factory.GetScheduler();
            await scheduler.ScheduleJob(job, trigger);

        }
        
        
        private async Task Update(ScheduleMessage request)
        {
            var scheduler = await factory.GetScheduler();
            
            var oldJob = await scheduler.GetJobDetail(new JobKey(request.Id, request.MessageTypeName));
            if (oldJob == null)
            {
                await Add(request);
                return;
            }
            
            var oldKey = new TriggerKey(oldJob.JobDataMap.GetString("trigger-key"), request.MessageTypeName);
            
            
            var job = BuildJob(request);
            
            if (oldJob.JobDataMap["cron"].ToString() != request.Schedule)
            {
                var triggerKey = Guid.NewGuid().ToString();
                job.JobDataMap.Put("trigger-key" , triggerKey);
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey, request.MessageTypeName)
                    .WithCronSchedule(request.Schedule, x => x
                        .WithMisfireHandlingInstructionFireAndProceed())
                    .ForJob(request.Id, request.MessageTypeName)
                    .Build();
                
                await scheduler.RescheduleJob(oldKey, trigger);
            }
            else
            {
                job.JobDataMap.Put("trigger-key" , oldKey.Name);
            }
            
            await scheduler.AddJob(job, true);
        }

        private async Task Delete(ScheduleMessage message)
        {
            var scheduler = await factory.GetScheduler();
            
            var oldJob = await scheduler.GetJobDetail(new JobKey(message.Id, message.MessageTypeName));
            if (oldJob == null) 
                return;
            var key = new TriggerKey(oldJob.JobDataMap.GetString("trigger-key"), message.MessageTypeName);
            
            await scheduler.UnscheduleJob(key);
        }
    }
}