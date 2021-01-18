
namespace DotMaps
{
    partial class MapDisplay
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mousePositionLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.latLngLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.memoryUsageBar = new System.Windows.Forms.ToolStripProgressBar();
            this.memoryUsageLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.openosmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveRenderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.slimToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.statusStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.statusLabel,
            this.mousePositionLabel,
            this.latLngLabel,
            this.memoryUsageBar,
            this.memoryUsageLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 428);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(800, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // statusLabel
            // 
            this.statusLabel.BackColor = System.Drawing.Color.White;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(49, 17);
            this.statusLabel.Text = "Loaded.";
            // 
            // mousePositionLabel
            // 
            this.mousePositionLabel.BackColor = System.Drawing.Color.White;
            this.mousePositionLabel.Name = "mousePositionLabel";
            this.mousePositionLabel.Size = new System.Drawing.Size(64, 17);
            this.mousePositionLabel.Text = "Mouse: 0,0";
            // 
            // latLngLabel
            // 
            this.latLngLabel.BackColor = System.Drawing.Color.Transparent;
            this.latLngLabel.Name = "latLngLabel";
            this.latLngLabel.Size = new System.Drawing.Size(118, 17);
            this.latLngLabel.Text = "| Lat: 0,000 Lon: 0,000";
            // 
            // memoryUsageBar
            // 
            this.memoryUsageBar.Name = "memoryUsageBar";
            this.memoryUsageBar.Size = new System.Drawing.Size(100, 16);
            // 
            // memoryUsageLabel
            // 
            this.memoryUsageLabel.BackColor = System.Drawing.Color.Transparent;
            this.memoryUsageLabel.Name = "memoryUsageLabel";
            this.memoryUsageLabel.Size = new System.Drawing.Size(24, 17);
            this.memoryUsageLabel.Text = "0/0";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openosmToolStripMenuItem,
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // openosmToolStripMenuItem
            // 
            this.openosmToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveRenderToolStripMenuItem,
            this.openToolStripMenuItem});
            this.openosmToolStripMenuItem.Name = "openosmToolStripMenuItem";
            this.openosmToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.openosmToolStripMenuItem.Text = "Map";
            // 
            // saveRenderToolStripMenuItem
            // 
            this.saveRenderToolStripMenuItem.Name = "saveRenderToolStripMenuItem";
            this.saveRenderToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.saveRenderToolStripMenuItem.Text = "Save render";
            this.saveRenderToolStripMenuItem.Click += new System.EventHandler(this.saveRenderToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slimToolStripMenuItem,
            this.splitToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(100, 20);
            this.toolStripMenuItem1.Text = ".osm Converter";
            // 
            // slimToolStripMenuItem
            // 
            this.slimToolStripMenuItem.Name = "slimToolStripMenuItem";
            this.slimToolStripMenuItem.Size = new System.Drawing.Size(97, 22);
            this.slimToolStripMenuItem.Text = "Slim";
            this.slimToolStripMenuItem.Click += new System.EventHandler(this.slimToolStripMenuItem_Click);
            // 
            // splitToolStripMenuItem
            // 
            this.splitToolStripMenuItem.Name = "splitToolStripMenuItem";
            this.splitToolStripMenuItem.Size = new System.Drawing.Size(97, 22);
            this.splitToolStripMenuItem.Text = "Split";
            this.splitToolStripMenuItem.Click += new System.EventHandler(this.splitToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.Location = new System.Drawing.Point(343, 204);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 50);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // MapDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MapDisplay";
            this.Text = "TileDisplay";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TileDisplay_FormClosed);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel mousePositionLabel;
        private System.Windows.Forms.ToolStripStatusLabel latLngLabel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem openosmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem slimToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem splitToolStripMenuItem;
        private System.Windows.Forms.ToolStripProgressBar memoryUsageBar;
        private System.Windows.Forms.ToolStripStatusLabel memoryUsageLabel;
        private System.Windows.Forms.ToolStripMenuItem saveRenderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

