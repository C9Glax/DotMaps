using System;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using System.Drawing;
using DotMaps.Utils;
using DotMaps.Datastructures;

namespace DotMaps
{
    partial class MapDisplay
    {
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
            if (fd.ShowDialog() == DialogResult.OK)
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
                    this.DrawMap(this.coreCount);
                }).Start();
            }
            this.previousLocation = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);
            this.allowMovement = true;
        }

        private void scaleMouseWheel(object sender, MouseEventArgs e)
        {
            int ticks = e.Delta / 120;
            this.scale += ticks * scalerate;

            if (!rendering && this.scale >= minScale && this.scale <= maxScale)
                DrawMap(this.coreCount);

            if (ticks < 0 && this.scale < minScale)
                this.scale = 50;
            else if (this.scale > maxScale)
                this.scale = 250;
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
                this.DrawMap(this.coreCount);

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
                if (outputFile.ShowDialog() == DialogResult.OK)
                {
                    Converter.Slimmer slimmer = new Converter.Slimmer();
                    slimmer.OnStatusChange += (s, s1) =>
                    {
                        this.statusLabel.Text = "Slim: " + s1.status;
                    };
                    new Thread(delegate () { slimmer.SlimOSMFormat(inputFile.FileName, outputFile.FileName); }).Start();
                }
            }
        }

        private void TileDisplay_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.memoryUsageThread.Abort();
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
