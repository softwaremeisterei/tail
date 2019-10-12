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
                watcher.Path = Path.GetDirectoryName(FilePath);
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
            Console.Clear();
            var encoding = GetEncoding(FilePath);
            var lines = File.ReadAllLines(FilePath, encoding);
            var tailLines = lines.Length > TailLineCount ? lines.Skip(lines.Length - TailLineCount).ToArray() : lines;
            foreach (var tailLine in tailLines)
            {
                Console.WriteLine(tailLine);
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
