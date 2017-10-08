using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsLiveSync
{
    //public class ViewModel: INotifyPropertyChanged
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    private string prop;
    //    public string Prop { get
    //        {
    //            return prop;
    //        }
    //        set
    //        {
    //            prop = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop)));
    //        }
    //    }

    //    public ViewModel() {
    //        Prop = "Value from ViewModel";
    //    }

    //}
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            //var vm = new ViewModel();
            //BindingContext = vm;

            //var binding1 = new Binding("Prop", stringFormat: "Format: {0}");
            //lbl.SetBinding(Label.TextProperty, binding1);            

            //vm.Prop = "xxxxx";

            //var host = Xam.Plugin.LiveSync.LiveSyncConfig.GetServerHost();
            //DisplayAlert("LiveSync Host", host, "OK");
        }

        //private void Button_Clicked(object sender, EventArgs e)
        //{
        //    DisplayAlert("Title", "Message", "Cancel");
        //}
    }
}
