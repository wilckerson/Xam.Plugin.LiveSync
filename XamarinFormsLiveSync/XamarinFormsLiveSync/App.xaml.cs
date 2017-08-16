using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Websockets;
using Xamarin.Forms;
using XamarinFormsLiveSync.Core.XamlParser;

namespace XamarinFormsLiveSync
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var page = new MainPage();
            MainPage = page;
            //MainPage = new NavigationPage(new MainPage());
            
            var newContent = XamlParser.ParseXamlToView(SampleXaml2);
            page.Content = newContent;

            //ListenToLiveSyncServer();
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
            Device.BeginInvokeOnMainThread(() =>
            {
                MainPage.DisplayAlert("", "Xamarin Forms Livesync Disconnected =/", "Ok");
            });
        }

        private void WebSocket_OnOpened()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                MainPage.DisplayAlert("", "Xamarin Forms Livesync Connected ;)", "Ok");
            });
        }

        private void WebSocket_OnMessage(string data)
        {
            string separator = "_ENDNAME_";
            var nameIdx = data.IndexOf(separator);
            var fileName = data.Substring(0, nameIdx);
            var fileContent = data.Substring(nameIdx + separator.Length);

            UpdateViewContent(MainPage, fileName, fileContent);

        }

        void UpdateViewContent<T>(T page, string fileName,string fileContent)
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
                        var newContent = ParseXamlToView(fileContent);
                        (page as ContentPage).Content = newContent;
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
                        var newContent = ParseXamlToView(fileContent);
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
            else if (MainPage is MasterDetailPage)
            {
                var masterName = (MainPage as MasterDetailPage).Master.GetType().Name + ".xaml";
                var detailsName = (MainPage as MasterDetailPage).Detail.GetType().Name + ".xaml";

                if (fileName == masterName)
                {
                    var subPage = (page as MasterDetailPage).Master;
                    UpdateViewContent(subPage, fileName, fileContent);
                    return;
                }
                else if(fileName == detailsName)
                {
                    var subPage = (page as MasterDetailPage).Detail;
                    UpdateViewContent(subPage, fileName, fileContent);
                    return;
                }
            }
            else if (MainPage is TabbedPage)
            {
                pageName = (MainPage as TabbedPage).CurrentPage.GetType().Name + ".xaml";
                if (fileName == pageName)
                {
                    var subPage = (page as TabbedPage).CurrentPage;
                    UpdateViewContent(subPage, fileName, fileContent);
                    return;
                }
            }            
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

        View ParseXamlToView(string xaml)
        {
            return new Label() { Text = DateTime.Now.ToString() };
        }

        static string SampleXaml2 = @"
<ContentPage>
 <Label Text=""{Binding NenhumItemMsg}"" IsVisible=""true"" HorizontalOptions=""Center"" Margin=""16""/>

</ContentPage>
";
    }
}
