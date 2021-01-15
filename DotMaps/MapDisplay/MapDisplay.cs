using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using DotMaps.Datastructures;
using DotMaps.Utils;

namespace DotMaps
{
    public partial class TileDisplay : Form
    {
        public TileDisplay()
        {
            InitializeComponent();
            this.scale = scaleSlider.Value;
            this.renderer = this.CreateGraphics();
            this.previousLocation = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);

            ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
            foreach (ManagementObject result in searcher.Get())
            {
                this.totalMemory = Convert.ToInt64(result["TotalVisibleMemorySize"]);
            }

            new Thread(MemoryUsage).Start();
        }

        private void MemoryUsage()
        {
            while (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.AbortRequested)
            {
                this.ChangeMemoryUsage(Process.GetCurrentProcess().PrivateMemorySize64);
                Thread.Sleep(1000);
            }
        }

        private delegate void ChangeMemoryUsageDelegate(long used);
        private void ChangeMemoryUsage(long used)
        {
            if (this.memoryUsageBar.Control.InvokeRequired)
                this.memoryUsageBar.Control.Invoke(new ChangeMemoryUsageDelegate(this.ChangeMemoryUsage), used);
            else
                this.memoryUsageBar.Value = (int)((100.0 / totalMemory) * (used / 1024));

            if (this.InvokeRequired)
                this.Invoke(new ChangeMemoryUsageDelegate(this.ChangeMemoryUsage), used);
            else
                this.memoryUsageLabel.Text = string.Format("{2:#0.0}% | {0}/{1}MB", used / 1024 / 1024, totalMemory / 1024, (100.0 / totalMemory) * (used / 1024));
        }
        private delegate void ChangeStatusDelegate(string status);
        private void ChangeStatusSafe(string status)
        {
            if (this.InvokeRequired)
                this.Invoke(new ChangeStatusDelegate(ChangeStatusSafe), status);
            else
                this.statusLabel.Text = status;
        }
        private delegate void ChangeProgressDelegate(int progress);
        private void ChangeProgressSafe(int progress)
        {
            if (this.toolStripProgressBar1.Control.InvokeRequired)
                this.toolStripProgressBar1.Control.Invoke(new ChangeProgressDelegate(this.ChangeProgressSafe), progress);
            else
                this.toolStripProgressBar1.Value = progress;
        }

        private delegate void ChangeLatLngDelegate(string text);
        private void ChangeLatLngSafe(string text)
        {
            if (this.InvokeRequired)
                this.Invoke(new ChangeLatLngDelegate(ChangeLatLngSafe), text);
            else
                this.latLngLabel.Text = text;
        }

        private long totalMemory;
        private Point previousLocation;
        private int scale;
        private Graph mapGraph;
        private _3DNode renderCenter;
        private Graphics renderer;
        private Hashtable pens;
        private bool allowMovement = false;

