using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsLiveSync.Core
{
    public class XamlLiveSyncServer
    {
        Application formsApp { get { return Application.Current; } }

        static XamlLiveSyncServer instance;
        public static void Init(string host)
        {
            if (instance == null)
            {
                instance = new XamlLiveSyncServer(host);
            }
        }

        private XamlLiveSyncServer(string host)
        {            
            var connection = Websockets.WebSocketFactory.Create();          
            connection.Open(host);
            connection.OnOpened += WebSocket_OnOpened;
            connection.OnMessage += WebSocket_OnMessage;
            connection.OnClosed += WebSocket_OnClose;
        }

        private void WebSocket_OnClose()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                formsApp.MainPage.DisplayAlert("", "Xamarin Forms Livesync Disconnected =/", "Ok");
            });
        }

        private void WebSocket_OnOpened()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                formsApp.MainPage.DisplayAlert("", "Xamarin Forms Livesync Connected ;)", "Ok");
            });
        }

        private void WebSocket_OnMessage(string data)
        {
            string separator = "_ENDNAME_";
            var nameIdx = data.IndexOf(separator);
            var fileName = data.Substring(0, nameIdx);
            var fileContent = data.Substring(nameIdx + separator.Length);

            UpdateViewContent(formsApp.MainPage, fileName, fileContent);
        }

        void UpdateViewContent<T>(T page, string fileName, string fileContent)
        {
            //Verifica se o arquivo mudado é referente a pagina atual
            var pageName = "";
            if (page is ContentPage)
            {
                pageName = page.GetType().Name + ".xaml";
                if (fileName == pageName)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                       XamlParser.XamlParser.ApplyXamlToPage((page as ContentPage), fileContent);  
                    });
                }
            }
            else if (page is ContentView)
            {
                pageName = page.GetType().Name + ".xaml";
                if (fileName == pageName)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        XamlParser.XamlParser.ApplyXamlToContentView((page as ContentView), fileContent);
                    });
                }
            }
            else if (page is NavigationPage)
            {
                var subPage = (page as NavigationPage).CurrentPage;
                UpdateViewContent(subPage, fileName, fileContent);
                return;
            }
            else if (page is MasterDetailPage)
            {
                    var subPageMaster = (page as MasterDetailPage).Master;
                    var subPageDetail = (page as MasterDetailPage).Detail;

                    UpdateViewContent(subPageMaster, fileName, fileContent);
                    UpdateViewContent(subPageDetail, fileName, fileContent);
                    return;
                
            }
            else if (page is TabbedPage)
            {
                var subPage = (page as TabbedPage).CurrentPage;
                UpdateViewContent(subPage, fileName, fileContent);
                return;
            }
        }

    }
}
