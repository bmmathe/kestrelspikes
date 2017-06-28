using System;
using System.Threading.Tasks;
using Quartz.Impl;
using Quartz.Logging;

namespace StorageQueueTest
{
    class Program
    {
        private static ILog _logger = LogProvider.GetLogger(typeof(ConsoleLogProvider));
        static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
            RunJobsAsync().Wait();
        }

        private static async Task RunJobsAsync()
        {
            try
            {
                var factory = new StdSchedulerFactory();
                var scheduler = await factory.GetScheduler();
                await scheduler.Start();

                await scheduler.ScheduleBasicServiceCall();
                await scheduler.ScheduleBasicStaticServiceCall();
                await scheduler.ScheduleServiceBus();
                await scheduler.ScheduleHangfire();
                await scheduler.PingServiceCall();

                var exitString = "";
                while (exitString != "q")
                {
                    exitString = Console.ReadLine();
                }

                await scheduler.Shutdown();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                Console.Read();
            }

        }
    }
}