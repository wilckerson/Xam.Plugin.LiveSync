using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xam.Plugin.LiveSync.iOS
{
    public class LiveSync: XamlLiveSyncServerCore
    {
        private LiveSync()
        {
            var host = GetHost();
            Websockets.Ios.WebsocketConnection.Link();
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

        private string GetHost()
        {
            if(Xamarin.Forms.Application.Current == null)
            {
                throw new Exception("You MUST call Xam.Plugin.LiveSync.Droid.LiveSync.Init() after the line LoadApplication(new App());");
            }

            string assFullName = Xamarin.Forms.Application.Current.GetType().Assembly.GetName().FullName;
            var assemblyQualifiedName = $"Xam.Plugin.LiveSync.LiveSyncConfig, {assFullName}";
            var type = Type.GetType(assemblyQualifiedName);
            var fieldInfo = type.GetField("HOST");
            var value = fieldInfo.GetValue(null).ToString();

            return value;
        }
    }
}
