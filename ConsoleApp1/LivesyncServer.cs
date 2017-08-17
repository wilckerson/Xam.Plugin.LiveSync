using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XamarinFormsLiveSync.Server
{
    public class LivesyncServer
    {
        public static int PORT = 5000;
        FileSystemWatcher watcher;
        string watcherPath;
        MyWebSocketHandler webSocketHandler;
        public string DisplayMessage { get; private set; }

        public LivesyncServer(string path)
        {
            this.watcherPath = path;
            DisplayHost();
            ConfigureWatcher();

            webSocketHandler = new MyWebSocketHandler();
        }

        private async void DisplayHost()
        {
            IPHostEntry hostEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());

            var ipAddress = hostEntry.AddressList
                .LastOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToString();
            //var port = Program.PORT; //"5000";// http.Connection.LocalPort;

            string host = $"http://{ipAddress}:{PORT}";
            DisplayMessage = $"Xamarin Forms LivesyncServer connected at: {host}";
            Console.WriteLine(DisplayMessage);
        }

        private void ConfigureWatcher()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = watcherPath;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
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
                await Task.Delay(700); //Necessário para evitar a exception "Esse arquivo está sendo usado por outro processo."

                var textContent = FileHelper.GetFileContent(path);

                if (string.IsNullOrEmpty(textContent)) { return; }

                var lastIdx = path.LastIndexOf('\\');
                string name = path.Substring(lastIdx + 1);

                string data = $"{name}_ENDNAME_{textContent}";
                await webSocketHandler.SendMessageToAllAsync(data);
            }
        }

        public async Task RequestHandler(HttpContext http, Func<Task> next)
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
