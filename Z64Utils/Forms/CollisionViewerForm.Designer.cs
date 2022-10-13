namespace Z64.Forms
{
    partial class CollisionViewerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CollisionViewerForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusErrorLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.modelViewer = new Z64.Forms.ModelViewerControl();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusErrorLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 616);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(883, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusErrorLabel
            // 
            this.toolStripStatusErrorLabel.ForeColor = System.Drawing.Color.Red;
            this.toolStripStatusErrorLabel.Name = "toolStripStatusErrorLabel";
            this.toolStripStatusErrorLabel.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusErrorLabel.Text = "toolStripStatusLabel1";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveScreenToolStripMenuItem,
            this.copyToClipboardToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(218, 48);
            // 
            // saveScreenToolStripMenuItem
            // 
            this.saveScreenToolStripMenuItem.Name = "saveScreenToolStripMenuItem";
            this.saveScreenToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.saveScreenToolStripMenuItem.Text = "Save Capture";
            this.saveScreenToolStripMenuItem.Click += new System.EventHandler(this.saveScreenToolStripMenuItem_Click);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.copyToClipboardToolStripMenuItem.Text = "Copy Capture To Clipboard";
            this.copyToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyToClipboardToolStripMenuItem_Click);
            // 
            // modelViewer
            // 
            this.modelViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modelViewer.BackColor = System.Drawing.Color.DodgerBlue;
            this.modelViewer.Location = new System.Drawing.Point(0, 3);
            this.modelViewer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.modelViewer.Name = "modelViewer";
            this.modelViewer.RenderCallback = null;
            this.modelViewer.Size = new System.Drawing.Size(871, 610);
            this.modelViewer.TabIndex = 4;
            this.modelViewer.VSync = false;
            this.modelViewer.MouseClick += new System.Windows.Forms.MouseEventHandler(this.modelViewer_MouseClick);
            // 
            // CollisionViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(883, 638);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.modelViewer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CollisionViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Collision Viewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ColViewerForm_FormClosed);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ModelViewerControl modelViewer;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusErrorLabel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveScreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}