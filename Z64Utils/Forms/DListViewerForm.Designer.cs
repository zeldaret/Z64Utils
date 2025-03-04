namespace Z64.Forms
{
    partial class DListViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DListViewerForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripRenderCfgBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripDisasBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSegmentsBtn = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusErrorLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.renderContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.modelViewer = new Z64.Forms.ModelViewerControl();
            this.listBox_routines = new System.Windows.Forms.ListBox();
            this.routineContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AddRoutineMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditRoutineMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemoveRoutineMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.renderContextMenuStrip.SuspendLayout();
            this.routineContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripRenderCfgBtn,
            this.toolStripDisasBtn,
            this.toolStripSeparator1,
            this.toolStripSegmentsBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(968, 25);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripRenderCfgBtn
            // 
            this.toolStripRenderCfgBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRenderCfgBtn.Name = "toolStripRenderCfgBtn";
            this.toolStripRenderCfgBtn.Size = new System.Drawing.Size(93, 22);
            this.toolStripRenderCfgBtn.Text = "Render Settings";
            this.toolStripRenderCfgBtn.Click += new System.EventHandler(this.toolStripRenderCfgBtn_Click);
            // 
            // toolStripDisasBtn
            // 
            this.toolStripDisasBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDisasBtn.Name = "toolStripDisasBtn";
            this.toolStripDisasBtn.Size = new System.Drawing.Size(76, 22);
            this.toolStripDisasBtn.Text = "Disassembly";
            this.toolStripDisasBtn.Click += new System.EventHandler(this.toolStripDisasBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSegmentsBtn
            // 
            this.toolStripSegmentsBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSegmentsBtn.Name = "toolStripSegmentsBtn";
            this.toolStripSegmentsBtn.Size = new System.Drawing.Size(63, 22);
            this.toolStripSegmentsBtn.Text = "Segments";
            this.toolStripSegmentsBtn.Click += new System.EventHandler(this.toolStripSegmentsBtn_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusErrorLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 616);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(968, 22);
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
            // renderContextMenuStrip
            // 
            this.renderContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveScreenToolStripMenuItem,
            this.copyToClipboardToolStripMenuItem});
            this.renderContextMenuStrip.Name = "contextMenuStrip1";
            this.renderContextMenuStrip.Size = new System.Drawing.Size(218, 48);
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
            this.modelViewer.Location = new System.Drawing.Point(1, 28);
            this.modelViewer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.modelViewer.Name = "modelViewer";
            this.modelViewer.RenderCallback = null;
            this.modelViewer.Size = new System.Drawing.Size(757, 585);
            this.modelViewer.TabIndex = 4;
            this.modelViewer.MouseClick += new System.Windows.Forms.MouseEventHandler(this.modelViewer_MouseClick);
            // 
            // listBox_routines
            // 
            this.listBox_routines.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox_routines.ContextMenuStrip = this.routineContextMenuStrip;
            this.listBox_routines.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listBox_routines.FormattingEnabled = true;
            this.listBox_routines.Location = new System.Drawing.Point(765, 28);
            this.listBox_routines.Name = "listBox_routines";
            this.listBox_routines.Size = new System.Drawing.Size(196, 576);
            this.listBox_routines.TabIndex = 10;
            this.listBox_routines.SelectedIndexChanged += new System.EventHandler(this.listBox_routines_SelectedIndexChanged);
            // 
            // routineContextMenuStrip
            // 
            this.routineContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddRoutineMenuItem,
            this.EditRoutineMenuItem,
            this.RemoveRoutineMenuItem});
            this.routineContextMenuStrip.Name = "routineContextMenuStrip";
            this.routineContextMenuStrip.Size = new System.Drawing.Size(118, 70);
            // 
            // AddRoutineMenuItem
            // 
            this.AddRoutineMenuItem.Name = "AddRoutineMenuItem";
            this.AddRoutineMenuItem.Size = new System.Drawing.Size(117, 22);
            this.AddRoutineMenuItem.Text = "Add";
            this.AddRoutineMenuItem.Click += new System.EventHandler(this.AddRoutineMenuItem_Click);
            // 
            // EditRoutineMenuItem
            // 
            this.EditRoutineMenuItem.Name = "EditRoutineMenuItem";
            this.EditRoutineMenuItem.Size = new System.Drawing.Size(117, 22);
            this.EditRoutineMenuItem.Text = "Edit";
            this.EditRoutineMenuItem.Click += new System.EventHandler(this.EditRoutineMenuItem_Click);
            // 
            // RemoveRoutineMenuItem
            // 
            this.RemoveRoutineMenuItem.Name = "RemoveRoutineMenuItem";
            this.RemoveRoutineMenuItem.Size = new System.Drawing.Size(117, 22);
            this.RemoveRoutineMenuItem.Text = "Remove";
            this.RemoveRoutineMenuItem.Click += new System.EventHandler(this.RemoveRoutineMenuItem_Click);
            // 
            // DListViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(968, 638);
            this.Controls.Add(this.modelViewer);
            this.Controls.Add(this.listBox_routines);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DListViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Display List Viewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DListViewerForm_FormClosed);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.renderContextMenuStrip.ResumeLayout(false);
            this.routineContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ModelViewerControl modelViewer;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusErrorLabel;
        private System.Windows.Forms.ToolStripButton toolStripRenderCfgBtn;
        private System.Windows.Forms.ContextMenuStrip renderContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem saveScreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ListBox listBox_routines;
        private System.Windows.Forms.ToolStripButton toolStripDisasBtn;
        private System.Windows.Forms.ToolStripButton toolStripSegmentsBtn;
        private System.Windows.Forms.ContextMenuStrip routineContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem AddRoutineMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RemoveRoutineMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EditRoutineMenuItem;
    }
}