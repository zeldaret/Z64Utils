using System;

namespace Z64.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label_loadProgress = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_fileFilter = new System.Windows.Forms.TextBox();
            this.listView_files = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStrip_fs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openObjectViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.injectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.romToolStripButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.romOpenItem = new System.Windows.Forms.ToolStripMenuItem();
            this.romExportFsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.romSaveItem = new System.Windows.Forms.ToolStripMenuItem();
            this.romImportNamesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.romExportNamesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.openObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.f3DEXDisassemblerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ROMRAMConversionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textureViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.checkNewReleasesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.objectAnalyzerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.contextMenuStrip_fs.SuspendLayout();
            this.mainToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(4, 484);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(568, 19);
            this.progressBar1.TabIndex = 3;
            // 
            // label_loadProgress
            // 
            this.label_loadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_loadProgress.AutoSize = true;
            this.label_loadProgress.Location = new System.Drawing.Point(4, 471);
            this.label_loadProgress.Name = "label_loadProgress";
            this.label_loadProgress.Size = new System.Drawing.Size(16, 13);
            this.label_loadProgress.TabIndex = 4;
            this.label_loadProgress.Text = "...";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Location = new System.Drawing.Point(7, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(560, 440);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.label3);
            this.tabPage5.Controls.Add(this.textBox_fileFilter);
            this.tabPage5.Controls.Add(this.listView_files);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(552, 414);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "FS";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Filter:";
            // 
            // textBox_fileFilter
            // 
            this.textBox_fileFilter.Location = new System.Drawing.Point(6, 27);
            this.textBox_fileFilter.Name = "textBox_fileFilter";
            this.textBox_fileFilter.Size = new System.Drawing.Size(540, 20);
            this.textBox_fileFilter.TabIndex = 2;
            this.textBox_fileFilter.TextChanged += new System.EventHandler(this.TextBox_fileFilter_TextChanged);
            // 
            // listView_files
            // 
            this.listView_files.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView_files.ContextMenuStrip = this.contextMenuStrip_fs;
            this.listView_files.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listView_files.FullRowSelect = true;
            this.listView_files.HideSelection = false;
            this.listView_files.Location = new System.Drawing.Point(6, 53);
            this.listView_files.MultiSelect = false;
            this.listView_files.Name = "listView_files";
            this.listView_files.Size = new System.Drawing.Size(540, 357);
            this.listView_files.TabIndex = 1;
            this.listView_files.UseCompatibleStateImageBehavior = false;
            this.listView_files.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Name = "columnHeader1";
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 229;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Name = "columnHeader2";
            this.columnHeader2.Text = "VROM";
            this.columnHeader2.Width = 115;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Name = "columnHeader3";
            this.columnHeader3.Text = "ROM";
            this.columnHeader3.Width = 115;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Name = "columnHeader4";
            this.columnHeader4.Text = "Type";
            // 
            // contextMenuStrip_fs
            // 
            this.contextMenuStrip_fs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openObjectViewerToolStripMenuItem,
            this.injectToolStripMenuItem,
            this.saveToolStripMenuItem1,
            this.renameToolStripMenuItem});
            this.contextMenuStrip_fs.Name = "contextMenuStrip_fs";
            this.contextMenuStrip_fs.Size = new System.Drawing.Size(203, 92);
            // 
            // openObjectViewerToolStripMenuItem
            // 
            this.openObjectViewerToolStripMenuItem.Name = "openObjectViewerToolStripMenuItem";
            this.openObjectViewerToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.openObjectViewerToolStripMenuItem.Text = "Open in Object Analyzer";
            this.openObjectViewerToolStripMenuItem.Click += new System.EventHandler(this.OpenObjectViewerToolStripMenuItem_Click);
            // 
            // injectToolStripMenuItem
            // 
            this.injectToolStripMenuItem.Name = "injectToolStripMenuItem";
            this.injectToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.injectToolStripMenuItem.Text = "Inject";
            this.injectToolStripMenuItem.Click += new System.EventHandler(this.InjectToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem1
            // 
            this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            this.saveToolStripMenuItem1.Size = new System.Drawing.Size(202, 22);
            this.saveToolStripMenuItem1.Text = "Save";
            this.saveToolStripMenuItem1.Click += new System.EventHandler(this.SaveToolStripMenuItem1_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.romToolStripButton,
            this.toolsToolStripButton,
            this.helpToolStripButton});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 0);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Size = new System.Drawing.Size(576, 25);
            this.mainToolStrip.TabIndex = 8;
            this.mainToolStrip.Text = "mainToolStrip";
            // 
            // romToolStripButton
            // 
            this.romToolStripButton.AutoToolTip = false;
            this.romToolStripButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.romOpenItem,
            this.romExportFsItem,
            this.romSaveItem,
            this.romImportNamesItem,
            this.romExportNamesItem});
            this.romToolStripButton.Image = global::Z64.Properties.Resources.file;
            this.romToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.romToolStripButton.Name = "romToolStripButton";
            this.romToolStripButton.Size = new System.Drawing.Size(63, 22);
            this.romToolStripButton.Text = "ROM";
            // 
            // romOpenItem
            // 
            this.romOpenItem.Image = global::Z64.Properties.Resources.open;
            this.romOpenItem.Name = "romOpenItem";
            this.romOpenItem.Size = new System.Drawing.Size(187, 22);
            this.romOpenItem.Text = "Open";
            this.romOpenItem.Click += new System.EventHandler(this.RomOpenItem_Click);
            // 
            // romExportFsItem
            // 
            this.romExportFsItem.Image = global::Z64.Properties.Resources.multiple_files;
            this.romExportFsItem.Name = "romExportFsItem";
            this.romExportFsItem.Size = new System.Drawing.Size(187, 22);
            this.romExportFsItem.Text = "Export FS";
            this.romExportFsItem.Click += new System.EventHandler(this.RomExportFsItem_Click);
            // 
            // romSaveItem
            // 
            this.romSaveItem.Image = global::Z64.Properties.Resources.save;
            this.romSaveItem.Name = "romSaveItem";
            this.romSaveItem.Size = new System.Drawing.Size(187, 22);
            this.romSaveItem.Text = "Save As";
            this.romSaveItem.Click += new System.EventHandler(this.RomSaveItem_Click);
            // 
            // romImportNamesItem
            // 
            this.romImportNamesItem.Name = "romImportNamesItem";
            this.romImportNamesItem.Size = new System.Drawing.Size(187, 22);
            this.romImportNamesItem.Text = "Import File Name List";
            this.romImportNamesItem.Click += new System.EventHandler(this.RomImportNamesItem_Click);
            // 
            // romExportNamesItem
            // 
            this.romExportNamesItem.Name = "romExportNamesItem";
            this.romExportNamesItem.Size = new System.Drawing.Size(187, 22);
            this.romExportNamesItem.Text = "Export File Name List";
            this.romExportNamesItem.Click += new System.EventHandler(this.RomExportNamesItem_Click);
            // 
            // toolsToolStripButton
            // 
            this.toolsToolStripButton.AutoToolTip = false;
            this.toolsToolStripButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openObjectToolStripMenuItem,
            this.f3DEXDisassemblerToolStripMenuItem,
            this.ROMRAMConversionsToolStripMenuItem,
            this.textureViewerToolStripMenuItem,
            this.objectAnalyzerToolStripMenuItem});
            this.toolsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolsToolStripButton.Name = "toolsToolStripButton";
            this.toolsToolStripButton.Size = new System.Drawing.Size(47, 22);
            this.toolsToolStripButton.Text = "Tools";
            // 
            // openObjectToolStripMenuItem
            // 
            this.openObjectToolStripMenuItem.Image = global::Z64.Properties.Resources.cube;
            this.openObjectToolStripMenuItem.Name = "openObjectToolStripMenuItem";
            this.openObjectToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.openObjectToolStripMenuItem.Text = "Open DList Viewer";
            this.openObjectToolStripMenuItem.Click += new System.EventHandler(this.OpenDlistViewerToolStripMenuItem_Click);
            // 
            // f3DEXDisassemblerToolStripMenuItem
            // 
            this.f3DEXDisassemblerToolStripMenuItem.Image = global::Z64.Properties.Resources.code;
            this.f3DEXDisassemblerToolStripMenuItem.Name = "f3DEXDisassemblerToolStripMenuItem";
            this.f3DEXDisassemblerToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.f3DEXDisassemblerToolStripMenuItem.Text = "F3DZEX Disassembler";
            this.f3DEXDisassemblerToolStripMenuItem.Click += new System.EventHandler(this.f3DEXDisassemblerToolStripMenuItem_Click);
            // 
            // ROMRAMConversionsToolStripMenuItem
            // 
            this.ROMRAMConversionsToolStripMenuItem.Image = global::Z64.Properties.Resources.conversion;
            this.ROMRAMConversionsToolStripMenuItem.Name = "ROMRAMConversionsToolStripMenuItem";
            this.ROMRAMConversionsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.ROMRAMConversionsToolStripMenuItem.Text = "ROM/RAM Conversions";
            this.ROMRAMConversionsToolStripMenuItem.Click += new System.EventHandler(this.ROMRAMConversionsToolStripMenuItem_Click);
            // 
            // textureViewerToolStripMenuItem
            // 
            this.textureViewerToolStripMenuItem.Image = global::Z64.Properties.Resources.texture;
            this.textureViewerToolStripMenuItem.Name = "textureViewerToolStripMenuItem";
            this.textureViewerToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.textureViewerToolStripMenuItem.Text = "Texture Viewer";
            this.textureViewerToolStripMenuItem.Click += new System.EventHandler(this.textureViewerToolStripMenuItem_Click);
            // 
            // helpToolStripButton
            // 
            this.helpToolStripButton.AutoToolTip = false;
            this.helpToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.helpToolStripButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkNewReleasesToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.helpToolStripButton.Name = "helpToolStripButton";
            this.helpToolStripButton.Size = new System.Drawing.Size(45, 22);
            this.helpToolStripButton.Text = "Help";
            // 
            // checkNewReleasesToolStripMenuItem
            // 
            this.checkNewReleasesToolStripMenuItem.Name = "checkNewReleasesToolStripMenuItem";
            this.checkNewReleasesToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.checkNewReleasesToolStripMenuItem.Text = "Check New Releases";
            this.checkNewReleasesToolStripMenuItem.Click += new System.EventHandler(this.checkNewReleasesToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // objectAnalyzerToolStripMenuItem
            // 
            this.objectAnalyzerToolStripMenuItem.Name = "objectAnalyzerToolStripMenuItem";
            this.objectAnalyzerToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.objectAnalyzerToolStripMenuItem.Text = "Object Analyzer";
            this.objectAnalyzerToolStripMenuItem.Click += new System.EventHandler(this.objectAnalyzerToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 507);
            this.Controls.Add(this.mainToolStrip);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label_loadProgress);
            this.Controls.Add(this.progressBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Z64 Utils";
            this.tabControl1.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.contextMenuStrip_fs.ResumeLayout(false);
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label_loadProgress;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ToolStripDropDownButton romToolStripButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_fs;
        private System.Windows.Forms.ToolStripMenuItem openObjectViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem injectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.ListView listView_files;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.TextBox textBox_fileFilter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStripMenuItem openObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem f3DEXDisassemblerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ROMRAMConversionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textureViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkNewReleasesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripMenuItem romOpenItem;
        private System.Windows.Forms.ToolStripMenuItem romExportFsItem;
        private System.Windows.Forms.ToolStripMenuItem romSaveItem;
        private System.Windows.Forms.ToolStripMenuItem romImportNamesItem;
        private System.Windows.Forms.ToolStripDropDownButton toolsToolStripButton;
        private System.Windows.Forms.ToolStripDropDownButton helpToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem romExportNamesItem;
        private System.Windows.Forms.ToolStripMenuItem objectAnalyzerToolStripMenuItem;
    }
}

