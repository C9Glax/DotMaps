using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

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

        private void DrawTiles(string path)
        {
            this.SuspendLayout();
            string[] subdirs = Directory.GetDirectories(path); //x
            int xAmount = subdirs.Length;
            string[] files = Directory.GetFiles(subdirs[0]);
            int yAmount = files.Length;
            int tileSize = Image.FromFile(files[0]).Width;
            int totalImages = xAmount * yAmount;
            this.toolStripStatusLabel1.Text = "TileSize: " + tileSize + " xAmount: " + xAmount + " yAmount: " + yAmount;
            GroupBox map = new GroupBox();
            map.Size = new Size(xAmount * tileSize, yAmount * tileSize);
            map.Location = new Point(this.Width / 2 - (xAmount * tileSize) / 2, this.Height / 2 - (xAmount * tileSize) / 2);
            this.Controls.Add(map);
            for(int x = 0; x < xAmount; x++)
            {
                for(int y = 0; y < yAmount; y++)
                {
                    this.toolStripProgressBar1.Value = (x * yAmount + (y+1))*100/totalImages;
                    PictureBox tile = new PictureBox();
                    tile.Image = Image.FromFile(path + "\\" + x + "\\" + y + ".png");
                    tile.Location = new Point(x*tileSize, y*tileSize);
                    tile.Size = new Size(tileSize, tileSize);
                    tile.MouseMove += new MouseEventHandler(tile_MouseMove);
                    map.Controls.Add(tile);
                }
            }
            toolStripStatusLabel2.Text = "X:" + map.Left + ",Y:" + map.Top;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Point previousLocation;
        private void tile_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentLocation = new Point(Cursor.Position.X - this.Location.X, Cursor.Position.Y - this.Location.Y);
            toolStripStatusLabel3.Text = "Mouse:" + currentLocation.X + "," + currentLocation.Y;
            Cursor.Current = Cursors.Default;
            if (e.Button == MouseButtons.Left)
            {
                Cursor.Current = Cursors.Hand;
                ((PictureBox)sender).Parent.Left -= previousLocation.X - currentLocation.X;
                ((PictureBox)sender).Parent.Top -= previousLocation.Y - currentLocation.Y;
                toolStripStatusLabel2.Text = "X:" + ((PictureBox)sender).Parent.Left + ",Y:" + ((PictureBox)sender).Parent.Top;
            }
            previousLocation = currentLocation;
            this.Invalidate();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.DrawTiles(folderBrowserDialog.SelectedPath);
            }
        }
    }
}
