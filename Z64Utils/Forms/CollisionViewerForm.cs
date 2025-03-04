using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using F3DZEX.Command;
using F3DZEX.Render;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using RDP;
using Syroot.BinaryData;
using Z64;

namespace Z64.Forms
{
    public partial class CollisionViewerForm : MicrosoftFontForm
    {
        public static CollisionViewerForm? Instance { get; set; }

        public struct RenderColPoly
        {
            public float[][] Points;
            public Vec3s Normal;
        }

        private Z64Game? _game;

        private Z64Object.ColHeaderHolder? _colHeader;
        private RenderColPoly[]? _polygons;

        private bool _cullBack;
        private bool _wireframe;

        private CollisionViewerForm(Z64Game? game)
        {
            _game = game;

            _colHeader = null;
            _polygons = null;

            _cullBack = true;
            _wireframe = false;

            InitializeComponent();
            Toolkit.Init();

            modelViewer.RenderCallback = RenderCallback;

            NewRender();
        }

        [MemberNotNull(nameof(Instance))]
        public static void OpenInstance(Z64Game? game)
        {
            if (Instance == null)
            {
                Instance = new CollisionViewerForm(game);
                Instance.Show();
            }
            else
            {
                Instance.Activate();
            }
        }

        public void SetColHeader(Z64Object.ColHeaderHolder collisionHeader)
        {
            _colHeader = collisionHeader;
            NewRender();
        }

        private CollisionVertexDrawer? cvd;

        private void RenderCallback(Matrix4 proj, Matrix4 view)
        {
            if (_polygons == null)
                return;

            GL.ClearColor(Color.DarkCyan);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_cullBack)
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.Disable(EnableCap.CullFace);
            }

            if (cvd == null)
                cvd = new();

            cvd.SendProjViewMatrices(ref proj, ref view);
            cvd.SendModelMatrix(Matrix4.Identity);
            cvd.SendColor(Color.LightGray);

            // TODO it would be better to only call the SetData functions once instead of every time
            if (_wireframe)
            {
                GL.LineWidth(3);
                cvd.SetDataLines(_polygons, BufferUsageHint.DynamicDraw);
                cvd.Draw(PrimitiveType.Lines);
            }
            else
            {
                cvd.SetDataTriangles(_polygons, BufferUsageHint.DynamicDraw);
                cvd.Draw(PrimitiveType.Triangles);
            }
        }

        private void NewRender()
        {
            toolStripStatusErrorLabel.Text = "";

            if (_colHeader != null)
            {
                _polygons = new RenderColPoly[_colHeader.NbPolygons];

                Debug.Assert(_colHeader.PolygonsHolder != null);
                Debug.Assert(_colHeader.VerticesHolder != null);
                for (int i = 0; i < _colHeader.NbPolygons; i++)
                {
                    Z64Object.CollisionPolygonsHolder.CollisionPoly colPoly = _colHeader
                        .PolygonsHolder
                        .CollisionPolys[i];
                    Vec3s v0 = _colHeader.VerticesHolder.Points[colPoly.Data[0] & 0x1FFF];
                    Vec3s v1 = _colHeader.VerticesHolder.Points[colPoly.Data[1] & 0x1FFF];
                    Vec3s v2 = _colHeader.VerticesHolder.Points[colPoly.Data[2] & 0x1FFF];

                    _polygons[i] = new RenderColPoly()
                    {
                        Points = new float[3][]
                        {
                            new float[3] { v0.X, v0.Y, v0.Z },
                            new float[3] { v1.X, v1.Y, v1.Z },
                            new float[3] { v2.X, v2.Y, v2.Z },
                        },
                        Normal = colPoly.Normal,
                    };
                }
            }
            else
            {
                _polygons = null;
            }

            modelViewer.Render();
        }

        private void ColViewerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
        }

        private void toolStripRenderCfgBtn_Click(object sender, EventArgs e)
        {
            // TODO open actual render config instead of just toggling wireframe rendering
            _wireframe = !_wireframe;
            modelViewer.Render();
        }

        private void cullingCfgBtn_Click(object sender, EventArgs e)
        {
            if (_cullBack)
            {
                _cullBack = false;
                cullingCfgBtn.Text = "Enable Backface Culling";
                cullingCfgBtn.ToolTipText = "Enable Backface Culling";
            }
            else
            {
                _cullBack = true;
                cullingCfgBtn.Text = "Disable Backface Culling";
                cullingCfgBtn.ToolTipText = "Disable Backface Culling";
            }
            modelViewer.Render();
        }

        private void saveScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = Filters.PNG;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var bmp = modelViewer.CaptureScreen();
                bmp.Save(saveFileDialog1.FileName);
            }
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bmp = modelViewer.CaptureScreen();
            Clipboard.SetImage(bmp);
        }

        private void modelViewer_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(modelViewer.PointToScreen(e.Location));
            }
        }
    }
}
