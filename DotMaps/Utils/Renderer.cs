using DotMaps.Datastructures;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;

namespace DotMaps.Utils
{
    public class Renderer
    {
        const int earthRadius = 6371;
        private Vector camera;
        private int scale;
        public Renderer(Vector camera, int scale)
        {
            this.camera = camera;
            this.scale = scale;
        }

        public Renderer(_3DNode cameraCenter, int scale)
        {
            this.camera = Functions._3DNodeToVector(cameraCenter).Scale(scale * earthRadius);
            this.scale = scale;
        }

        public _2DNode GetCoordinatesFromCenter(_3DNode node)
        {
            Vector nodeVector = Functions._3DNodeToVector(node);

            //Intersection between line through node and camera-plane
            double factor = (Math.Pow(camera.x, 2) + Math.Pow(camera.y, 2) + Math.Pow(camera.z, 2)) /
                (camera.x * nodeVector.x + camera.y * nodeVector.y + camera.z * nodeVector.z);
            Vector toLocation = nodeVector.Scale(factor).Subtract(camera);
            

            return new _2DNode((float)toLocation.z, -(float)toLocation.y);
        }

        public Bitmap DrawMap(Graph mapGraph, _3DNode cameraCenter, Hashtable pens, int renderWidth, int renderHeight, int threads)
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
                        if (Functions.DistanceBetweenNodes(cameraCenter, node) * scale < (renderHeight > renderWidth ? renderHeight : renderWidth))
                        {
                            foreach (Graph.Connection connection in node.GetConnections())
                            {
                                Pen pen = pens[connection.roadType] == null ? (Pen)pens["default"] : (Pen)pens[connection.roadType];

                                _2DNode _2dfrom = this.GetCoordinatesFromCenter(node.coordinates);
                                _2DNode _2dto;
                                foreach (_3DNode coord in connection.coordinates)
                                {
                                    _2dto = this.GetCoordinatesFromCenter(coord);
                                    draw.Enqueue(new Line(pen,
                                        new _2DNode(_2dfrom.X + (renderWidth / 2), _2dfrom.Y + (renderHeight / 2)),
                                        new _2DNode(_2dto.X + (renderWidth / 2), _2dto.Y + (renderHeight / 2))));
                                    _2dfrom = _2dto;
                                }
                                _2dto = this.GetCoordinatesFromCenter(connection.neighbor.coordinates);
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
