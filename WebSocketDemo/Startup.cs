using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSocketDemo.Controllers;
using WebSocketDemo.Models;
using WebSocketDemo.Push;
using WebSocketDemo.Services;

namespace WebSocketDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var hashQueue = new ConcurrentQueue<HashRequest>();

            services.AddMvc();
            services.AddSingleton<JobStore>();
            services.AddSingleton<QueueHashJob>(provider => hashQueue.Enqueue);
            services.AddSingleton<IHostedService, HashService>(provider => new HashService(
                provider.GetRequiredService<JobStore>(),
                hashQueue,
                provider.GetRequiredService<ILoggerFactory>()));
            services.AddSingleton<IMessageSource, JobToPushMessageAdapter>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseMessagePushHandler("/websocket");
        }
    }
}
