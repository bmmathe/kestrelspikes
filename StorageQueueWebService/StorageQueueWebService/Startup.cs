using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace StorageQueueWebService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(config =>
                config.UseSqlServerStorage(@"[redacted]"));

            services.Configure<MyOptions>(Configuration);

            var connectionString = Configuration["AzureStorageAccountConnectionString"];
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();            
            CloudQueue queue = queueClient.GetQueueReference("testqueue");
            services.AddSingleton(queue);


            var serviceBusConnectionString = "[redacted]";
            var serviceBusClient = new TopicClient(serviceBusConnectionString, "testtopic");
            services.AddSingleton(serviceBusClient);
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseHangfireDashboard();
            app.UseHangfireServer();

            app.UseMvc();
        }
    }
}
