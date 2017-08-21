
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Linq;

namespace XamarinFormsLiveSync.Server
{
    public class MyWebSocketHandler
    {
        private Dictionary<Guid, WebSocket> _sockets = new Dictionary<Guid, WebSocket>();

        public IDictionary<Guid, WebSocket> Sockets { get { return _sockets; } }

        public MyWebSocketHandler()
        {
        }

        public MyWebSocketHandler(IDictionary<Guid, WebSocket>  sockets)
        {
            this._sockets = new Dictionary<Guid, WebSocket>(sockets);
        }

        public void OnConnected(WebSocket socket)
        {
            _sockets.Add(Guid.NewGuid(), socket);
        }

        public void OnDisconnected(WebSocket socket)
        {
            var item = _sockets.FirstOrDefault(f => f.Value == socket);
            _sockets.Remove(item.Key);

        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var msgBytes = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(buffer: new ArraySegment<byte>(array: msgBytes,
                                                                  offset: 0,
                                                                  count: msgBytes.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(Guid socketId, string message)
        {
            if (_sockets.TryGetValue(socketId, out WebSocket socket))
            {
                await SendMessageAsync(socket, message);
            }
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in _sockets)
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, message);
            }
        }

        public async void ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var message = Encoding.UTF8.GetString(buffer)?.Replace("\0", string.Empty);

            //Echo
            await SendMessageToAllAsync($"Echo: {message}");
        }
    }


}