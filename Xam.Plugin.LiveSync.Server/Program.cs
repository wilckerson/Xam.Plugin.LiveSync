using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Xam.Plugin.LiveSync.Server
{
    class Program
    {
        public static string PATH_TO_WATCH;
        public static string IP_ADDRESS;
        public static int PORT;
        public static string HOST { get { return $"http://{IP_ADDRESS}:{PORT}"; } }

        static void Main(string[] args)
        {
            PATH_TO_WATCH = GetArgsValue<string>(args, "--path", Directory.GetCurrentDirectory());
            PORT = GetArgsValue<int>(args, "--port", 9759);
            IP_ADDRESS = GetIPAddress();

            Console.WriteLine($"Xam.Plugin.LiveSync.Server connected at: {HOST}");

            var host = new WebHostBuilder()
                .UseUrls($"http://*:{PORT}")
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
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
