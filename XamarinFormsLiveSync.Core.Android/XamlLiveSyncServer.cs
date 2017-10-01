using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamarinFormsLiveSync.Core.Android
{
    public class XamlLiveSyncServer: XamlLiveSyncServerCore
    {
        private XamlLiveSyncServer(string host)
        {
            Websockets.Droid.WebsocketConnection.Link();
            base.InitWebsocket(host);
        }

        static XamlLiveSyncServer instance;
        public static void Init(string host)
        {
            if (instance == null)
            {
                instance = new XamlLiveSyncServer(host);
            }
        }
    }
}
