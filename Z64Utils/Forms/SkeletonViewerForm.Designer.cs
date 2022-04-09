
namespace Z64.Forms
{
    partial class SkeletonViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkeletonViewerForm));
            this.modelViewer = new Z64.Forms.ModelViewerControl();
            this.treeView_hierarchy = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripRenderCfgBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripDisassemblyBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSegmentsBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripErrorLabel = new System.Windows.Forms.ToolStripLabel();
            this.listBox_anims = new System.Windows.Forms.ListBox();
            this.trackBar_anim = new System.Windows.Forms.TrackBar();
            this.label_anim = new System.Windows.Forms.Label();
            this.button_playAnim = new System.Windows.Forms.Button();
            this.button_playbackAnim = new System.Windows.Forms.Button();
            this.numUpDown_playbackSpeed = new System.Windows.Forms.NumericUpDown();
            this.label_playbackSpeed = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_anim)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDown_playbackSpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // modelViewer
            // 
            this.modelViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modelViewer.BackColor = System.Drawing.Color.DodgerBlue;
            this.modelViewer.Location = new System.Drawing.Point(7, 28);
            this.modelViewer.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.modelViewer.Name = "modelViewer";
            this.modelViewer.RenderCallback = null;
            this.modelViewer.Size = new System.Drawing.Size(506, 508);
            this.modelViewer.TabIndex = 0;
            this.modelViewer.VSync = true;
            // 
            // treeView_hierarchy
            // 
            this.treeView_hierarchy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView_hierarchy.HideSelection = false;
            this.treeView_hierarchy.Location = new System.Drawing.Point(520, 40);
            this.treeView_hierarchy.Name = "treeView_hierarchy";
            this.treeView_hierarchy.Size = new System.Drawing.Size(176, 264);
            this.treeView_hierarchy.TabIndex = 1;
            this.treeView_hierarchy.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_hierarchy_AfterSelect);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(521, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Hierarchy";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(521, 309);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Animations";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripRenderCfgBtn,
            this.toolStripDisassemblyBtn,
            this.toolStripSeparator1,
            this.toolStripSegmentsBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(706, 25);
            this.toolStrip1.TabIndex = 9;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripRenderCfgBtn
            // 
            this.toolStripRenderCfgBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRenderCfgBtn.Name = "toolStripRenderCfgBtn";
            this.toolStripRenderCfgBtn.Size = new System.Drawing.Size(93, 22);
            this.toolStripRenderCfgBtn.Text = "Render Settings";
            this.toolStripRenderCfgBtn.Click += new System.EventHandler(this.ToolStripRenderCfgBtn_Click);
            // 
            // toolStripDisassemblyBtn
            // 
            this.toolStripDisassemblyBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDisassemblyBtn.Name = "toolStripDisassemblyBtn";
            this.toolStripDisassemblyBtn.Size = new System.Drawing.Size(76, 22);
            this.toolStripDisassemblyBtn.Text = "Disassembly";
            this.toolStripDisassemblyBtn.Click += new System.EventHandler(this.ToolStripDisassemblyBtn_Click);
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
            this.toolStripSegmentsBtn.Click += new System.EventHandler(this.ToolStripSegmentsBtn_Click);
            // 
            // toolStrip2
            // 
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripErrorLabel});
            this.toolStrip2.Location = new System.Drawing.Point(0, 592);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(706, 25);
            this.toolStrip2.TabIndex = 10;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripErrorLabel
            // 
            this.toolStripErrorLabel.ForeColor = System.Drawing.Color.Red;
            this.toolStripErrorLabel.Name = "toolStripErrorLabel";
            this.toolStripErrorLabel.Size = new System.Drawing.Size(86, 22);
            this.toolStripErrorLabel.Text = "toolStripLabel1";
            // 
            // listBox_anims
            // 
            this.listBox_anims.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox_anims.FormattingEnabled = true;
            this.listBox_anims.Location = new System.Drawing.Point(522, 325);
            this.listBox_anims.Name = "listBox_anims";
            this.listBox_anims.Size = new System.Drawing.Size(172, 212);
            this.listBox_anims.TabIndex = 11;
            this.listBox_anims.SelectedIndexChanged += new System.EventHandler(this.listBox_anims_SelectedIndexChanged);
            // 
            // trackBar_anim
            // 
            this.trackBar_anim.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar_anim.Enabled = false;
            this.trackBar_anim.Location = new System.Drawing.Point(86, 557);
            this.trackBar_anim.Name = "trackBar_anim";
            this.trackBar_anim.Size = new System.Drawing.Size(493, 45);
            this.trackBar_anim.TabIndex = 12;
            this.trackBar_anim.ValueChanged += new System.EventHandler(this.trackBar_anim_ValueChanged);
            // 
            // label_anim
            // 
            this.label_anim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_anim.AutoSize = true;
            this.label_anim.Location = new System.Drawing.Point(91, 539);
            this.label_anim.Name = "label_anim";
            this.label_anim.Size = new System.Drawing.Size(24, 13);
            this.label_anim.TabIndex = 13;
            this.label_anim.Text = "0/0";
            // 
            // button_playAnim
            // 
            this.button_playAnim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_playAnim.BackgroundImage = global::Z64.Properties.Resources.play_icon;
            this.button_playAnim.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_playAnim.Enabled = false;
            this.button_playAnim.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.button_playAnim.Location = new System.Drawing.Point(44, 553);
            this.button_playAnim.Name = "button_playAnim";
            this.button_playAnim.Size = new System.Drawing.Size(36, 36);
            this.button_playAnim.TabIndex = 14;
            this.button_playAnim.UseVisualStyleBackColor = true;
            this.button_playAnim.Click += new System.EventHandler(this.button_playAnim_Click);
            // 
            // button_playbackAnim
            // 
            this.button_playbackAnim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_playbackAnim.BackgroundImage = global::Z64.Properties.Resources.playback_icon;
            this.button_playbackAnim.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_playbackAnim.Enabled = false;
            this.button_playbackAnim.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.button_playbackAnim.Location = new System.Drawing.Point(4, 553);
            this.button_playbackAnim.Name = "button_playbackAnim";
            this.button_playbackAnim.Size = new System.Drawing.Size(36, 36);
            this.button_playbackAnim.TabIndex = 15;
            this.button_playbackAnim.UseVisualStyleBackColor = true;
            this.button_playbackAnim.Click += new System.EventHandler(this.button_playbackAnim_Click);
            // 
            // numUpDown_playbackSpeed
            // 
            this.numUpDown_playbackSpeed.Location = new System.Drawing.Point(585, 566);
            this.numUpDown_playbackSpeed.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numUpDown_playbackSpeed.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numUpDown_playbackSpeed.Name = "numUpDown_playbackSpeed";
            this.numUpDown_playbackSpeed.Size = new System.Drawing.Size(111, 20);
            this.numUpDown_playbackSpeed.TabIndex = 16;
            this.numUpDown_playbackSpeed.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numUpDown_playbackSpeed.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label_playbackSpeed
            // 
            this.label_playbackSpeed.AutoSize = true;
            this.label_playbackSpeed.Location = new System.Drawing.Point(585, 550);
            this.label_playbackSpeed.Name = "label_playbackSpeed";
            this.label_playbackSpeed.Size = new System.Drawing.Size(86, 13);
            this.label_playbackSpeed.TabIndex = 17;
            this.label_playbackSpeed.Text = "Playback speed:";
            // 
            // SkeletonViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(706, 617);
            this.Controls.Add(this.label_playbackSpeed);
            this.Controls.Add(this.numUpDown_playbackSpeed);
            this.Controls.Add(this.button_playbackAnim);
            this.Controls.Add(this.button_playAnim);
            this.Controls.Add(this.label_anim);
            this.Controls.Add(this.listBox_anims);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeView_hierarchy);
            this.Controls.Add(this.modelViewer);
            this.Controls.Add(this.trackBar_anim);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SkeletonViewerForm";
            this.Text = "Skeleton Viewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SkeletonViewerForm_FormClosed);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_anim)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDown_playbackSpeed)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ModelViewerControl modelViewer;
        private System.Windows.Forms.TreeView treeView_hierarchy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripRenderCfgBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripLabel toolStripErrorLabel;
        private System.Windows.Forms.ToolStripButton toolStripDisassemblyBtn;
        private System.Windows.Forms.ToolStripButton toolStripSegmentsBtn;
        private System.Windows.Forms.ListBox listBox_anims;
        private System.Windows.Forms.TrackBar trackBar_anim;
        private System.Windows.Forms.Label label_anim;
        private System.Windows.Forms.Button button_playAnim;
        private System.Windows.Forms.Button button_playbackAnim;
        private System.Windows.Forms.NumericUpDown numUpDown_playbackSpeed;
        private System.Windows.Forms.Label label_playbackSpeed;
    }
}