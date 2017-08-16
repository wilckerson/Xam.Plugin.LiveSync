using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Websockets;
using Xamarin.Forms;

namespace XamarinFormsLiveSync
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new XamarinFormsLiveSync.MainPage();

            ListenToLiveSyncServer();
        }

        private void ListenToLiveSyncServer()
        {
            var connection = Websockets.WebSocketFactory.Create();
            connection.Open("http://192.168.0.11:5000");
            connection.OnOpened += WebSocket_OnOpened;
            connection.OnMessage += WebSocket_OnMessage;
            connection.OnClosed += WebSocket_OnClose;
        }
        
        private void WebSocket_OnClose()
        {
            MainPage.DisplayAlert("","Xamarin Forms Livesync Disconnected =/","Ok");
        }

        private void WebSocket_OnOpened()
        {
            MainPage.DisplayAlert("","Xamarin Forms Livesync Connected ;)","Ok");

        }

        private void WebSocket_OnMessage(string data)
        {
            string separator = "_ENDNAME_";
            var nameIdx = data.IndexOf(separator);
            var fileName = data.Substring(0, nameIdx);
            var fileContent = data.Substring(nameIdx + separator.Length);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
