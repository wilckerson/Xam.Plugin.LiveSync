using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xam.Plugin.LiveSync.XamlParser
{
    public class XamlParser
    {
        public static void ApplyXamlToPage(ContentPage rootPage, string xaml)
        {

            try
            {
                var viewObj = ParseXamlToViewObject(rootPage, xaml);
                if (viewObj is ContentPage)
                {
                    var newPage = (viewObj as ContentPage);
                    rootPage.Content = newPage.Content;

                    if (!string.IsNullOrEmpty(newPage.Title))
                    {
                        rootPage.Title = newPage.Title;
                    }

                    if (newPage.ToolbarItems != null && newPage.ToolbarItems.Any())
                    {
                        rootPage.ToolbarItems.Clear();
                        foreach (var item in newPage.ToolbarItems)
                        {
                            rootPage.ToolbarItems.Add(item);
                        }
                    }

                    //TODO: Atualizar outras propriedades normalmente utilizadas
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Livesync: Error applying Xaml to Page", ex.Message, "Ok");
            }
        }

        public static void ApplyXamlToContentView(ContentView rootContentView, string xaml)
        {
            try
            {
                var viewObj = ParseXamlToViewObject(rootContentView, xaml);
                if (viewObj is ContentView)
                {
                    var newContentView = (viewObj as ContentView);
                    rootContentView.Content = newContentView.Content;
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Livesync: Error applying Xaml to ContentView", ex.Message, "Ok");

            }
        }

        public static object ParseXamlToViewObject(object rootPage, string xaml)
        {

            XamlAstParser astParser = new XamlAstParser();
            var rootNode = astParser.ExtractAst(xaml);

            XamlAstBuilder astBuilder = new XamlAstBuilder(rootPage);
            var viewObj = astBuilder.BuildNode(rootNode);

            return viewObj;

        }


    }
}
