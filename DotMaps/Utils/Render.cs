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
            Vector cameraVector = Functions.GetCameraVector(renderCenter, scale);

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
                        if (Functions.DistanceBetweenNodes(renderCenter, node) * scale < (renderHeight > renderWidth ? renderHeight : renderWidth))
                        {
                            foreach (Graph.Connection connection in node.GetConnections())
                            {
                                Pen pen = pens[connection.roadType] == null ? (Pen)pens["default"] : (Pen)pens[connection.roadType];

                                _2DNode _2dfrom = Functions._2DNodeFromGraphNodeAndVector(node, renderCenter, cameraVector);
                                _2DNode _2dto;
                                foreach (_3DNode coord in connection.coordinates)
                                {
                                    _2dto = Functions._2DNodeFrom3DNodeAndCameraVector(coord, renderCenter, cameraVector);
                                    draw.Enqueue(new Line(pen,
                                        new _2DNode(_2dfrom.X + (renderWidth / 2), _2dfrom.Y + (renderHeight / 2)),
                                        new _2DNode(_2dto.X + (renderWidth / 2), _2dto.Y + (renderHeight / 2))));
                                    _2dfrom = _2dto;
                                }
                                _2dto = Functions._2DNodeFromGraphNodeAndVector(connection.neighbor, renderCenter, cameraVector);
                                draw.Enqueue(new Line(pen,
                                        new _2DNode(_2dfrom.X + (renderWidth / 2), _2dfrom.Y + (renderHeight / 2)),
                                        new _2DNode(_2dto.X + (renderWidth / 2), _2dto.Y + (renderHeight / 2))));
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
                        float halfPenWidth = line.pen.Width / 2;
                        g.FillEllipse(new SolidBrush(line.pen.Color), line.from.X - halfPenWidth, line.from.Y - halfPenWidth, line.pen.Width, line.pen.Width);
                        g.DrawLine(line.pen, line.from.X, line.from.Y, line.to.X, line.to.Y);
                        g.FillEllipse(new SolidBrush(line.pen.Color), line.to.X - halfPenWidth, line.to.Y - halfPenWidth, line.pen.Width, line.pen.Width);
                    }
                }
                Console.WriteLine(string.Format("Done :) Total/Rendered Nodes: {0}/{1}", nodes.Length, renderedNodes));
                return render;
            }
        }
    }
}
