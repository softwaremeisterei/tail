using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tail
{
    class Program
    {
        static int TailLineCount = 10;
        static string FilePath;

        static void Main(string[] args)
        {
            if (args.Length != 1 || args[0].Equals("-h", StringComparison.OrdinalIgnoreCase))
            {
                PrintUsage();
                return;
            }

            FilePath = args[0].Trim('\"');

            if (!File.Exists(FilePath))
            {
                Console.Error.WriteLine("file not found");
                return;
            }

            Refresh();

            using (var watcher = new FileSystemWatcher())
            {
                var dir = Path.GetDirectoryName(FilePath);
                watcher.Path = string.IsNullOrEmpty(dir) ? Directory.GetCurrentDirectory() : dir;
                watcher.Filter = Path.GetFileName(FilePath);
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;
                watcher.Changed += OnChanged;
                watcher.EnableRaisingEvents = true;

                while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Refresh();
        }

        private static void Refresh()
        {
            var tail = new List<String>();
            var encoding = GetEncoding(FilePath);
            using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream, encoding))
                {
                    while (reader.Peek() >= 0)
                    {
                        var line = reader.ReadLine();
                        tail.Add(line);

                        if (tail.Count > TailLineCount)
                        {
                            tail.RemoveAt(0);
                        }
                    }
                }
            }
            Console.Clear();
            foreach (var line in tail)
            {
                Console.WriteLine(line);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: tail [FILE]");
        }

        public static Encoding GetEncoding(string filename)
        {
            try
            {
                using (var reader = new StreamReader(filename, Encoding.Default, true))
                {
                    reader.Peek();
                    var encoding = reader.CurrentEncoding;
                    return encoding;
                }
            }
            catch
            {
                return Encoding.Default;
            }

        }
    }
}
