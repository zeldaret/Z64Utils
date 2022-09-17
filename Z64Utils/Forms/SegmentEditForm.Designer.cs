namespace Z64.Forms
{
    partial class SegmentEditForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SegmentEditForm));
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_address = new System.Windows.Forms.TabPage();
            this.addressValue = new System.Windows.Forms.TextBox();
            this.tabPage_file = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.tabPage_primColor = new System.Windows.Forms.TabPage();
            this.primColorALabel = new System.Windows.Forms.Label();
            this.primColorA = new System.Windows.Forms.TextBox();
            this.primColorBLabel = new System.Windows.Forms.Label();
            this.primColorB = new System.Windows.Forms.TextBox();
            this.primColorGLabel = new System.Windows.Forms.Label();
            this.primColorG = new System.Windows.Forms.TextBox();
            this.primColorRLabel = new System.Windows.Forms.Label();
            this.primColorR = new System.Windows.Forms.TextBox();
            this.primColorLodFracLabel = new System.Windows.Forms.Label();
            this.primColorLodFrac = new System.Windows.Forms.TextBox();
            this.tabPage_empty = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.okBtn = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tabPage_envColor = new System.Windows.Forms.TabPage();
            this.envColorALabel = new System.Windows.Forms.Label();
            this.envColorA = new System.Windows.Forms.TextBox();
            this.envColorBLabel = new System.Windows.Forms.Label();
            this.envColorB = new System.Windows.Forms.TextBox();
            this.envColorGLabel = new System.Windows.Forms.Label();
            this.envColorG = new System.Windows.Forms.TextBox();
            this.envColorRLabel = new System.Windows.Forms.Label();
            this.envColorR = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage_address.SuspendLayout();
            this.tabPage_file.SuspendLayout();
            this.tabPage_primColor.SuspendLayout();
            this.tabPage_envColor.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Empty",
            "Address",
            "ROM FS",
            "File",
            "Ident Matrices",
            "Null Bytes",
            "Empty Dlist"});
            this.comboBox1.Location = new System.Drawing.Point(21, 22);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(108, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_address);
            this.tabControl1.Controls.Add(this.tabPage_file);
            this.tabControl1.Controls.Add(this.tabPage_primColor);
            this.tabControl1.Controls.Add(this.tabPage_envColor);
            this.tabControl1.Controls.Add(this.tabPage_empty);
            this.tabControl1.Location = new System.Drawing.Point(26, 45);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(99, 219);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage_address
            // 
            this.tabPage_address.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage_address.Controls.Add(this.addressValue);
            this.tabPage_address.Location = new System.Drawing.Point(4, 22);
            this.tabPage_address.Name = "tabPage_address";
            this.tabPage_address.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_address.Size = new System.Drawing.Size(91, 193);
            this.tabPage_address.TabIndex = 0;
            this.tabPage_address.Text = "address";
            // 
            // addressValue
            // 
            this.addressValue.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.addressValue.ForeColor = System.Drawing.SystemColors.ControlText;
            this.addressValue.Location = new System.Drawing.Point(5, 3);
            this.addressValue.Name = "addressValue";
            this.addressValue.Size = new System.Drawing.Size(80, 20);
            this.addressValue.TabIndex = 5;
            this.addressValue.Text = "00000000";
            this.addressValue.TextChanged += new System.EventHandler(this.addressValue_TextChanged);
            // 
            // tabPage_file
            // 
            this.tabPage_file.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage_file.Controls.Add(this.button1);
            this.tabPage_file.Location = new System.Drawing.Point(4, 22);
            this.tabPage_file.Name = "tabPage_file";
            this.tabPage_file.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_file.Size = new System.Drawing.Size(91, 193);
            this.tabPage_file.TabIndex = 1;
            this.tabPage_file.Text = "file";
            // 
            // button1
            // 
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(8, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Select File";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabPage_primColor
            // 
            this.tabPage_primColor.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage_primColor.Controls.Add(this.primColorALabel);
            this.tabPage_primColor.Controls.Add(this.primColorA);
            this.tabPage_primColor.Controls.Add(this.primColorBLabel);
            this.tabPage_primColor.Controls.Add(this.primColorB);
            this.tabPage_primColor.Controls.Add(this.primColorGLabel);
            this.tabPage_primColor.Controls.Add(this.primColorG);
            this.tabPage_primColor.Controls.Add(this.primColorRLabel);
            this.tabPage_primColor.Controls.Add(this.primColorR);
            this.tabPage_primColor.Controls.Add(this.primColorLodFracLabel);
            this.tabPage_primColor.Controls.Add(this.primColorLodFrac);
            this.tabPage_primColor.Location = new System.Drawing.Point(4, 22);
            this.tabPage_primColor.Name = "tabPage_primColor";
            this.tabPage_primColor.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_primColor.Size = new System.Drawing.Size(91, 193);
            this.tabPage_primColor.TabIndex = 3;
            this.tabPage_primColor.Text = "primColor";
            // 
            // primColorALabel
            // 
            this.primColorALabel.AutoSize = true;
            this.primColorALabel.Location = new System.Drawing.Point(4, 151);
            this.primColorALabel.Name = "primColorALabel";
            this.primColorALabel.Size = new System.Drawing.Size(13, 13);
            this.primColorALabel.TabIndex = 9;
            this.primColorALabel.Text = "a";
            // 
            // primColorA
            // 
            this.primColorA.Location = new System.Drawing.Point(5, 165);
            this.primColorA.Name = "primColorA";
            this.primColorA.Size = new System.Drawing.Size(80, 20);
            this.primColorA.TabIndex = 8;
            this.primColorA.TextChanged += new System.EventHandler(this.primColorA_TextChanged);
            // 
            // primColorBLabel
            // 
            this.primColorBLabel.AutoSize = true;
            this.primColorBLabel.Location = new System.Drawing.Point(4, 114);
            this.primColorBLabel.Name = "primColorBLabel";
            this.primColorBLabel.Size = new System.Drawing.Size(13, 13);
            this.primColorBLabel.TabIndex = 7;
            this.primColorBLabel.Text = "b";
            // 
            // primColorB
            // 
            this.primColorB.Location = new System.Drawing.Point(5, 128);
            this.primColorB.Name = "primColorB";
            this.primColorB.Size = new System.Drawing.Size(80, 20);
            this.primColorB.TabIndex = 6;
            this.primColorB.TextChanged += new System.EventHandler(this.primColorB_TextChanged);
            // 
            // primColorGLabel
            // 
            this.primColorGLabel.AutoSize = true;
            this.primColorGLabel.Location = new System.Drawing.Point(4, 77);
            this.primColorGLabel.Name = "primColorGLabel";
            this.primColorGLabel.Size = new System.Drawing.Size(13, 13);
            this.primColorGLabel.TabIndex = 5;
            this.primColorGLabel.Text = "g";
            // 
            // primColorG
            // 
            this.primColorG.Location = new System.Drawing.Point(5, 91);
            this.primColorG.Name = "primColorG";
            this.primColorG.Size = new System.Drawing.Size(80, 20);
            this.primColorG.TabIndex = 4;
            this.primColorG.TextChanged += new System.EventHandler(this.primColorG_TextChanged);
            // 
            // primColorRLabel
            // 
            this.primColorRLabel.AutoSize = true;
            this.primColorRLabel.Location = new System.Drawing.Point(4, 40);
            this.primColorRLabel.Name = "primColorRLabel";
            this.primColorRLabel.Size = new System.Drawing.Size(10, 13);
            this.primColorRLabel.TabIndex = 3;
            this.primColorRLabel.Text = "r";
            // 
            // primColorR
            // 
            this.primColorR.Location = new System.Drawing.Point(5, 54);
            this.primColorR.Name = "primColorR";
            this.primColorR.Size = new System.Drawing.Size(80, 20);
            this.primColorR.TabIndex = 2;
            this.primColorR.TextChanged += new System.EventHandler(this.primColorR_TextChanged);
            // 
            // primColorLodFracLabel
            // 
            this.primColorLodFracLabel.AutoSize = true;
            this.primColorLodFracLabel.Location = new System.Drawing.Point(4, 3);
            this.primColorLodFracLabel.Name = "primColorLodFracLabel";
            this.primColorLodFracLabel.Size = new System.Drawing.Size(39, 13);
            this.primColorLodFracLabel.TabIndex = 1;
            this.primColorLodFracLabel.Text = "lodfrac";
            // 
            // primColorLodFrac
            // 
            this.primColorLodFrac.Location = new System.Drawing.Point(5, 17);
            this.primColorLodFrac.Name = "primColorLodFrac";
            this.primColorLodFrac.Size = new System.Drawing.Size(80, 20);
            this.primColorLodFrac.TabIndex = 0;
            this.primColorLodFrac.TextChanged += new System.EventHandler(this.primColorLodFrac_TextChanged);
            // 
            // tabPage_empty
            // 
            this.tabPage_empty.Location = new System.Drawing.Point(4, 22);
            this.tabPage_empty.Name = "tabPage_empty";
            this.tabPage_empty.Size = new System.Drawing.Size(91, 193);
            this.tabPage_empty.TabIndex = 2;
            this.tabPage_empty.Text = "Empty";
            this.tabPage_empty.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(53, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Source:";
            // 
            // okBtn
            // 
            this.okBtn.Enabled = false;
            this.okBtn.Location = new System.Drawing.Point(38, 268);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 3;
            this.okBtn.Text = "Ok";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // tabPage_envColor
            // 
            this.tabPage_envColor.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage_envColor.Controls.Add(this.envColorALabel);
            this.tabPage_envColor.Controls.Add(this.envColorA);
            this.tabPage_envColor.Controls.Add(this.envColorBLabel);
            this.tabPage_envColor.Controls.Add(this.envColorB);
            this.tabPage_envColor.Controls.Add(this.envColorGLabel);
            this.tabPage_envColor.Controls.Add(this.envColorG);
            this.tabPage_envColor.Controls.Add(this.envColorRLabel);
            this.tabPage_envColor.Controls.Add(this.envColorR);
            this.tabPage_envColor.Location = new System.Drawing.Point(4, 22);
            this.tabPage_envColor.Name = "tabPage_envColor";
            this.tabPage_envColor.Size = new System.Drawing.Size(91, 193);
            this.tabPage_envColor.TabIndex = 4;
            this.tabPage_envColor.Text = "envColor";
            // 
            // envColorALabel
            // 
            this.envColorALabel.AutoSize = true;
            this.envColorALabel.Location = new System.Drawing.Point(4, 114);
            this.envColorALabel.Name = "envColorALabel";
            this.envColorALabel.Size = new System.Drawing.Size(13, 13);
            this.envColorALabel.TabIndex = 17;
            this.envColorALabel.Text = "a";
            // 
            // envColorA
            // 
            this.envColorA.Location = new System.Drawing.Point(5, 128);
            this.envColorA.Name = "envColorA";
            this.envColorA.Size = new System.Drawing.Size(80, 20);
            this.envColorA.TabIndex = 16;
            this.envColorA.TextChanged += new System.EventHandler(this.envColorA_TextChanged);
            // 
            // envColorBLabel
            // 
            this.envColorBLabel.AutoSize = true;
            this.envColorBLabel.Location = new System.Drawing.Point(4, 77);
            this.envColorBLabel.Name = "envColorBLabel";
            this.envColorBLabel.Size = new System.Drawing.Size(13, 13);
            this.envColorBLabel.TabIndex = 15;
            this.envColorBLabel.Text = "b";
            // 
            // envColorB
            // 
            this.envColorB.Location = new System.Drawing.Point(5, 91);
            this.envColorB.Name = "envColorB";
            this.envColorB.Size = new System.Drawing.Size(80, 20);
            this.envColorB.TabIndex = 14;
            this.envColorB.TextChanged += new System.EventHandler(this.envColorB_TextChanged);
            // 
            // envColorGLabel
            // 
            this.envColorGLabel.AutoSize = true;
            this.envColorGLabel.Location = new System.Drawing.Point(4, 40);
            this.envColorGLabel.Name = "envColorGLabel";
            this.envColorGLabel.Size = new System.Drawing.Size(13, 13);
            this.envColorGLabel.TabIndex = 13;
            this.envColorGLabel.Text = "g";
            // 
            // envColorG
            // 
            this.envColorG.Location = new System.Drawing.Point(5, 54);
            this.envColorG.Name = "envColorG";
            this.envColorG.Size = new System.Drawing.Size(80, 20);
            this.envColorG.TabIndex = 12;
            this.envColorG.TextChanged += new System.EventHandler(this.envColorG_TextChanged);
            // 
            // envColorRLabel
            // 
            this.envColorRLabel.AutoSize = true;
            this.envColorRLabel.Location = new System.Drawing.Point(4, 3);
            this.envColorRLabel.Name = "envColorRLabel";
            this.envColorRLabel.Size = new System.Drawing.Size(10, 13);
            this.envColorRLabel.TabIndex = 11;
            this.envColorRLabel.Text = "r";
            // 
            // envColorR
            // 
            this.envColorR.Location = new System.Drawing.Point(5, 17);
            this.envColorR.Name = "envColorR";
            this.envColorR.Size = new System.Drawing.Size(80, 20);
            this.envColorR.TabIndex = 10;
            this.envColorR.TextChanged += new System.EventHandler(this.envColorR_TextChanged);
            // 
            // SegmentEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(150, 302);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.comboBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SegmentEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Segment";
            this.tabControl1.ResumeLayout(false);
            this.tabPage_address.ResumeLayout(false);
            this.tabPage_address.PerformLayout();
            this.tabPage_file.ResumeLayout(false);
            this.tabPage_primColor.ResumeLayout(false);
            this.tabPage_primColor.PerformLayout();
            this.tabPage_envColor.ResumeLayout(false);
            this.tabPage_envColor.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_address;
        private System.Windows.Forms.TabPage tabPage_file;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox addressValue;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TabPage tabPage_empty;
        private System.Windows.Forms.TabPage tabPage_primColor;
        private System.Windows.Forms.TextBox primColorLodFrac;
        private System.Windows.Forms.Label primColorLodFracLabel;
        private System.Windows.Forms.Label primColorRLabel;
        private System.Windows.Forms.TextBox primColorR;
        private System.Windows.Forms.Label primColorALabel;
        private System.Windows.Forms.TextBox primColorA;
        private System.Windows.Forms.Label primColorBLabel;
        private System.Windows.Forms.TextBox primColorB;
        private System.Windows.Forms.Label primColorGLabel;
        private System.Windows.Forms.TextBox primColorG;
        private System.Windows.Forms.TabPage tabPage_envColor;
        private System.Windows.Forms.Label envColorALabel;
        private System.Windows.Forms.TextBox envColorA;
        private System.Windows.Forms.Label envColorBLabel;
        private System.Windows.Forms.TextBox envColorB;
        private System.Windows.Forms.Label envColorGLabel;
        private System.Windows.Forms.TextBox envColorG;
        private System.Windows.Forms.Label envColorRLabel;
        private System.Windows.Forms.TextBox envColorR;
    }
}