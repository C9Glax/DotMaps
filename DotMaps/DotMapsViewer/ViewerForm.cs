using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DotMaps.Utils;

namespace DotMaps
{
    public partial class ViewerForm : Form
    {
        private Status status;
        private Thread statusThread;

        public ViewerForm()
        {
            this.status = new Status();
            InitializeComponent();
            this.statusThread = new Thread(threadStatus);
            this.statusThread.Start();
        }

        private void threadStatus()
        {
            while (true)
            {
                this.statusLabel.Text = this.status.currentStatus;
                Thread.Sleep(10);
            }
        }

        private void convertosmToSlimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "OpenStreetMaps files (*.osm)|*.osm";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Thread threadSlimmer = new Thread(this.slimmerThread);
                    threadSlimmer.Start(openFileDialog.FileName);
                }
            }
        }
        private void slimmerThread(object path)
        {
            new Slimmer((string)path, this.status);
        }
    }
}
