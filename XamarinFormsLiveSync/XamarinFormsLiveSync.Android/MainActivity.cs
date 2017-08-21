using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using XamarinFormsLiveSync.Core;

namespace XamarinFormsLiveSync.Droid
{
    [Activity(Label = "XamarinFormsLiveSync", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            //XamarinLivesync
            Websockets.Droid.WebsocketConnection.Link();
            XamarinFormsLiveSync.Core.XamlLiveSyncServer.Init("http://192.168.0.11:8161");

            SegmentedControl.FormsPlugin.Android.SegmentedControlRenderer.Init();

            LoadApplication(new App());
          
        }
    }
}

