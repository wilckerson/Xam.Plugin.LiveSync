using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

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
                    PATH_TO_WATCH = GetArgsValue<string>(args, "--project-path", Directory.GetCurrentDirectory());
                    var configFilePath = GetArgsValue<string>(args, "--config-path", Directory.GetCurrentDirectory());

                    //debugLogFile.WriteLine($"{DateTime.Now}: --project-path {PATH_TO_WATCH}");
                    //debugLogFile.WriteLine($"{DateTime.Now}: --config-path {configFilePath}");

                    var hostText = FileHelper.GetFileContent(configFilePath);
                    HOST = hostText;
                    
                    var hostPort = hostText.Split(":").LastOrDefault();
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
              
                using (StreamWriter writetext = new StreamWriter($"{directory}/ServerException_{DateTime.Now}.log"))
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
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

            var ipAddress = hostEntry.AddressList
                .LastOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToString();

            return ipAddress;
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
