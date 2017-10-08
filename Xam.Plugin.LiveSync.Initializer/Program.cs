using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xam.Plugin.LiveSync.Initializer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var serverPath = GetArgsValue<string>(args, "--server-path", "C:\\Projetos\\XamarinFormsLiveSync\\Xam.Plugin.LiveSync.Server\\bin\\Debug\\netcoreapp2.0\\Xam.Plugin.LiveSync.Server.dll");
                var projectPath = GetArgsValue<string>(args, "--project-path", "C:\\Projetos\\XamarinFormsLiveSync\\XamarinFormsLiveSync\\XamarinFormsLiveSync");
                var configPath = GetArgsValue<string>(args, "--config-path", "C:\\Projetos\\XamarinFormsLiveSync\\Xam.Plugin.LiveSync.Initializer\\bin\\Debug\\netcoreapp2.0\\LiveSync.host");
                //var serverPath = GetArgsValue<string>(args, "--server-path", @"C:\Windows\System32\notepad.exe");

                var port = GetArgsValue<int>(args, "--port", 9759);
                var ip_address = GetIPAddress();
                var host = $"http://{ip_address}:{port}";

                Console.WriteLine($"Xam.Plugin.LiveSync.Initializer will run server at: {host}");

                LiveSyncConfigGenerator.GeneratePartialClass(host);
                LiveSyncConfigGenerator.GenerateHostFile(host);

                //Task.Run(() =>
                //{
                KillServerIfExistAndStartNew("dotnet",
                    serverPath,
                    $"--project-path {projectPath}",
                    $"--config-path {configPath}"
                    );
                //});
                //Task.Delay(3000).Wait();
            }
            catch (Exception ex)
            {
                var location = Assembly.GetEntryAssembly().Location;
                var directory = Path.GetDirectoryName(location);

                using (StreamWriter writetext = new StreamWriter($"{directory}/Exception_{DateTime.Now}.log"))
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

        static void KillServerIfExistAndStartNew(string execName, params string[] execArgs)
        {
            var location = Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);

            string filePath = $"{directory}/LiveSyncServerProcess.txt";

            if (File.Exists(filePath))
            {

                //Lê o número do processo do server iniciado anteriormente se houver
                using (StreamReader readtext = new StreamReader(filePath))
                {
                    string content = readtext.ReadToEnd();

                    if (int.TryParse(content, out int processId))
                    {
                        //Tenta encessar o processo
                        try
                        {
                            var existingProc = Process.GetProcessById(processId);
                            existingProc?.Kill();

                            Console.WriteLine("Old server killed");
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                }
            }

            Console.WriteLine("Starting the new server");


            //ThreadStart ths = new ThreadStart(() => {

            //Inicia um novo processo
            var proc = new Process();
            proc.StartInfo.FileName = execName; //"dotnet";
            proc.StartInfo.Arguments = string.Join(" ", execArgs);
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized; //ProcessWindowStyle.Hidden;
            proc.StartInfo.CreateNoWindow = false;
            proc.StartInfo.UseShellExecute = true;

            proc.Start();

            Console.WriteLine($"Server started with process ID = {proc.Id}");

            //Salva o Id do processo
            using (StreamWriter writetext = new StreamWriter(filePath))
            {
                writetext.Write(proc.Id);
            }
            //});
            //Thread th = new Thread(ths);
            //th.Start();
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
