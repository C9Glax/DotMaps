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
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
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
                    this.isRendering = true;
                    this.pictureBox1.Image = Render.DrawMap(this.mapGraph, this.renderCenter, this.scale, this.pens, this.Width * 3, this.Height * 3, this.coreCount);
                    this.isRendering = false;

                    this.pictureBox1.MouseDown += new MouseEventHandler(this.MapMouseDown);
                    this.pictureBox1.MouseMove += new MouseEventHandler(this.MapMouseMove);
                    this.pictureBox1.MouseUp += new MouseEventHandler(this.MapMouseUp);
                }).Start();
            }
            this.previousMapLocation = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);
            this.allowMovement = true;
        }

        private void scaleMouseWheel(object sender, MouseEventArgs e)
        {
            int ticks = e.Delta / 120;
            this.scale += ticks * scalerate;

            if (!this.isRendering && this.scale >= minScale && this.scale <= maxScale)
                this.pictureBox1.Image = Render.DrawMap(this.mapGraph, this.renderCenter, this.scale, this.pens, this.Width * 3, this.Height * 3, this.coreCount);

            if (ticks < 0 && this.scale < minScale)
                this.scale = 50;
            else if (this.scale > maxScale)
                this.scale = 250;
        }


        private void MapMouseMove(object sender, MouseEventArgs e)
        {
            Point currentMouseLocation = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);
            if (e.Button.ToString().ToLower().Contains("left") && this.allowMovement)
            {
                Point locationDifference = new Point(currentMouseLocation.X - previousMouseLocation.X, currentMouseLocation.Y - previousMouseLocation.Y);
                this.pictureBox1.Location = new Point(this.pictureBox1.Location.X + locationDifference.X, this.pictureBox1.Location.Y + locationDifference.Y);
            }
            this.previousMouseLocation = currentMouseLocation;
            this.mousePositionLabel.Text = string.Format("| Mouse: {0}, {1}", currentMouseLocation.X, currentMouseLocation.Y);
        }

        private void MapMouseDown(object sender, MouseEventArgs e)
        {
            this.previousMapLocation = new Point(this.pictureBox1.Left, this.pictureBox1.Top);
        }

        private void MapMouseUp(object sender, MouseEventArgs e)
        {
            Point currentMapLocation = new Point(this.pictureBox1.Left, this.pictureBox1.Top);
            if (e.Button.ToString().ToLower().Contains("left") && this.allowMovement)
            {
                Point locationDifference = new Point(currentMapLocation.X - previousMapLocation.X, currentMapLocation.Y - previousMapLocation.Y);
                this.renderCenter = new _3DNode(this.renderCenter.lat + (locationDifference.Y / (100f * this.scale)), this.renderCenter.lon + (-locationDifference.X / (100f * this.scale)));
                this.latLngLabel.Text = string.Format("| Lat: {0} Lon: {1}", this.renderCenter.lat, this.renderCenter.lon);
                this.pictureBox1.Image = Render.DrawMap(this.mapGraph, this.renderCenter, this.scale, this.pens, this.Width * 3, this.Height * 3, this.coreCount);
                this.pictureBox1.Location = new Point(-this.Width, -this.Height);

                this.previousMapLocation = currentMapLocation;
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
                    Converter converter = new Converter(Converter.ConvertType.SLIM, inputFile.FileName, outputFile.FileName);
                    converter.OnStatusChange += (s, s1) =>
                    {
                        this.statusLabel.Text = "Slim: " + s1.status;
                    };
                    new Thread(delegate () {
                        converter.Start();
                    }).Start();
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
                    Converter converter = new Converter(Converter.ConvertType.SPLIT, inputFile.FileName, outputFile.FileName);
                    converter.OnStatusChange += (s, s1) =>
                    {
                        this.statusLabel.Text = "Split: " + s1.status;
                    };
                    new Thread(delegate () {
                        converter.Start();
                    }).Start();
                }
            }
        }
        private void saveRenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog outputFile = new SaveFileDialog
            {
                FileName = "Render.png",
                DefaultExt = ".png",
                Filter = "(.png)|*.png",
                InitialDirectory = Environment.CurrentDirectory,
                RestoreDirectory = true
            };
            if (outputFile.ShowDialog() == DialogResult.OK)
            {
                this.pictureBox1.Image.Save(outputFile.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
