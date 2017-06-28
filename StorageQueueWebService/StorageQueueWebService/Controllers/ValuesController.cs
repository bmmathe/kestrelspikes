using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace StorageQueueWebService.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private MyOptions _configuration;
        private CloudQueue _queue;
        private TopicClient _topic;

        public ValuesController(IOptions<MyOptions> optionsAccessor, CloudQueue queue, TopicClient topic)
        {
            _configuration = optionsAccessor.Value;
            _queue = queue;
            _topic = topic;
        }
        // GET api/values
        [HttpGet]
        public string Get()
        {
            //var connectionString = _configuration.AzureStorageAccountConnectionString;
            //var storageAccount = CloudStorageAccount.Parse(connectionString);
            //CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            //// Retrieve a reference to a queue
            //CloudQueue queue = queueClient.GetQueueReference("testqueue");

            //// Peek at the next message
            //var peekedMessage = await queue.GetMessageAsync();
            //var value = peekedMessage.AsString;
            //await queue.DeleteMessageAsync(peekedMessage);
            //return peekedMessage.AsString;

            return "Hello, World";
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        [Route("newinstance")]
        public ActionResult Post([FromBody]TestMessage request)
        {            
            var connectionString = _configuration.AzureStorageAccountConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue messageQueue = queueClient.GetQueueReference("testqueue");
            CloudQueueMessage message = new CloudQueueMessage(request.Value);
            messageQueue.AddMessageAsync(message).Wait();
            return Ok();
        }

        [HttpPost]
        [Route("singleton")]
        public ActionResult PostSingleton([FromBody]TestMessage request)
        {                                
            CloudQueueMessage message = new CloudQueueMessage(request.Value);
            _queue.AddMessageAsync(message).Wait();
            return Ok();
        }

        [HttpPost]
        [Route("asb")]
        public ActionResult PostToServiceBus([FromBody] TestMessage request)
        {
            var body = JsonConvert.SerializeObject(request);
            var msg = new Message {Body = Encoding.UTF8.GetBytes(body), ContentType = "text/plain; charset=utf-8", MessageId = Guid.NewGuid().ToString("N")};
            _topic.SendAsync(msg).Wait();
            return Ok();
        }

        [HttpPost]
        [Route("hangfire")]
        public ActionResult PostToHangfire([FromBody] TestMessage request)
        {
            BackgroundJob.Enqueue(() => WaitForTenSeconds());

            return Ok();
        }

        public void WaitForTenSeconds()
        {
            
        }
        
    }

    public class TestMessage
    {
        public string Value { get; set; }
    }
}
