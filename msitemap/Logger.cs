using System;
using System.IO;

namespace msitemap
{
    public static class Logger
    {
        private static readonly string LogFilePath = "log.txt";
        private static readonly object _lock = new object();

        public static void Log(string message)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(message);
            lock (_lock)
            {
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
        }
    }
}
