using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xam.Plugin.LiveSync.Server
{
    public class FileHelper
    {
        public static string GetFileContent(string path)
        {
            try
            {
                using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var encoding = GetEncoding(stream);
                    
                    using (StreamReader streamReader = new StreamReader(stream, encoding))
                    {
                        string textContent = streamReader.ReadToEnd();
                        return textContent;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file at {path}. Exception: {ex.Message}");
                return "";
            }
        }

        public static Encoding GetEncoding(Stream file)
        {
            // Read the BOM
            var bom = new byte[4];
            //using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            //{
            file.Read(bom, 0, 4);
            file.Seek(0, SeekOrigin.Begin);
            //}

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }
    }
}
