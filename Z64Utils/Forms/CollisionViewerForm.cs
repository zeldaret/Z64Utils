using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;
using RDP;
using System.IO;
using System.Globalization;
using Syroot.BinaryData;
using Z64;
using Common;

namespace Z64.Forms
{
    public partial class CollisionViewerForm : MicrosoftFontForm
    {
        public static CollisionViewerForm Instance { get; set; }

        public struct RenderColPoly
        {
            public double[][] Points;
            public Vec3s Normal;
        }

        Z64Game _game;
        F3DZEX.Render.Renderer _renderer;
        F3DZEX.Render.Renderer.Config _rendererCfg;
        SettingsForm _settingsForm;

        Z64Object.ColHeaderHolder _colHeader;
        RenderColPoly[] _polygons;

        bool _cullBack;

        private CollisionViewerForm(Z64Game game)
        {
            _game = game;
            _rendererCfg = new F3DZEX.Render.Renderer.Config();
            _rendererCfg.HighlightColor = Color.Gray;
            _rendererCfg.RenderMode = F3DZEX.Render.RdpVertexDrawer.ModelRenderMode.Wireframe;
            _renderer = new F3DZEX.Render.Renderer(game, _rendererCfg);

            _colHeader = null;
            _polygons = null;

            _cullBack = true;

            InitializeComponent();
            Toolkit.Init();

            modelViewer.RenderCallback = RenderCallback;

            NewRender();
        }

        public static void OpenInstance(Z64Game game)
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

        private void RenderCallback(Matrix4 proj, Matrix4 view)
        {
            if (_polygons == null)
                return;

            _renderer.RenderStart(proj, view);
            _renderer.PrepareForCollisionRender();

            if (_cullBack)
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.Disable(EnableCap.CullFace);
            }

            GL.Begin(PrimitiveType.Triangles);
            foreach (var poly in _polygons)
            {
                GL.Normal3(poly.Normal.X, poly.Normal.Y, poly.Normal.Z);
                GL.Vertex3(poly.Points[0]);
                GL.Vertex3(poly.Points[1]);
                GL.Vertex3(poly.Points[2]);
            }
            GL.End();
        }

        private void NewRender()
        {
            _renderer.ClearErrors();

            toolStripStatusErrorLabel.Text = "";

            if (_colHeader != null)
            {
                _polygons = new RenderColPoly[_colHeader.NbPolygons];

                for (int i = 0; i < _colHeader.NbPolygons; i++)
                {
                    Z64Object.CollisionPolygonsHolder.CollisionPoly colPoly = _colHeader.PolygonsHolder.CollisionPolys[i];
                    Vec3s v0 = _colHeader.VerticesHolder.Points[colPoly.Data[0] & 0x1FFF];
                    Vec3s v1 = _colHeader.VerticesHolder.Points[colPoly.Data[1] & 0x1FFF];
                    Vec3s v2 = _colHeader.VerticesHolder.Points[colPoly.Data[2] & 0x1FFF];

                    _polygons[i] = new RenderColPoly()
                    {
                        Points = new double[3][] {
                            new double[3] { v0.X, v0.Y, v0.Z },
                            new double[3] { v1.X, v1.Y, v1.Z },
                            new double[3] { v2.X, v2.Y, v2.Z }
                        },
                        Normal = colPoly.Normal
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
            _settingsForm?.Close();
        }

        private void toolStripRenderCfgBtn_Click(object sender, EventArgs e)
        {
            if (_settingsForm != null)
            {
                _settingsForm.Activate();
            }
            else
            {
                _settingsForm = new SettingsForm(_rendererCfg);
                _settingsForm.FormClosed += (sender, e) => _settingsForm = null;
                _settingsForm.SettingsChanged += (sender, e) => { modelViewer.Render(); };
                _settingsForm.Show();
            }
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
