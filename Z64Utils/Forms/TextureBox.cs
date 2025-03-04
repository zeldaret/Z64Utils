using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Common;

#nullable enable

/// <summary>
/// Inherits from PictureBox; adds Interpolation Mode Setting
/// </summary>
///
namespace Z64.Forms
{
    public class TextureBox : PictureBox
    {
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private SaveFileDialog saveFileDialog1;
        private ToolStripMenuItem toolStripMenuItem2;
        private System.ComponentModel.IContainer components;

        public InterpolationMode InterpolationMode { get; set; }
        public int AlphaTileSize { get; set; } = 20;

        public TextureBox()
            : base()
        {
            InitializeComponent();

            this.toolStripMenuItem1.Click += ToolStripMenuItem1_Click;
            this.toolStripMenuItem2.Click += ToolStripMenuItem2_Click;
            this.MouseClick += TextureBox_MouseClick;
        }

        private void ToolStripMenuItem2_Click(object? sender, System.EventArgs e)
        {
            Clipboard.SetImage(Image);
        }

        private void ToolStripMenuItem1_Click(object? sender, System.EventArgs e)
        {
            saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = Filters.PNG;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Image.Save(saveFileDialog1.FileName);
            }
        }

        private void TextureBox_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(PointToScreen(e.Location));
            }
        }

        [MemberNotNull(
            nameof(components),
            nameof(contextMenuStrip1),
            nameof(toolStripMenuItem1),
            nameof(saveFileDialog1),
            nameof(toolStripMenuItem2),
            nameof(contextMenuStrip1)
        )]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            //
            // contextMenuStrip1
            //
            this.contextMenuStrip1.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    this.toolStripMenuItem1,
                    this.toolStripMenuItem2,
                }
            );
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(173, 48);
            //
            // toolStripMenuItem1
            //
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(172, 22);
            this.toolStripMenuItem1.Text = "Save Texture";
            //
            // toolStripMenuItem2
            //
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(172, 22);
            this.toolStripMenuItem2.Text = "Copy To Clipboard";
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
        }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            var g = paintEventArgs.Graphics;

            for (int y = 0; y < Height; y += AlphaTileSize)
            {
                int offset = (y / AlphaTileSize) % 2 == 1 ? AlphaTileSize : 0;
                for (int x = offset; x < Width; x += AlphaTileSize * 2)
                {
                    g.FillRectangle(
                        new SolidBrush(Color.White),
                        x + AlphaTileSize,
                        y,
                        AlphaTileSize,
                        AlphaTileSize
                    );
                    g.FillRectangle(
                        new SolidBrush(Color.FromArgb(200, 200, 200)),
                        x,
                        y,
                        AlphaTileSize,
                        AlphaTileSize
                    );
                }
            }

            g.InterpolationMode = InterpolationMode;
            base.OnPaint(paintEventArgs);
        }
    }
}
