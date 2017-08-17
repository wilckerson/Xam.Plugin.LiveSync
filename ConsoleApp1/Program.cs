using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace XamarinFormsLiveSync.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            string currentDicrectory = Directory.GetCurrentDirectory();

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
