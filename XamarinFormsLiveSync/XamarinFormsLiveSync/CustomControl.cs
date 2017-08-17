using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsLiveSync
{
    public class CustomControl: ContentView
    {
        public CustomControl()
        {
            var stk = new StackLayout();
            stk.BackgroundColor = Color.Gray;
            stk.Children.Add(new Label() { Text = "CustomControl_Lbl1 " });
            stk.Children.Add(new Label() { Text = "CustomControl_Lbl2 " });
            this.Content = stk;
        }
    }
}
