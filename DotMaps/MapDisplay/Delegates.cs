using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotMaps
{
    partial class MapDisplay
    {
        private delegate void ChangeMemoryUsageDelegate(long used);
        private void ChangeMemoryUsageSafe(long used)
        {
            if (this.memoryUsageBar.Control.InvokeRequired)
                this.memoryUsageBar.Control.Invoke(new ChangeMemoryUsageDelegate(this.ChangeMemoryUsageSafe), used);
            else
                this.memoryUsageBar.Value = (int)((100.0 / totalMemory) * (used / 1024));

            if (this.InvokeRequired)
                this.Invoke(new ChangeMemoryUsageDelegate(this.ChangeMemoryUsageSafe), used);
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
    }
}
