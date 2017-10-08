using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xam.Plugin.LiveSync.Droid
{
    public class LiveSync: XamlLiveSyncServerCore
    {
        private LiveSync()
        {
            var host = Xam.Plugin.LiveSync.LiveSyncConfig.GetServerHost();
            Websockets.Droid.WebsocketConnection.Link();
            base.InitWebsocket(host);
        }

        static LiveSync instance;
        public static void Init()
        {
            if (instance == null)
            {
                instance = new LiveSync();
            }
        }

        public static string GetHost()
        {
            var host = Xam.Plugin.LiveSync.LiveSyncConfig.GetServerHost();
            return host;
        }
    }
}
