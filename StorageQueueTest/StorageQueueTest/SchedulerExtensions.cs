using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace StorageQueueTest
{
    public static class SchedulerExtensions
    {
        public static async Task ScheduleBasicServiceCall(this IScheduler scheduler)
        {
            var job = JobBuilder.Create<BasicServiceCall>()
                .WithIdentity("BasicServiceCall", "Group1")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "Group1")
                .StartAt(DateTimeOffset.Now.Add(TimeSpan.FromSeconds(4)))
                .WithSimpleSchedule(x=>x.WithIntervalInSeconds(30).RepeatForever())                
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public static async Task ScheduleBasicStaticServiceCall(this IScheduler scheduler)
        {
            var job = JobBuilder.Create<BasicServiceCallStatic>()
                .WithIdentity("BasicServiceCallStatic", "Group1")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger2", "Group1")
                .StartAt(DateTimeOffset.Now.Add(TimeSpan.FromSeconds(7)))
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())                
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public static async Task ScheduleServiceBus(this IScheduler scheduler)
        {
            var job = JobBuilder.Create<ServiceBusJob>()
                .WithIdentity("ScheduleServiceBus", "Group1")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger3", "Group1")
                .StartAt(DateTimeOffset.Now.Add(TimeSpan.FromSeconds(13)))
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public static async Task ScheduleHangfire(this IScheduler scheduler)
        {
            var job = JobBuilder.Create<HangfireJob>()
                .WithIdentity("ScheduleHangfire", "Group1")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger4", "Group1")
                .StartAt(DateTimeOffset.Now.Add(TimeSpan.FromSeconds(23)))
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public static async Task PingServiceCall(this IScheduler scheduler)
        {
            var job = JobBuilder.Create<PingJob>()
                .WithIdentity("PingServiceCall", "Group1")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger5", "Group1")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
