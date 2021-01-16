using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using DotMaps.Datastructures;
using DotMaps.Utils;

namespace DotMaps
{
    public partial class MapDisplay : Form
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MapDisplay());
        }

        private long totalMemory;
        private int coreCount;
        private Thread memoryUsageThread;
        private const int scalerate = 50, minScale = 50, maxScale = 250;

        public MapDisplay()
        {
            InitializeComponent();
            this.scale = 100;
            this.previousLocation = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);

            foreach (ManagementObject result in new ManagementObjectSearcher(new ObjectQuery("SELECT * FROM Win32_OperatingSystem")).Get())
                this.totalMemory = Convert.ToInt64(result["TotalVisibleMemorySize"]);

            foreach (ManagementObject result in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                this.coreCount += int.Parse(result["NumberOfCores"].ToString());

            this.memoryUsageThread = new Thread(MemoryUsageThread);
            this.memoryUsageThread.Start();
            this.MouseWheel += new MouseEventHandler(this.scaleMouseWheel);
        }

        private void MemoryUsageThread()
        {
            while (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.AbortRequested)
            {
                this.ChangeMemoryUsageSafe(Process.GetCurrentProcess().PrivateMemorySize64);
                Thread.Sleep(1000);
            }
        }

        private Point previousLocation;
        private int scale;
        private Graph mapGraph;
        private _3DNode renderCenter;
        private Hashtable pens;
        private bool allowMovement = false, rendering = false;

        private void SetTitle(string title)
        {
            this.Text = "MapDisplay - " + title;
        }

        private void DrawMap(int threads)
        {
            rendering = true;
            if (this.mapGraph == null)
            {
                rendering = false;
                return;
            }

            Graph.GraphNode[] nodes = this.mapGraph.GetNodes();
            int nodesPerThread = (int)Math.Ceiling((double)nodes.Length / (double)threads);
            int activeThreads = threads;
            int renderedNodes = 0;

            ConcurrentQueue<Line> draw = new ConcurrentQueue<Line>();
            Bitmap render = new Bitmap(this.Width * 3, this.Height * 3);
            new Thread(delegate ()
            {
                using (Graphics g = Graphics.FromImage(render))
                {
                    while (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.AbortRequested)
                    {
                        if (draw.Count > 0)
                        {
                            Line line;
                            while (!draw.TryDequeue(out line)) ;
                            g.FillEllipse(new SolidBrush(line.pen.Color), (line.from.X - line.pen.Width / 2) + (this.Width / 2), (line.from.Y - line.pen.Width / 2) + (this.Height / 2), line.pen.Width, line.pen.Width);
                            g.DrawLine(line.pen, line.from.X + (this.Width / 2), line.from.Y + (this.Height / 2), line.to.X + (this.Width / 2), line.to.Y + (this.Height / 2));
                            g.FillEllipse(new SolidBrush(line.pen.Color), (line.to.X - line.pen.Width / 2) + (this.Width / 2), (line.to.Y - line.pen.Width / 2) + (this.Height / 2), line.pen.Width, line.pen.Width);
                        }
                        else if (activeThreads == 0)
                        {
                            this.ChangeStatusSafe(string.Format("Done :) Total/Rendered Nodes: {0}/{1}", nodes.Length,renderedNodes));
                            this.BackgroundImage = render;
                            Thread.CurrentThread.Abort();
                        }
                    }
                }
            }).Start();

            this.ChangeStatusSafe("Drawing Map");
            for (int thread = 0; thread < threads; thread++)
            {
                int startNodeIndex = thread * nodesPerThread;
                int maxNodeIndex = (thread + 1) * nodesPerThread;
                Console.WriteLine("Thread {0} rendering Nodes {1} to {2}", Thread.CurrentThread.Name, startNodeIndex, maxNodeIndex - 1);
                new Thread(delegate ()
                {
                    for(; startNodeIndex < maxNodeIndex && startNodeIndex < nodes.Length; startNodeIndex++)
                    {
                        Graph.GraphNode node = nodes[startNodeIndex];
                        if (Functions.DistanceBetweenNodes(this.renderCenter, node) < (this.Height < this.Width ? this.Width : this.Height) / 800 * scale)
                        {
                            foreach (Graph.Connection connection in node.GetConnections())
                            {
                                Pen pen = this.pens[connection.roadType] == null ? (Pen)this.pens["default"] : (Pen)this.pens[connection.roadType];

                                _2DNode _2dfrom = Functions._2DNodeFromGraphNode(node, this.renderCenter, this.scale);
                                _2DNode _2dto;
                                foreach (_3DNode coord in connection.coordinates)
                                {
                                    _2dto = Functions._2DNodeFrom3DNode(coord, this.renderCenter, this.scale);
                                    draw.Enqueue(new Line(pen, _2dfrom, _2dto));
                                    _2dfrom = _2dto;
                                }
                                _2dto = Functions._2DNodeFromGraphNode(connection.neighbor, this.renderCenter, this.scale);
                                draw.Enqueue(new Line(pen, _2dfrom, _2dto));
                            }
                            renderedNodes++;
                        }
                    }
                    activeThreads--;
                }).Start();
            }
            Console.WriteLine("Total Nodes: {0}", nodes.Length);
            rendering = false;
        }
    }
}
