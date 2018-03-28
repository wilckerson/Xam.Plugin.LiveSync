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
using System.Reflection;

namespace Xam.Plugin.LiveSync.Server
{
    public class LivesyncServer
    {
        FileSystemWatcher watcher;
        string watcherPath;
        MyWebSocketHandler webSocketHandler;
        DateTime nextProcess;

        public string DisplayMessage { get; private set; }

        public LivesyncServer(string path, string host)
        {
            this.watcherPath = path;
            ConfigureWatcher();

            webSocketHandler = new MyWebSocketHandler();
            nextProcess = DateTime.Now;

        }

        private void ConfigureWatcher()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = watcherPath;
            watcher.IncludeSubdirectories = true;
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
                else if (path.EndsWith(".xaml.g.cs")) //Handling VS2017 edit file behavior
                {
                    var compiledFileContent = FileHelper.GetFileContent(path);
                    var startStr = "XamlFilePathAttribute";
                    var startIdx = compiledFileContent.IndexOf(startStr);
                    var endIdx = compiledFileContent.IndexOf(")]", startIdx);
                    var rawContent = compiledFileContent.Substring(startIdx+ startStr.Length, endIdx - (startIdx+startStr.Length));
                    var pathContent = rawContent
                        .Replace("(", "")
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Replace("+", "")
                        .Replace("\\\\","\\")
                        .Replace("\"","")
                        .Replace(" ","");
                    path = pathContent;
                }
                else if (path.Contains(".xaml") && path.Contains(".#"))
                {
                    var newPath = path.Replace(".#", "");
                    path = newPath;
                }
                else if (!path.EndsWith(".xaml")) { return; }

                //------------------------------------------------------------
                //O evento OnChange está sendo chamado duas vezes sempre que 
                //altera o arquivo. A logica abaixo, desconsidera a segunda 
                //chamada caso ocorra em até 3 segundos
                //
                var dtNow = DateTime.Now;
                if (dtNow < nextProcess)
                {
                    return;
                }
                nextProcess = dtNow.AddSeconds(3);
                //---------------------------------------------


                await Task.Delay(700); //Necessário para evitar a exception "Esse arquivo está sendo usado por outro processo."

                var textContent = FileHelper.GetFileContent(path);

                if (string.IsNullOrEmpty(textContent)) { return; }

                var lastIdx = path.LastIndexOf('\\');
                if (lastIdx == -1)
                {
                    lastIdx = path.LastIndexOf('/');
                }
                string name = path.Substring(lastIdx + 1);

                string lsgMsg = $"{DateTime.Now}: Changes at file {name}. Sending to app...";
                Console.WriteLine(lsgMsg);
                DebugLog(lsgMsg);

                string data = $"{name}_ENDNAME_{textContent}";
                try
                {
                    await webSocketHandler.SendMessageToAllAsync(data);
                }
                catch (Exception)
                //catch (System.ObjectDisposedException ex)
                {
                    //Se der algum erro, reinicia o Socket mantendo as conexoes abertas
                    var sockets = webSocketHandler.Sockets;
                    webSocketHandler = new MyWebSocketHandler(sockets);
                    nextProcess = DateTime.Now;

                    OnChanged(sender, e);
                }

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

        void DebugLog(string content)
        {
            var location = Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);
            using (StreamWriter writetext = new StreamWriter($"{directory}/Debug.log", true))
            {
                writetext.WriteLine(content);
            }
        }

    }
}
