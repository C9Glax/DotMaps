using System;
using System.IO;
using System.Threading;

namespace DotMaps.Converter
{
    public class Converter
    {
        public static void Main(string[] args)
        {
            new Converter();
        }

        public Converter()
        {
            Console.WriteLine("S[l]im, S[p]lit or [B]oth?");
            char choice = Console.ReadKey().KeyChar;
            Console.WriteLine("Path to .osm (e.g. E:\\Desktop\\xxx.osm)");
            string path = Console.ReadLine();
            Console.WriteLine("Path to save _convert.osm (e.g. E:\\Desktop\\xxx.osm)");
            string newPath = Console.ReadLine();
            Thread statusPrinterThread = new Thread(StatusThread);
            statusPrinterThread.Start(this);

            switch (choice)
            {
                case 'l':
                    new Slimmer().SlimOSMFormat(path, newPath);
                    break;
                case 'p':
                    new Splitter().SplitWays(path, newPath);
                    break;
                case 'b':
                    new Slimmer().SlimOSMFormat(path, newPath + ".temp");
                    new Splitter().SplitWays(newPath + ".temp", newPath);
                    File.Delete(newPath + ".temp");
                    break;
                default:
                    Console.WriteLine("Invalid option '{0}'. Exiting", choice);
                    Environment.Exit(0);
                    break;
            }
            statusPrinterThread.Abort();

        }

        public string status = "";

        public static void StatusThread(object parent)
        {
            int line;
            while (Thread.CurrentThread.ThreadState != ThreadState.AbortRequested)
            {
                line = Console.CursorTop;
                Console.WriteLine(((Converter)parent).status);
                Console.CursorTop = line;
                Thread.Sleep(100);
            }
        }
    }
}
