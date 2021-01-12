using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using DotMaps.Utils;
using DotMaps.Datastructures;

namespace DotMaps.Tiles
{
    public partial class TileDisplay : Form
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TileDisplay());
        }

        public TileDisplay()
        {
            InitializeComponent();
        }

        private Point previousLocation;
        private float minLat, minLon, maxLat, maxLon;
        private int scale;
        private _3DNode center;

        private void DrawTiles(string path)
        {
            this.SuspendLayout();
            string[] subdirs = Directory.GetDirectories(path); //x
            int xAmount = subdirs.Length;
            string[] files = Directory.GetFiles(subdirs[0]);
            int yAmount = files.Length;
            int tileSize = Image.FromFile(files[0]).Width;
            int totalImages = xAmount * yAmount;
            this.toolStripStatusLabel1.Text = string.Format("Tilesize: {0} | Scale: {3} | Amount X: {1} Y: {2} | MinLat: {4} MinLon: {5}", tileSize, xAmount, yAmount, scale, minLat.ToString(), minLon.ToString());
            GroupBox map = new GroupBox
            {
                BackColor = Color.Black,
                Size = new Size(xAmount * tileSize, yAmount * tileSize),
                Location = new Point(this.Width / 2 - (xAmount * tileSize) / 2, this.Height / 2 - (xAmount * tileSize) / 2)
            };
            this.Controls.Add(map);
            for(int x = 0; x < xAmount; x++)
            {
                for(int y = 0; y < yAmount; y++)
                {
                    this.toolStripProgressBar1.Value = (x * yAmount + (y+1))*100/totalImages;
                    PictureBox tile = new PictureBox
                    {
                        Image = Image.FromFile(path + "\\" + x + "\\" + y + ".png"),
                        Location = new Point(x * tileSize, y * tileSize),
                        Size = new Size(tileSize, tileSize)
                    };
                    tile.MouseMove += new MouseEventHandler(tile_MouseMove);
                    map.Controls.Add(tile);
                }
            }
            toolStripStatusLabel2.Text = "X:" + map.Left + ",Y:" + map.Top;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void tile_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentLocation = new Point(Cursor.Position.X - this.Location.X, Cursor.Position.Y - this.Location.Y);
            toolStripStatusLabel3.Text = "Mouse:" + currentLocation.X + "," + currentLocation.Y;

            if (e.Button == MouseButtons.Left)
            {
                Cursor.Current = Cursors.Hand;
                ((PictureBox)sender).Parent.Left -= previousLocation.X - currentLocation.X;
                ((PictureBox)sender).Parent.Top -= previousLocation.Y - currentLocation.Y;
                toolStripStatusLabel2.Text = "X:" + ((PictureBox)sender).Parent.Left + ",Y:" + ((PictureBox)sender).Parent.Top;
            }
            else
                Cursor.Current = Cursors.Default;

            previousLocation = currentLocation;
            this.Invalidate();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string line = "";
                using (StreamReader reader = new StreamReader(folderBrowserDialog.SelectedPath + "\\map.osm"))
                    while (!line.Contains("bounds"))
                        line = reader.ReadLine();
                foreach (string part in line.Split(' '))
                {
                    if (part.Contains("minlat"))
                        this.minLat = Convert.ToSingle(part.Split('"')[1]);
                    else if (part.Contains("minlon"))
                        this.minLon = Convert.ToSingle(part.Split('"')[1]);
                    else if (part.Contains("maxlat"))
                        this.maxLat = Convert.ToSingle(part.Split('"')[1]);
                    else if (part.Contains("maxlon"))
                        this.maxLon = Convert.ToSingle(part.Split('"')[1]);
                }

                float latDiff = maxLat - minLat;
                float lonDiff = maxLon - minLon;
                this.center = new _3DNode(minLat + latDiff / 2, minLon + lonDiff / 2);

                using (StreamReader reader = new StreamReader(folderBrowserDialog.SelectedPath + "\\scale"))
                    this.scale = Convert.ToInt32(reader.ReadLine());

                this.DrawTiles(folderBrowserDialog.SelectedPath);
            }
        }
    }
}
