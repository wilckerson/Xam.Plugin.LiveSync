using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Xam.Plugin.LiveSync.Server
{
  class Program
  {
    public static string PATH_TO_WATCH;
    public static int PORT;
    public static string HOST;

    static void Main(string[] args)
    {
      var location = Assembly.GetEntryAssembly().Location;
      var directory = Path.GetDirectoryName(location);

      try
      {
        //using (StreamWriter debugLogFile = new StreamWriter($"{directory}/Server_Debug.log"))
        {
          PATH_TO_WATCH = GetArgsValue<string>(args, "--project-path", "").Trim();
          var portArg = GetArgsValue<string>(args, "--port", "9759").Trim();
          var configFilePath = GetArgsValue<string>(args, "--config-path", "").Trim();

          if (string.IsNullOrEmpty(PATH_TO_WATCH))
          {
            Console.WriteLine("Xam.Plugin.LiveSync.Server:");
            Console.WriteLine("The argument --project-path is required. Set it to the root location where your XAML files are.");
            Console.ReadKey();
            return;
          }

          //debugLogFile.WriteLine($"{DateTime.Now}: --project-path {PATH_TO_WATCH}");
          //debugLogFile.WriteLine($"{DateTime.Now}: --config-path {configFilePath}");

          if (string.IsNullOrEmpty(configFilePath))
          {
            HOST = $"{GetIPAddress()}:{portArg}";
          }
          else
          {
            var hostText = FileHelper.GetFileContent(configFilePath);
            HOST = hostText;
          }

          var hostPort = HOST.Split(":").LastOrDefault();
          int.TryParse(hostPort, out PORT);

          //debugLogFile.WriteLine($"{DateTime.Now}: host {HOST}");

          Console.WriteLine($"Xam.Plugin.LiveSync.Server connected at: {HOST} watching the directory: {PATH_TO_WATCH}");

          var host = new WebHostBuilder()
              .UseUrls($"http://*:{PORT}")
              .UseKestrel()
              .UseStartup<Startup>()
              .Build();

          host.Run();
        }
      }
      catch (Exception ex)
      {

        using (StreamWriter writetext = new StreamWriter($"{directory}/ServerException_{DateTime.Now.Ticks}.log"))
        {
          writetext.WriteLine(ex.Message);
          writetext.WriteLine(ex.StackTrace);

          if (ex.InnerException != null)
          {
            writetext.WriteLine(ex.InnerException.Message);
            writetext.WriteLine(ex.InnerException.StackTrace);
          }
        }
      }
    }

    static string GetIPAddress()
    {
      bool isOSx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
      if (isOSx)
      {
        var proc = new Process();
        proc.StartInfo.FileName = "ipconfig";
        proc.StartInfo.Arguments = "getifaddr en0";
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;
        proc.Start();
        string result = proc.StandardOutput.ReadToEnd();
        result = result?.Trim();
        proc.WaitForExit();

        return result;
      }
      else
      {
        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

        var ipAddress = hostEntry.AddressList
            .LastOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
            .ToString();

        return ipAddress;
      }
    }

    static T GetArgsValue<T>(string[] args, string argsName, T defaultValue)
    {
      T value = defaultValue;

      for (int i = 0; i < args.Length; i++)
      {
        if (args[i] == argsName)
        {
          var cmdArg = args.ElementAtOrDefault(i + 1);
          if (!string.IsNullOrEmpty(cmdArg) && !cmdArg.StartsWith("--"))
          {
            try
            {
              value = (T)Convert.ChangeType(cmdArg, typeof(T));
            }
            catch (Exception)
            {
            }

            break;
          }
        }
      }

      return value;
    }
  }
}
