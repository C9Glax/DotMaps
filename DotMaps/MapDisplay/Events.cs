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
                    this.renderer = new Renderer(this.renderCenter, this.scale, this.coreCount);
                    this.ChangeLatLngSafe(string.Format("| Lat: {0} Lon: {1} |", this.renderCenter.lat, this.renderCenter.lon));
                    this.ChangeStatusSafe("Drawing Map");
                    this.isRendering = true;
                    this.pictureBox1.Image = renderer.DrawMap(this.mapGraph, this.pens, this.Width * 3, this.Height * 3);
                    this.isRendering = false;

                    this.pictureBox1.MouseDown += new MouseEventHandler(this.MapMouseDown);
                    this.pictureBox1.MouseMove += new MouseEventHandler(this.MapMouseMove);
                    this.pictureBox1.MouseUp += new MouseEventHandler(this.MapMouseUp);
                    this.pictureBox1.MouseWheel += new MouseEventHandler(this.scaleMouseWheel);
                }).Start();
            }
            this.mapPrevious2DLocation = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);
            this.allowMovement = true;
        }

        private void scaleMouseWheel(object sender, MouseEventArgs e)
        {
            int ticks = e.Delta / 120;
            this.scale += ticks * scalerate;

            if (!this.isRendering && this.scale >= minScale && this.scale <= maxScale)
            {
                this.isRendering = true;
                this.renderer = new Renderer(this.renderCenter, this.scale, this.coreCount);
                this.pictureBox1.Image = this.renderer.DrawMap(this.mapGraph, this.pens, this.Width * 3, this.Height * 3);


                this.isRendering = false;
            }

            if (ticks < 0 && this.scale < minScale)
                this.scale = 50;
            else if (this.scale > maxScale)
                this.scale = 250;
        }


        private void MapMouseMove(object sender, MouseEventArgs e)
        {
            Point mouseCurrent2DPosition = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);
            if (e.Button.ToString().ToLower().Contains("left") && this.allowMovement)
            {
                Point locationDifference = new Point(mouseCurrent2DPosition.X - previousMouseLocation.X, mouseCurrent2DPosition.Y - previousMouseLocation.Y);
                this.pictureBox1.Location = new Point(this.pictureBox1.Location.X + locationDifference.X, this.pictureBox1.Location.Y + locationDifference.Y);
            }
            this.previousMouseLocation = mouseCurrent2DPosition;
            _3DNode mouseCurrent3DPosition = this.renderer.GetGeoCoordinatesForPosition(new _2DNode(e.X - (this.pictureBox1.Width / 2), this.pictureBox1.Height - e.Y - (this.pictureBox1.Height / 2)));
            this.mousePositionLabel.Text = string.Format("| Mouse: {0}, {1}", mouseCurrent3DPosition.lat, mouseCurrent3DPosition.lon);
        }

        private void MapMouseDown(object sender, MouseEventArgs e)
        {
            this.mapPrevious2DLocation = new Point(this.pictureBox1.Left, this.pictureBox1.Top);
        }

        private void MapMouseUp(object sender, MouseEventArgs e)
        {
            Point mapCurrent2DLocation = new Point(this.pictureBox1.Left, this.pictureBox1.Top);
            if (e.Button.ToString().ToLower().Contains("left") && this.allowMovement)
            {
                Point locationDifference = new Point(mapCurrent2DLocation.X - mapPrevious2DLocation.X, mapCurrent2DLocation.Y - mapPrevious2DLocation.Y);
                _2DNode mapCenter = new _2DNode(-locationDifference.X, locationDifference.Y);
                this.renderCenter = this.renderer.GetGeoCoordinatesForPosition(mapCenter);
                this.latLngLabel.Text = string.Format("| Lat: {0} Lon: {1}", this.renderCenter.lat, this.renderCenter.lon);
                this.renderer = new Renderer(this.renderCenter, this.scale, this.coreCount);
                this.pictureBox1.Image = this.renderer.DrawMap(this.mapGraph, this.pens, this.Width * 3, this.Height * 3);
                this.pictureBox1.Location = new Point(-this.Width, -this.Height);

                this.mapPrevious2DLocation = mapCurrent2DLocation;
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
