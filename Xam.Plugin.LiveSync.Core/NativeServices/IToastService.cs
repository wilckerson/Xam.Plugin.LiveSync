using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xam.Plugin.LiveSync.NativeServices
{
  public interface IToastService
  {
    void ShowToast(string text, int duration = 5);
  }
}
