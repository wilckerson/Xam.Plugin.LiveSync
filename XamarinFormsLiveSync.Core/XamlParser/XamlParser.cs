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
        public static View ParseXamlToView(string xaml) {

            XamlAstParser astParser = new XamlAstParser();
            var rootNode = astParser.ExtractAst(xaml);

            XamlAstBuilder astBuilder = new XamlAstBuilder();
            var viewRoot = astBuilder.Build(rootNode);

            return viewRoot;
        }
    }
}
