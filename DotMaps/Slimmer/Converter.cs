using System;
using System.IO;

namespace DotMaps
{
    public partial class Converter
    {

        public enum ConvertType { SLIM, SPLIT, BOTH }
        private ConvertType operation;
        private string path, newPath;

        /*
        * Event Handler
        */
        public event StatusEventHandler OnStatusChange;
        public class StatusChangedEventArgs : EventArgs
        {
            public string status;
        }
        public delegate void StatusEventHandler(object sender, StatusChangedEventArgs e);

        public event ProgressEventHandler OnProgress;
        public class ProgressEventArgs : EventArgs
        {
            public float progress;
        }
        public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

        public static void Main(string[] args)
        {
            Console.WriteLine("S[l]im, S[p]lit or [B]oth?");
            char choice = Console.ReadKey().KeyChar;
            Console.WriteLine("Path to .osm (e.g. E:\\Desktop\\xxx.osm)");
            string path = Console.ReadLine();
            Console.WriteLine("Path to save _convert.osm (e.g. E:\\Desktop\\xxx.osm)");
            string newPath = Console.ReadLine();
            Converter converter;
            switch (choice)
            {
                case 'l':
                    converter = new Converter(ConvertType.SLIM, path, newPath);
                    break;
                case 'p':
                    converter = new Converter(ConvertType.SPLIT, path, newPath);
                    break;
                case 'b':
                    converter = new Converter(ConvertType.BOTH, path, newPath);
                    break;
                default:
                    Console.WriteLine("Invalid option '{0}'. Exiting", choice);
                    Environment.Exit(-1);
                    return;
            }
            converter.OnProgress += (s, e) =>
            {
                string progressString = string.Format("{0:#0.00}%", e.progress);
                int consoleLeft = Console.CursorLeft, consoleTop = Console.CursorTop;
                Console.SetCursorPosition(Console.WindowWidth - progressString.Length, consoleTop);
                Console.WriteLine(progressString);
                Console.SetCursorPosition(consoleLeft, consoleTop);
            };
            converter.OnStatusChange += (s, e) =>
            {
                Console.WriteLine(e.status);
            };
            converter.Start();
        }


        public Converter(ConvertType operation, string path, string newPath)
        {
            this.operation = operation;
            this.path = path;
            this.newPath = newPath;
        }

        public void Start()
        {
            switch (operation)
            {
                case ConvertType.SLIM:
                    Slimmer.SlimOSMFormat(this, path, newPath);
                    break;
                case ConvertType.SPLIT:
                    Splitter.SplitWays(this, path, newPath);
                    break;
                case ConvertType.BOTH:
                    Slimmer.SlimOSMFormat(this, path, newPath + ".temp");
                    Splitter.SplitWays(this, newPath + ".temp", newPath);
                    File.Delete(newPath + ".temp");
                    break;
            }
        }
    }
}
