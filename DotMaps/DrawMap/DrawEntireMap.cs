﻿using DotMaps.Datastructures;
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
            /*Renderer renderer = new Renderer(new _3DNode(0, 0), 200);
            _2DNode pp = renderer.GetCoordinatesFromCenter(new _3DNode(1, 1));
            Console.WriteLine("<{0} ,\t {1}>", pp.X, pp.Y);
            Console.ReadKey();*/
            DrawMap();
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

            Renderer renderer = new Renderer(renderCenter, scale);

            _2DNode topLeft = renderer.GetCoordinatesFromCenter(new _3DNode(mapGraph.maxLat, mapGraph.minLon));
            _2DNode bottomRight = renderer.GetCoordinatesFromCenter(new _3DNode(mapGraph.minLat, mapGraph.maxLon));
            int width = (int)(bottomRight.X - topLeft.X);
            int height = (int)(bottomRight.Y - topLeft.Y);

            Bitmap render = renderer.DrawMap(mapGraph, renderCenter, pens, width, height, coreCount);

            render.Save(outputFilePath);
        }

    }
}
