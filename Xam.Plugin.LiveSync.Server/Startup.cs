using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Xam.Plugin.LiveSync.Server
{
    public class Startup
    {
        MyWebSocketHandler webSocketHandler;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            webSocketHandler = new MyWebSocketHandler();

            app.UseWebSockets();
            app.Use(async (http, next) =>
            {
                if (http.WebSockets.IsWebSocketRequest)
                {                    
                    await WebSocketRequestHandler(http, next);
                    return;
                }

                await http.Response.WriteAsync($"Xam.Plugin.LiveSync Server {DateTime.Now}");
                return;
                //await next();
            });
        }

        public async Task WebSocketRequestHandler(HttpContext http, Func<Task> next)
        {
            var socket = await http.WebSockets.AcceptWebSocketAsync();
            webSocketHandler.OnConnected(socket);

            await Receive(socket, (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    webSocketHandler.ReceiveAsync(socket, result, buffer);
                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    webSocketHandler.OnDisconnected(socket);
                    return;
                }

            });
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                       cancellationToken: CancellationToken.None).ConfigureAwait(false);

                handleMessage(result, buffer);
            }
        }
    }
}
