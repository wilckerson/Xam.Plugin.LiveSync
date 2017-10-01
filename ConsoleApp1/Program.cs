using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace XamarinFormsLiveSync.Server
{
    class Program
    {
        public static string PATH_TO_WATCH;
        public static int PORT;

        static void Main(string[] args)
        {
            string currentDicrectory = Directory.GetCurrentDirectory();
            PATH_TO_WATCH = currentDicrectory;
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--path")
                {
                    var cmdArg = args.ElementAtOrDefault(i + 1);
                    if (!string.IsNullOrEmpty(cmdArg) && !cmdArg.StartsWith("--"))
                    {
                        PATH_TO_WATCH = cmdArg;
                        i++;

                        if (!Directory.Exists(PATH_TO_WATCH))
                        {
                            Console.WriteLine($"Error: The directory {PATH_TO_WATCH} does not exist.");
                            Console.ReadKey();
                            return;
                        }
                    }
                }
                else if (args[i] == "--port")
                {
                    var cmdArg = args.ElementAtOrDefault(i + 1);
                    if (!string.IsNullOrEmpty(cmdArg) && !cmdArg.StartsWith("--"))
                    {
                        int.TryParse(cmdArg, out int port);
                        LivesyncServer.PORT = port;
                        i++;
                    }
                }
            }

            //Task.Run(() =>
            //{
                var host = new WebHostBuilder()
                .UseUrls($"http://*:{LivesyncServer.PORT}")
                .UseKestrel()
                .UseContentRoot(currentDicrectory)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

                host.Run();
            //});

            //Console.WriteLine("After start server =)");
            //Console.ReadKey();
        }

    }
}
