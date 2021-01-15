using System;
using System.Windows.Forms;
using DotMaps.Datastructures;

namespace DotMaps
{
    public partial class TileDisplay : Form
    {
        public TileDisplay()
        {
            InitializeComponent();
            this.scale = scaleSlider.Value;
        }

        private int scale;
        private Graph mapGraph;
        private _3DNode renderCenter;

        private void openosmToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                this.mapGraph = Utils.Importer.ImportOSM(fd.FileName);
                this.renderCenter = new _3DNode(this.mapGraph.minLat + (this.mapGraph.maxLat - this.mapGraph.minLat) / 2, this.mapGraph.minLon + (this.mapGraph.maxLon - this.mapGraph.minLon) / 2);
            }
        }

        private void DrawMap()
        {
            
        }

        private void scaleSlider_ValueChanged(object sender, EventArgs e)
        {
            this.scale = scaleSlider.Value;
            //Draw

        }
    }
}
