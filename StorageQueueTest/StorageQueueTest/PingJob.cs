using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quartz;
using Quartz.Logging;

namespace StorageQueueTest
{
    public class PingJob : IJob
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(PingJob));
        public async Task Execute(IJobExecutionContext context)
        {
            Logger.Info("Ping Job started.");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("[redacted]");
                var sw = Stopwatch.StartNew();
                try
                {
                    var response = await client.GetAsync("storagequeuewebservice/api/values");
                    sw.Stop();
                    if (response.IsSuccessStatusCode)
                        SendMetrics(sw.ElapsedMilliseconds);
                    else
                        SendMetrics(sw.ElapsedMilliseconds, false);
                    Logger.Info($"Ping: {sw.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    SendMetrics(sw.ElapsedMilliseconds, false);
                    Logger.Error(ex.Message);
                }
            }
        }

        private void SendMetrics(long ms, bool isSuccess = true)
        {
            using (var c = new StatsDClient())
            {
                if (isSuccess)
                {
                    c.Timing($"storagequeue.ping.success", ms);
                }
                else
                {
                    c.Timing($"storagequeue.ping.fail", ms);
                }
            }
        }
    }
}
