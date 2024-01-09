using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Windows.Shapes;

namespace SaveScammer
{
    internal class Misc
    {
        public static JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };
        static Misc()
        {
        }

        public static ulong HashString(string text)
        {
            return GetUInt64Hash(SHA256.Create(), text);
        }

        public static ulong GetUInt64Hash(HashAlgorithm hasher, string text)
        {
            using (hasher)
            {
                var bytes = hasher.ComputeHash(Encoding.Default.GetBytes(text));
                Array.Resize(ref bytes, bytes.Length + bytes.Length % 8); //make multiple of 8 if hash is not, for exampel SHA1 creates 20 bytes. 
                return Enumerable.Range(0, bytes.Length / 8) // create a counter for de number of 8 bytes in the bytearray
                    .Select(i => BitConverter.ToUInt64(bytes, i * 8)) // combine 8 bytes at a time into a integer
                    .Aggregate((x, y) => x ^ y); //xor the bytes together so you end up with a ulong (64-bit int)
            }
        }

        public static string ValidateFileName(string text)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var cleanFileName = new string(text.Where(m => !invalidChars.Contains(m)).ToArray<char>());
            return cleanFileName;
        }
        public static void PurgeDirectory(string directoryPath)
        {
            foreach (var subDir in Directory.EnumerateDirectories(directoryPath))
            {
                PurgeDirectory(subDir);
            }

            foreach(var subFile in Directory.EnumerateFiles(directoryPath))
            {
                File.Delete(subFile);
            }    
        }

        static string[] Sizes = { "B", "KB", "MB", "GB", "TB" };

        public static string GetSizeString(long sizeBytes)
        {
            double size = sizeBytes;
            int step = 0;
            while(size > 512)
            {
                size /= 1024.0;
                step++;
            }
            return $"{size:0.##} {Sizes[step]}";
        }

        internal static long GetLongTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        public static long GetFileSize(string path)
        {
            if(File.Exists(path))
                return new FileInfo(path).Length;
            return 0;
        }
    }
}
