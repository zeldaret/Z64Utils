namespace Z64.Forms
{
    partial class ObjectAnalyzerForm
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "06000000",
            "-32767, -32767, -32767",
            "0xFFFF",
            "-32767, -32767",
            "255, 255, 255, 255"}, -1);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "00000000",
            "cube_vtx",
            "Vertex"}, -1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectAnalyzerForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_empty = new System.Windows.Forms.TabPage();
            this.tabPage_text = new System.Windows.Forms.TabPage();
            this.textBox_holderInfo = new System.Windows.Forms.TextBox();
            this.tabPage_texture = new System.Windows.Forms.TabPage();
            this.label_textureInfo = new System.Windows.Forms.Label();
            this.pic_texture = new Z64.Forms.TextureBox();
            this.tabPage_vtx = new System.Windows.Forms.TabPage();
            this.listView_vtx = new System.Windows.Forms.ListView();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
            this.tabPage_unknow = new System.Windows.Forms.TabPage();
            this.hexBox1 = new Be.Windows.Forms.HexBox();
            this.listView_map = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openInDlistViewerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToDlistViewerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSkeletonViewerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findDlistsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analyzeDlistsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disassemblySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage_text.SuspendLayout();
            this.tabPage_texture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic_texture)).BeginInit();
            this.tabPage_vtx.SuspendLayout();
            this.tabPage_unknow.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_empty);
            this.tabControl1.Controls.Add(this.tabPage_text);
            this.tabControl1.Controls.Add(this.tabPage_texture);
            this.tabControl1.Controls.Add(this.tabPage_vtx);
            this.tabControl1.Controls.Add(this.tabPage_unknow);
            this.tabControl1.ItemSize = new System.Drawing.Size(47, 18);
            this.tabControl1.Location = new System.Drawing.Point(377, 27);
            this.tabControl1.MinimumSize = new System.Drawing.Size(357, 207);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(632, 499);
            this.tabControl1.TabIndex = 8;
            // 
            // tabPage_empty
            // 
            this.tabPage_empty.Location = new System.Drawing.Point(4, 22);
            this.tabPage_empty.Name = "tabPage_empty";
            this.tabPage_empty.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_empty.Size = new System.Drawing.Size(624, 473);
            this.tabPage_empty.TabIndex = 0;
            this.tabPage_empty.Text = "nothing";
            this.tabPage_empty.UseVisualStyleBackColor = true;
            // 
            // tabPage_text
            // 
            this.tabPage_text.Controls.Add(this.textBox_holderInfo);
            this.tabPage_text.Location = new System.Drawing.Point(4, 22);
            this.tabPage_text.Name = "tabPage_text";
            this.tabPage_text.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_text.Size = new System.Drawing.Size(624, 473);
            this.tabPage_text.TabIndex = 1;
            this.tabPage_text.Text = "ucode";
            this.tabPage_text.UseVisualStyleBackColor = true;
            // 
            // textBox_holderInfo
            // 
            this.textBox_holderInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_holderInfo.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBox_holderInfo.Location = new System.Drawing.Point(6, 6);
            this.textBox_holderInfo.Multiline = true;
            this.textBox_holderInfo.Name = "textBox_holderInfo";
            this.textBox_holderInfo.ReadOnly = true;
            this.textBox_holderInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_holderInfo.Size = new System.Drawing.Size(617, 461);
            this.textBox_holderInfo.TabIndex = 7;
            this.textBox_holderInfo.WordWrap = false;
            // 
            // tabPage_texture
            // 
            this.tabPage_texture.Controls.Add(this.label_textureInfo);
            this.tabPage_texture.Controls.Add(this.pic_texture);
            this.tabPage_texture.Location = new System.Drawing.Point(4, 22);
            this.tabPage_texture.Name = "tabPage_texture";
            this.tabPage_texture.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_texture.Size = new System.Drawing.Size(624, 473);
            this.tabPage_texture.TabIndex = 2;
            this.tabPage_texture.Text = "texture";
            this.tabPage_texture.UseVisualStyleBackColor = true;
            // 
            // label_textureInfo
            // 
            this.label_textureInfo.AutoSize = true;
            this.label_textureInfo.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label_textureInfo.Location = new System.Drawing.Point(6, 3);
            this.label_textureInfo.Name = "label_textureInfo";
            this.label_textureInfo.Size = new System.Drawing.Size(25, 13);
            this.label_textureInfo.TabIndex = 1;
            this.label_textureInfo.Text = "...";
            // 
            // pic_texture
            // 
            this.pic_texture.AlphaTileSize = 10;
            this.pic_texture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pic_texture.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.pic_texture.Location = new System.Drawing.Point(6, 20);
            this.pic_texture.Name = "pic_texture";
            this.pic_texture.Size = new System.Drawing.Size(617, 447);
            this.pic_texture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pic_texture.TabIndex = 0;
            this.pic_texture.TabStop = false;
            // 
            // tabPage_vtx
            // 
            this.tabPage_vtx.Controls.Add(this.listView_vtx);
            this.tabPage_vtx.Location = new System.Drawing.Point(4, 22);
            this.tabPage_vtx.Name = "tabPage_vtx";
            this.tabPage_vtx.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_vtx.Size = new System.Drawing.Size(624, 473);
            this.tabPage_vtx.TabIndex = 3;
            this.tabPage_vtx.Text = "vertex";
            this.tabPage_vtx.UseVisualStyleBackColor = true;
            // 
            // listView_vtx
            // 
            this.listView_vtx.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_vtx.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listView_vtx.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listView_vtx.FullRowSelect = true;
            this.listView_vtx.HideSelection = false;
            this.listView_vtx.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listView_vtx.Location = new System.Drawing.Point(6, 6);
            this.listView_vtx.Name = "listView_vtx";
            this.listView_vtx.Size = new System.Drawing.Size(619, 461);
            this.listView_vtx.TabIndex = 0;
            this.listView_vtx.UseCompatibleStateImageBehavior = false;
            this.listView_vtx.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Address";
            this.columnHeader4.Width = 78;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Coordinates";
            this.columnHeader5.Width = 158;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Flag";
            this.columnHeader6.Width = 87;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Tex Coordinates";
            this.columnHeader7.Width = 127;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Normal/Color";
            this.columnHeader8.Width = 149;
            // 
            // tabPage_unknow
            // 
            this.tabPage_unknow.Controls.Add(this.hexBox1);
            this.tabPage_unknow.Location = new System.Drawing.Point(4, 22);
            this.tabPage_unknow.Name = "tabPage_unknow";
            this.tabPage_unknow.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_unknow.Size = new System.Drawing.Size(624, 473);
            this.tabPage_unknow.TabIndex = 4;
            this.tabPage_unknow.Text = "unknow";
            this.tabPage_unknow.UseVisualStyleBackColor = true;
            // 
            // hexBox1
            // 
            this.hexBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hexBox1.ColumnInfoVisible = true;
            this.hexBox1.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.hexBox1.LineInfoVisible = true;
            this.hexBox1.Location = new System.Drawing.Point(6, 6);
            this.hexBox1.Name = "hexBox1";
            this.hexBox1.ReadOnly = true;
            this.hexBox1.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hexBox1.Size = new System.Drawing.Size(622, 461);
            this.hexBox1.StringViewVisible = true;
            this.hexBox1.TabIndex = 1;
            this.hexBox1.UseFixedBytesPerLine = true;
            this.hexBox1.VScrollBarVisible = true;
            // 
            // listView_map
            // 
            this.listView_map.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listView_map.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView_map.ContextMenuStrip = this.contextMenuStrip1;
            this.listView_map.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listView_map.FullRowSelect = true;
            this.listView_map.HideSelection = false;
            this.listView_map.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem2});
            this.listView_map.Location = new System.Drawing.Point(12, 49);
            this.listView_map.MultiSelect = false;
            this.listView_map.Name = "listView_map";
            this.listView_map.Size = new System.Drawing.Size(359, 477);
            this.listView_map.TabIndex = 7;
            this.listView_map.UseCompatibleStateImageBehavior = false;
            this.listView_map.View = System.Windows.Forms.View.Details;
            this.listView_map.SelectedIndexChanged += new System.EventHandler(this.listView_map_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Offset";
            this.columnHeader1.Width = 59;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 151;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Type";
            this.columnHeader3.Width = 124;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openInDlistViewerMenuItem,
            this.addToDlistViewerMenuItem,
            this.openSkeletonViewerMenuItem,
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(203, 114);
            // 
            // openInDlistViewerMenuItem
            // 
            this.openInDlistViewerMenuItem.Name = "openInDlistViewerMenuItem";
            this.openInDlistViewerMenuItem.Size = new System.Drawing.Size(202, 22);
            this.openInDlistViewerMenuItem.Text = "Open In Dlist Viewer";
            this.openInDlistViewerMenuItem.Click += new System.EventHandler(this.openInDisplayViewerMenuItem_Click);
            // 
            // addToDlistViewerMenuItem
            // 
            this.addToDlistViewerMenuItem.Name = "addToDlistViewerMenuItem";
            this.addToDlistViewerMenuItem.Size = new System.Drawing.Size(202, 22);
            this.addToDlistViewerMenuItem.Text = "Add to Dlist Viewer";
            this.addToDlistViewerMenuItem.Click += new System.EventHandler(this.addToDisplayViewerMenuItem_Click);
            // 
            // openSkeletonViewerMenuItem
            // 
            this.openSkeletonViewerMenuItem.Name = "openSkeletonViewerMenuItem";
            this.openSkeletonViewerMenuItem.Size = new System.Drawing.Size(202, 22);
            this.openSkeletonViewerMenuItem.Text = "Open in Skeleton Viewer";
            this.openSkeletonViewerMenuItem.Click += new System.EventHandler(this.openSkeletonViewerMenuItem_Click);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1009, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportCToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.Visible = false;
            // 
            // exportCToolStripMenuItem
            // 
            this.exportCToolStripMenuItem.Name = "exportCToolStripMenuItem";
            this.exportCToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.exportCToolStripMenuItem.Text = "Export C";
            this.exportCToolStripMenuItem.Click += new System.EventHandler(this.exportCToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findDlistsToolStripMenuItem,
            this.analyzeDlistsToolStripMenuItem,
            this.importJSONToolStripMenuItem,
            this.exportJSONToolStripMenuItem,
            this.resetToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.toolsToolStripMenuItem.Text = "Analysis";
            // 
            // findDlistsToolStripMenuItem
            // 
            this.findDlistsToolStripMenuItem.Name = "findDlistsToolStripMenuItem";
            this.findDlistsToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+F";
            this.findDlistsToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.findDlistsToolStripMenuItem.Text = "Find Dlists";
            this.findDlistsToolStripMenuItem.Click += new System.EventHandler(this.findDlistsToolStripMenuItem_Click);
            // 
            // analyzeDlistsToolStripMenuItem
            // 
            this.analyzeDlistsToolStripMenuItem.Name = "analyzeDlistsToolStripMenuItem";
            this.analyzeDlistsToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+A";
            this.analyzeDlistsToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.analyzeDlistsToolStripMenuItem.Text = "Analyze Dlists";
            this.analyzeDlistsToolStripMenuItem.Click += new System.EventHandler(this.analyzeDlistsToolStripMenuItem_Click);
            // 
            // importJSONToolStripMenuItem
            // 
            this.importJSONToolStripMenuItem.Name = "importJSONToolStripMenuItem";
            this.importJSONToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.importJSONToolStripMenuItem.Text = "Import JSON";
            this.importJSONToolStripMenuItem.Click += new System.EventHandler(this.importJSONToolStripMenuItem_Click);
            // 
            // exportJSONToolStripMenuItem
            // 
            this.exportJSONToolStripMenuItem.Name = "exportJSONToolStripMenuItem";
            this.exportJSONToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.exportJSONToolStripMenuItem.Text = "Export JSON";
            this.exportJSONToolStripMenuItem.Click += new System.EventHandler(this.exportJSONToolStripMenuItem_Click);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disassemblySettingsToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // disassemblySettingsToolStripMenuItem
            // 
            this.disassemblySettingsToolStripMenuItem.Name = "disassemblySettingsToolStripMenuItem";
            this.disassemblySettingsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.disassemblySettingsToolStripMenuItem.Text = "Disassembly Settings";
            this.disassemblySettingsToolStripMenuItem.Click += new System.EventHandler(this.disassemblySettingsToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(50, 27);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(321, 20);
            this.textBox1.TabIndex = 10;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Filter:";
            // 
            // ObjectAnalyzerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1009, 528);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.listView_map);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ObjectAnalyzerForm";
            this.Text = "Object Analyzer";
            this.tabControl1.ResumeLayout(false);
            this.tabPage_text.ResumeLayout(false);
            this.tabPage_text.PerformLayout();
            this.tabPage_texture.ResumeLayout(false);
            this.tabPage_texture.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic_texture)).EndInit();
            this.tabPage_vtx.ResumeLayout(false);
            this.tabPage_unknow.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_empty;
        private System.Windows.Forms.TabPage tabPage_texture;
        private TextureBox pic_texture;
        private System.Windows.Forms.TabPage tabPage_vtx;
        private System.Windows.Forms.TabPage tabPage_unknow;
        private System.Windows.Forms.ListView listView_map;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findDlistsToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox_holderInfo;
        private System.Windows.Forms.ToolStripMenuItem analyzeDlistsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disassemblySettingsToolStripMenuItem;
        private Be.Windows.Forms.HexBox hexBox1;
        private System.Windows.Forms.ListView listView_vtx;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ToolStripMenuItem importJSONToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportJSONToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportCToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage_text;
        private System.Windows.Forms.ToolStripMenuItem openInDlistViewerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToDlistViewerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSkeletonViewerMenuItem;
        private System.Windows.Forms.Label label_textureInfo;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
    }
}