using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsLiveSync.Core.XamlParser
{
    public class XamlParser
    {
        public static View ParseXamlAndGetContentView(string xaml)
        {
            var viewObj = ParseXamlToViewObject(xaml);
            if (viewObj is ContentPage)
            {
                var content = (viewObj as ContentPage).Content;
                return content;
            }
            else if(viewObj is ContentView)
            {
                var content = (viewObj as ContentView).Content;
                return content;
            }
            else
            {
                return null;
            }
        }

        public static object ParseXamlToViewObject(string xaml) {

            XamlAstParser astParser = new XamlAstParser();
            var rootNode = astParser.ExtractAst(xaml);

            XamlAstBuilder astBuilder = new XamlAstBuilder();
            var viewObj = astBuilder.BuildNode(rootNode);

            return viewObj;
        }
    }
}
