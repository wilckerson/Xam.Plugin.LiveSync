using System;
using Android.Views;
using Android.Widget;
using Xam.Plugin.LiveSync.Droid.NativeServices;
using Xam.Plugin.LiveSync.NativeServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(ToastService))]
namespace Xam.Plugin.LiveSync.Droid.NativeServices
{
  public class ToastService : IToastService
  {
    private Toast toast;

    public void ShowToast(string text, int duration = 5)
    {
      toast = Toast.MakeText(Android.App.Application.Context, text, ToastLength.Long);
      toast.SetGravity(GravityFlags.Top, 0, 20);
      toast.Show();
    }

    public void CancelToast()
    {

      if (toast != null)
        toast.Cancel();
    }
  }
}
