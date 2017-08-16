
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

namespace XamarinFormsLiveSync.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        MyWebSocketHandler _webSocketHandler;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = "C://Projetos//XamarinFormsLiveSync//XamarinFormsLiveSync//XamarinFormsLiveSync";
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
            
            _webSocketHandler = new MyWebSocketHandler();
            app.UseWebSockets();
            app.Use(async (http, next) =>
            {
                if (!http.WebSockets.IsWebSocketRequest)
                {
                        //await next();
                        await http.Response.WriteAsync("Hello World!");
                    return;
                }
                
                    var socket = await http.WebSockets.AcceptWebSocketAsync();
                _webSocketHandler.OnConnected(socket);

                await Receive(socket, (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        _webSocketHandler.ReceiveAsync(socket, result, buffer);
                        return;
                    }

                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _webSocketHandler.OnDisconnected(socket);
                        return;
                    }

                });



            });
        }

        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                var path = e.FullPath;
                
                if (path.EndsWith("TMP"))
                {
                    var idx = path.LastIndexOf('~');
                    var newPath = path.Substring(0, idx);
                    path = newPath;
                }

                if (!path.EndsWith(".xaml")) { return; }

                var textContent = GetFileContent(path);

                if (string.IsNullOrEmpty(textContent)) { return; }

                string data = $"{e.Name}\r{textContent}";
                await _webSocketHandler.SendMessageToAllAsync(data);
            }
        }


        string GetFileContent(string path)
        {
            try
            {



                using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var encoding = GetEncoding(stream);

                    /* 
                                    MemoryStream convertedStream = new MemoryStream();

                                    if (encoding != Encoding.UTF8)
                                    {
                                        MemoryStream mStream = new MemoryStream();
                                        stream.CopyTo(mStream);
                                        var convertedBytes = Encoding.Convert(encoding, Encoding.UTF8, mStream.ToArray());
                                        convertedStream.Write(convertedBytes, 0, convertedBytes.Length);
                                        convertedStream.Seek(0, SeekOrigin.Begin);

                                    }
                                    else
                                    {
                                        stream.CopyTo(convertedStream);
                                        convertedStream.Seek(0, SeekOrigin.Begin);
                                    }
                                    */

                    using (StreamReader streamReader = new StreamReader(stream, encoding))
                    {
                        string textContent = streamReader.ReadToEnd();
                        return textContent;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return "";
            }
        }

        public static Encoding GetEncoding(Stream file)
        {
            // Read the BOM
            var bom = new byte[4];
            //using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            //{
            file.Read(bom, 0, 4);
            file.Seek(0, SeekOrigin.Begin);
            //}

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }
        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                       cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }


}