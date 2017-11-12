using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using Xam.Plugin.LiveSync.iOS.NativeServices;
using Xam.Plugin.LiveSync.NativeServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(ToastService))]
namespace Xam.Plugin.LiveSync.iOS.NativeServices
{
  public class ToastService : IToastService
  {
   
    #region IToastService implementation

    public void ShowToast(string text, int duration = 5)
    {

      NSOperationQueue.MainQueue.AddOperation(async() =>
      {
        UIWindow keyWindow = UIApplication.SharedApplication.KeyWindow;
        UILabel toastView = new UILabel();
        toastView.Text = text;
        toastView.Font = UIKit.UIFont.PreferredBody;
        toastView.TextColor = UIColor.White;
        toastView.BackgroundColor = UIColor.DarkGray;
        toastView.TextAlignment = UITextAlignment.Center;
        toastView.AdjustsFontSizeToFitWidth = true;
        toastView.Frame = new CGRect(0.0, 0.0, keyWindow.Frame.Size.Width - 100, 50.0);
        toastView.Layer.CornerRadius = 10;
        toastView.Layer.MasksToBounds = true;
        toastView.Center = keyWindow.Center;
        toastView.LayoutMargins = new UIEdgeInsets(10, 10, 10, 10);

        keyWindow.AddSubview(toastView);
        await Task.Delay(duration * 1000);
        UIView.Animate(2, 0, UIViewAnimationOptions.CurveEaseOut,
          () => toastView.Alpha = 0.0f,
          () => toastView.RemoveFromSuperview());

      });

    }

    #endregion
  }
}