        private void openosmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.allowMovement = false;
            OpenFileDialog fd = new OpenFileDialog
            {
                FileName = "map.osm",
                DefaultExt = ".osm",
                Filter = "Open Street Map (.osm)|*.osm|All files (*.*)|*.*",
                InitialDirectory = Environment.CurrentDirectory,
                RestoreDirectory = true
            };
            if(fd.ShowDialog() == DialogResult.OK)
            {
                this.pens = new Hashtable();
                foreach (string type in File.ReadAllLines("roadRender.txt"))
                    if (!type.StartsWith("//"))
                    {
                        string key = type.Split(',')[0];
                        if (!pens.ContainsKey(key))
                            pens.Add(key, new Pen(Color.FromName(type.Split(',')[2]), Convert.ToInt32(type.Split(',')[1])));
                    }

                this.SetTitle(fd.FileName);

                new Thread(delegate ()
                {
                    this.ChangeStatusSafe("Importing Map. This will take a while depending on size");
                    Importer importer = new Importer();
                    importer.OnProgress += (s, p) => { this.ChangeProgressSafe(p.progress); };
                    importer.OnStatusChange += (s, s1) => { this.ChangeStatusSafe(s1.status); };
                    this.mapGraph = importer.ImportOSM(fd.FileName);
                    this.renderCenter = new _3DNode(this.mapGraph.minLat + (this.mapGraph.maxLat - this.mapGraph.minLat) / 2, this.mapGraph.minLon + (this.mapGraph.maxLon - this.mapGraph.minLon) / 2);
                    this.ChangeLatLngSafe(string.Format("| Lat: {0} Lon: {1} |", this.renderCenter.lat, this.renderCenter.lon));
                    this.ChangeStatusSafe("Drawing Map");
                    this.DrawMap();
                    this.ChangeStatusSafe("Done :)");
                }).Start();
            }
            this.previousLocation = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);
            this.allowMovement = true;
        }

        private void SetTitle(string title)
        {
            this.Text = "TileDisplay - " + title;
        }

        private void DrawMap()
        {
            if (this.mapGraph == null)
                return;
            this.renderer.Clear(Color.Transparent);

            Graph.GraphNode[] nodes = this.mapGraph.GetNodes();
            for (int i = 0; i < nodes.Length; i++)
            {
                Graph.GraphNode node = nodes[i];
                this.ChangeProgressSafe((int)((100.0 / nodes.Length) * (i + 1)));
                this.ChangeStatusSafe(string.Format("{0:#0.0}% Drawing Map", ((100.0 / nodes.Length) * (i + 1))));
                if (Functions.DistanceBetweenNodes(this.renderCenter, node) < (this.Height < this.Width ? this.Width : this.Height) / 2 * scale * 2)
                {
                    foreach (Graph.Connection connection in node.GetConnections())
                    {
                        Pen pen = (Pen)pens[connection.roadType];
                        if (pen == null)
                            pen = (Pen)pens["default"];

                        _2DNode _2dfrom = Functions._2DNodeFromGraphNode(node, this.renderCenter, this.scale);
                        _2DNode _2dto;
                        foreach (_3DNode coord in connection.coordinates)
                        {
                            _2dto = Functions._2DNodeFrom3DNode(coord, this.renderCenter, this.scale);

                            this.renderer.FillEllipse(new SolidBrush(pen.Color), (_2dfrom.X - pen.Width / 2) + (this.Width / 2), (_2dfrom.Y - pen.Width / 2) + (this.Height / 2), pen.Width, pen.Width);
                            this.renderer.DrawLine(pen, _2dfrom.X + (this.Width / 2), _2dfrom.Y + (this.Height / 2), _2dto.X + (this.Width / 2), _2dto.Y + (this.Height / 2));
                            this.renderer.FillEllipse(new SolidBrush(pen.Color), (_2dto.X - pen.Width / 2) + (this.Width / 2), (_2dto.Y - pen.Width / 2) + (this.Height / 2), pen.Width, pen.Width);

                            _2dfrom = _2dto;
                        }

                        _2dto = Functions._2DNodeFromGraphNode(connection.neighbor, this.renderCenter, this.scale);

                        this.renderer.FillEllipse(new SolidBrush(pen.Color), (_2dfrom.X - pen.Width / 2) + (this.Width / 2), (_2dfrom.Y - pen.Width / 2) + (this.Height / 2), pen.Width, pen.Width);
                        this.renderer.DrawLine(pen, _2dfrom.X + (this.Width / 2), _2dfrom.Y + (this.Height / 2), _2dto.X + (this.Width / 2), _2dto.Y + (this.Height / 2));
                        this.renderer.FillEllipse(new SolidBrush(pen.Color), (_2dto.X - pen.Width / 2) + (this.Width / 2), (_2dto.Y - pen.Width / 2) + (this.Height / 2), pen.Width, pen.Width);
                    }
                }
            }
        }

        private void scaleSlider_ValueChanged(object sender, EventArgs e)
        {
            this.scale = scaleSlider.Value;
            DrawMap();
        }


        private void TileDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentMouseLocation = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);
            mousePositionLabel.Text = string.Format("| Mouse: {0}, {1}", currentMouseLocation.X, currentMouseLocation.Y);
        }

        private void TileDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            this.previousLocation = new Point(e.X, e.Y);
        }

        private void TileDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            Point currentMouseLocation = new Point(e.X, e.Y);
            if (e.Button.ToString().ToLower().Contains("left") && this.allowMovement)
            {
                Point locationDifference = new Point(currentMouseLocation.X - previousLocation.X, currentMouseLocation.Y - previousLocation.Y);
                Console.WriteLine("{0},{1} == {4},{5} ==> {2},{3}", previousLocation.X, previousLocation.Y, currentMouseLocation.X, currentMouseLocation.Y, locationDifference.X, locationDifference.Y);
                this.renderCenter = new _3DNode(this.renderCenter.lat + (locationDifference.Y / (100f * this.scale)), this.renderCenter.lon + (-locationDifference.X / (100f * this.scale)));
                this.latLngLabel.Text = string.Format("| Lat: {0} Lon: {1}", this.renderCenter.lat, this.renderCenter.lon);
                this.DrawMap();

                previousLocation = currentMouseLocation;
            }
        }

        private void slimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog inputFile = new OpenFileDialog
            {
                Title = "Select File to Slim...",
                FileName = ".osm",
                DefaultExt = ".osm",
                Filter = "Open Street Map (.osm)|*.osm|All files (*.*)|*.*",
                InitialDirectory = Environment.CurrentDirectory,
                RestoreDirectory = true
            };
            if (inputFile.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog outputFile = new SaveFileDialog
                {
                    FileName = "Slim.osm",
                    DefaultExt = ".osm",
                    Filter = "Open Street Map (.osm)|*.osm|All files (*.*)|*.*",
                    InitialDirectory = Environment.CurrentDirectory,
                    RestoreDirectory = true
                };
                if(outputFile.ShowDialog() == DialogResult.OK)
                {
                    Converter.Slimmer slimmer = new Converter.Slimmer();
                    slimmer.OnStatusChange += (s, s1) =>
                    {
                        this.statusLabel.Text = "Slim: " + s1.status;
                    };
                    new Thread(delegate() { slimmer.SlimOSMFormat(inputFile.FileName, outputFile.FileName); } ).Start();
                }
            }
        }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TileDisplay());
        }

        private void splitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog inputFile = new OpenFileDialog
            {
                Title = "Select File to Split Ways...",
                FileName = ".osm",
                DefaultExt = ".osm",
                Filter = "Open Street Map (.osm)|*.osm|All files (*.*)|*.*",
                InitialDirectory = Environment.CurrentDirectory,
                RestoreDirectory = true
            };
            if (inputFile.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog outputFile = new SaveFileDialog
                {
                    FileName = "Split.osm",
                    DefaultExt = ".osm",
                    Filter = "Open Street Map (.osm)|*.osm|All files (*.*)|*.*",
                    InitialDirectory = Environment.CurrentDirectory,
                    RestoreDirectory = true
                };
                if (outputFile.ShowDialog() == DialogResult.OK)
                {
                    new Converter.Splitter().SplitWays(inputFile.FileName, outputFile.FileName);
                }
            }
        }
    }
}
