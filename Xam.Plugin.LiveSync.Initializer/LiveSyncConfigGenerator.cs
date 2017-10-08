using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Xam.Plugin.LiveSync.Initializer
{
    class LiveSyncConfigGenerator
    {
        public static void GeneratePartialClass(string host)
        {
            var location = Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);

            using (StreamWriter writetext = new StreamWriter($"{directory}/LiveSyncConfig.cs"))
            {
                var fileContent = GetFileContent(host);
                writetext.Write(fileContent);
            }
        }
        public static void GenerateHostFile(string host)
        {
            var location = Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);

            using (StreamWriter writetext = new StreamWriter($"{directory}/LiveSync.host"))
            {
                writetext.Write(host);
            }
        }

        static string GetFileContent(string host)
        {
            var content = csharpFileTemplate.Replace("__HOST__", host);
            return content;
        }

        static string csharpFileTemplate = @"
            using System;
            namespace Xam.Plugin.LiveSync
            {
                public class LiveSyncConfig
                {
                    public const string HOST = ""__HOST__"";                   
                }
            }
        ";
    }
}
