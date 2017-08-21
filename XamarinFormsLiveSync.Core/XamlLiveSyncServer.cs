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
                        try
                        {
                            var newContent = XamlParser.XamlParser.ParseXamlAndGetContentView(page, fileContent);
                            (page as ContentPage).Content = newContent;
                        }
                        catch (Exception ex)
                        {

                        }

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
                        var newContent = XamlParser.XamlParser.ParseXamlAndGetContentView(page, fileContent);
                        (page as ContentView).Content = newContent;
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
                var masterName = (page as MasterDetailPage).Master.GetType().Name + ".xaml";
                var detailsName = (page as MasterDetailPage).Detail.GetType().Name + ".xaml";

                if (fileName == masterName)
                {
                    var subPage = (page as MasterDetailPage).Master;
                    UpdateViewContent(subPage, fileName, fileContent);
                    return;
                }
                else if (fileName == detailsName)
                {
                    var subPage = (page as MasterDetailPage).Detail;
                    UpdateViewContent(subPage, fileName, fileContent);
                    return;
                }
            }
            else if (page is TabbedPage)
            {
                pageName = (page as TabbedPage).CurrentPage.GetType().Name + ".xaml";
                if (fileName == pageName)
                {
                    var subPage = (page as TabbedPage).CurrentPage;
                    UpdateViewContent(subPage, fileName, fileContent);
                    return;
                }
            }
        }

    }
}
