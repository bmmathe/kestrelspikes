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
    public class ServiceBusJob : IJob
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ServiceBusJob));
        public async Task Execute(IJobExecutionContext context)
        {
            Logger.Info("ServiceBus Job started.");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("[redacted]");
                var sw = Stopwatch.StartNew();
                try
                {
                    var response = await client.PostAsync("storagequeuewebservice/api/values/asb", new StringContent(JsonConvert.SerializeObject(new { Value = "Hello, World" }), Encoding.UTF8, "application/json"));
                    sw.Stop();
                    if(response.IsSuccessStatusCode)
                        SendMetrics(sw.ElapsedMilliseconds);
                    else
                        SendMetrics(sw.ElapsedMilliseconds, false);
                    Logger.Info($"Service Bus Post: {sw.ElapsedMilliseconds}ms");
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
                    c.Timing($"storagequeue.asb.success", ms);
                }
                else
                {
                    c.Timing($"storagequeue.asb.fail", ms);
                }
            }
        }
    }
}
