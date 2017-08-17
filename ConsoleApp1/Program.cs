using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace XamarinFormsLiveSync.Server
{
    class Program
    {
        public static string PATH_TO_WATCH;

        static void Main(string[] args)
        {
            string currentDicrectory = Directory.GetCurrentDirectory();
            PATH_TO_WATCH = currentDicrectory;

            if (args.Length > 1 && args[0] == "--path")
            {
                var path = args[1];
                PATH_TO_WATCH = path;

                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Error: The directory {path} does not exist.");
                    Console.ReadKey();
                    return;
                }
            }

            var host = new WebHostBuilder()
                .UseUrls($"http://*:{LivesyncServer.PORT}")
                .UseKestrel()
                .UseContentRoot(currentDicrectory)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
