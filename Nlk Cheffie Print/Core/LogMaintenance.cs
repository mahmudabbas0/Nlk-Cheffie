using System;
using System.IO;
using System.Text;

namespace Nlk_Cheffie_Print.Core
{
    internal static class LogMaintenance
    {
        private static readonly TimeSpan Retention = TimeSpan.FromDays(30);

        public static void Append(string path, string line, long maximumBytes)
        {
            try
            {
                string? directory = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(directory)) return;

                Directory.CreateDirectory(directory);
                Prune(directory);

                long incomingBytes = Encoding.UTF8.GetByteCount(line);
                if (File.Exists(path) && new FileInfo(path).Length + incomingBytes > maximumBytes)
                {
                    File.Move(path, path + ".previous", true);
                }

                File.AppendAllText(path, line, Encoding.UTF8);
            }
            catch
            {
                // Logging must never affect printing or network delivery.
            }
        }

        private static void Prune(string directory)
        {
            DateTime cutoff = DateTime.UtcNow.Subtract(Retention);
            foreach (string file in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                if (File.GetLastWriteTimeUtc(file) < cutoff)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
