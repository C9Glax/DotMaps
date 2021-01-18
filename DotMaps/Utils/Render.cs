using DotMaps.Datastructures;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;

namespace DotMaps.Utils
{
    public struct Render
    {
        public static Bitmap DrawMap(Graph mapGraph, _3DNode renderCenter, int scale, Hashtable pens, int renderWidth, int renderHeight, int threads)
        {
            Graph.GraphNode[] nodes = mapGraph.GetNodes();
            int nodesPerThread = (int)Math.Ceiling(nodes.Length / (double)threads);
            int activeThreads = threads;
            int renderedNodes = 0;

            ConcurrentQueue<Line> draw = new ConcurrentQueue<Line>();
            Bitmap render = new Bitmap(renderWidth, renderHeight);

            Console.WriteLine("Drawing Map...");
            for (int thread = 0; thread < threads; thread++)
            {
                int startNodeIndex = thread * nodesPerThread;
                int maxNodeIndex = (thread + 1) * nodesPerThread;
                new Thread(delegate ()
                {
                    for (; startNodeIndex < maxNodeIndex && startNodeIndex < nodes.Length; startNodeIndex++)
                    {
                        Graph.GraphNode node = nodes[startNodeIndex];
                        if (Functions.DistanceBetweenNodes(renderCenter, node) < (renderHeight < renderWidth ? renderHeight : renderWidth) / 800 * scale)
                        {
                            foreach (Graph.Connection connection in node.GetConnections())
                            {
                                Pen pen = pens[connection.roadType] == null ? (Pen)pens["default"] : (Pen)pens[connection.roadType];

                                _2DNode _2dfrom = Functions._2DNodeFromGraphNode(node, renderCenter, scale);
                                _2DNode _2dto;
                                foreach (_3DNode coord in connection.coordinates)
                                {
                                    _2dto = Functions._2DNodeFrom3DNode(coord, renderCenter, scale);
                                    draw.Enqueue(new Line(pen, _2dfrom, _2dto));
                                    _2dfrom = _2dto;
                                }
                                _2dto = Functions._2DNodeFromGraphNode(connection.neighbor, renderCenter, scale);
                                draw.Enqueue(new Line(pen, _2dfrom, _2dto));
                            }
                            renderedNodes++;
                        }
                    }
                    activeThreads--;
                }).Start();
            }
            Console.WriteLine("Total Nodes: {0}", nodes.Length);

            using (Graphics g = Graphics.FromImage(render))
            {
                while (activeThreads > 0 || draw.Count > 0)
                {
                    if (draw.Count > 0)
                    {
                        Line line;
                        while (!draw.TryDequeue(out line)) ;
                        g.FillEllipse(new SolidBrush(line.pen.Color), (line.from.X - line.pen.Width / 2) + (renderWidth / 2), (line.from.Y - line.pen.Width / 2) + (renderHeight / 2), line.pen.Width, line.pen.Width);
                        g.DrawLine(line.pen, line.from.X + (renderWidth / 2), line.from.Y + (renderHeight / 2), line.to.X + (renderWidth / 2), line.to.Y + (renderHeight / 2));
                        g.FillEllipse(new SolidBrush(line.pen.Color), (line.to.X - line.pen.Width / 2) + (renderWidth / 2), (line.to.Y - line.pen.Width / 2) + (renderHeight / 2), line.pen.Width, line.pen.Width);
                    }
                }
                Console.WriteLine(string.Format("Done :) Total/Rendered Nodes: {0}/{1}", nodes.Length, renderedNodes));
                return render;
            }
        }
    }
}
