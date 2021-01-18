using DotMaps.Datastructures;
using DotMaps.Utils;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Management;

namespace DotMaps
{
    class DrawEntireMap
    {
        static void Main(string[] args)
        {
            DrawMap();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }


        private static void DrawMap()
        {
            Console.WriteLine("Path to .osm file (E:\\Desktop\\map.osm)");
            string path = Console.ReadLine();
            Console.WriteLine("Outputfile (E:\\Desktop\\render.png");
            string outputFilePath = Console.ReadLine();
            Console.WriteLine("Scale (200)");
            int scale = Convert.ToInt32(Console.ReadLine());


            Importer importer = new Importer();
            importer.OnProgress += (s, e) =>
            {
                string progressString = string.Format("{0:#0.00}%", e.progress);
                int consoleLeft = Console.CursorLeft, consoleTop = Console.CursorTop;
                Console.SetCursorPosition(Console.WindowWidth - progressString.Length, consoleTop);
                Console.WriteLine(progressString);
                Console.SetCursorPosition(consoleLeft, consoleTop);
            };
            importer.OnStatusChange += (s, e) =>
            {
                Console.WriteLine(e.status);
            };
            Graph mapGraph = importer.ImportOSM(path);
            _3DNode renderCenter = new _3DNode(mapGraph.minLat + (mapGraph.maxLat - mapGraph.minLat) / 2, mapGraph.minLon + (mapGraph.maxLon - mapGraph.minLon) / 2);

            Hashtable pens = new Hashtable();
            foreach (string type in File.ReadAllLines("roadRender.txt"))
                if (!type.StartsWith("//"))
                {
                    string key = type.Split(',')[0];
                    if (!pens.ContainsKey(key))
                        pens.Add(key, new Pen(Color.FromName(type.Split(',')[2]), Convert.ToInt32(type.Split(',')[1])));
                }

            int coreCount = 0;
            foreach (ManagementObject result in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                coreCount += int.Parse(result["NumberOfCores"].ToString());

            _2DNode bottomLeft = Functions._2DNodeFrom3DNode(new _3DNode(mapGraph.minLat, mapGraph.minLon), renderCenter, scale);
            _2DNode topRight = Functions._2DNodeFrom3DNode(new _3DNode(mapGraph.maxLat, mapGraph.maxLon), renderCenter, scale);
            int width = (int)(topRight.X - bottomLeft.X);
            int height = (int)(bottomLeft.Y - topRight.Y);

            Bitmap render = Render.DrawMap(mapGraph, renderCenter, scale, pens, width, height, coreCount);

            render.Save(outputFilePath);
        }

    }
}
