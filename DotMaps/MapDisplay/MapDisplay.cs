using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using DotMaps.Datastructures;

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
        private int coreCount, scale;
        private Thread memoryUsageThread;
        private const int scalerate = 50, minScale = 50, maxScale = 250;
        private Point mapPrevious2DLocation, previousMouseLocation;
        private Graph mapGraph;
        private _3DNode renderCenter;
        private Hashtable pens;
        private Utils.Renderer renderer;

        public MapDisplay()
        {
            InitializeComponent();
            this.pictureBox1.Location = new Point(-this.Width, -this.Height);
            this.pictureBox1.Size = new Size(this.Width * 3, this.Height * 3);
            this.pictureBox1.SendToBack();

            this.scale = 100;
            this.mapPrevious2DLocation = new Point(Cursor.Position.X - this.Location.X - 8, Cursor.Position.Y - this.Location.Y - 32);

            foreach (ManagementObject result in new ManagementObjectSearcher(new ObjectQuery("SELECT * FROM Win32_OperatingSystem")).Get())
                this.totalMemory = Convert.ToInt64(result["TotalVisibleMemorySize"]);

            foreach (ManagementObject result in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                this.coreCount += int.Parse(result["NumberOfCores"].ToString());

            this.memoryUsageThread = new Thread(MemoryUsageThread);
            this.memoryUsageThread.Start();
        }

        private void MemoryUsageThread()
        {
            while (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.AbortRequested)
            {
                this.ChangeMemoryUsageSafe(Process.GetCurrentProcess().PrivateMemorySize64);
                Thread.Sleep(1000);
            }
        }

        private bool allowMovement = false, isRendering = false;

        private void SetTitle(string title)
        {
            this.Text = "MapDisplay - " + title;
        }
    }
}
