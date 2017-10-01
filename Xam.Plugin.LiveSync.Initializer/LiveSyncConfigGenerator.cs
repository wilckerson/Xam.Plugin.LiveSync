using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Xam.Plugin.LiveSync.Initializer
{
    class LiveSyncConfigGenerator
    {
        public static void Generate(string host)
        {
            var location = Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);

            using (StreamWriter writetext = new StreamWriter($"{directory}/LiveSyncConfig.cs"))
            {
                var fileContent = GetFileContent(host);
                writetext.Write(fileContent);
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
                public partial class LiveSyncConfig : BaseLiveSyncConfig
                {
                    public new static string GetServerHost()
                    {
                        return ""__HOST__"";
                    }
                }
            }
        ";
    }
}
