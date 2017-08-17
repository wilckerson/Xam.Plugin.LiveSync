
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace XamarinFormsLiveSync.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            string path = "C://Projetos//XamarinFormsLiveSync//XamarinFormsLiveSync//XamarinFormsLiveSync";
            var livesyncServer = new LivesyncServer(path);
            
            app.UseWebSockets();
            app.Use(async (http, next) =>
            {
                if (http.WebSockets.IsWebSocketRequest)
                {
                    await livesyncServer.RequestHandler(http, next);
                    return;                   
                }
                
                await http.Response.WriteAsync(livesyncServer.DisplayMessage);
                await next();
            });
        }


        
    }


}