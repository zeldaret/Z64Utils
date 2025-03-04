using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using F3DZEX.Render;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace Z64.Forms
{
    public partial class ModelViewerControl : GLControl
    {
        public Matrix4 Projection => _projectionMtx;
        public Matrix4 View => _viewMtx;

        Vector3 _camPos;
        Vector3 _angle;
        Point _oldPos = Point.Empty;
        Point _oldAnglePos = Point.Empty;
        Action<Matrix4, Matrix4>? _render;
        bool _init = false;
        Matrix4 _projectionMtx;
        Matrix4 _viewMtx;

        public Action<Matrix4, Matrix4>? RenderCallback
        {
            get => _render;
            set
            {
                _render = value;
                Render();
            }
        }

        public ModelViewerControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _init = true;
            _camPos = new Vector3(0, 0, -5000);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            _camPos.Z += e.Delta * 4 * (Math.Max(0.01f, Math.Abs(_camPos.Z) / 10000));
            Render();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _oldPos = Point.Empty;
            _oldAnglePos = Point.Empty;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Right)
            {
                if (!_oldPos.IsEmpty)
                {
                    _camPos.X +=
                        (e.Location.X - _oldPos.X)
                        * (Math.Abs(_camPos.Z) / (SystemInformation.MouseWheelScrollDelta * 4));
                    _camPos.Y -=
                        (e.Location.Y - _oldPos.Y)
                        * (Math.Abs(_camPos.Z) / (SystemInformation.MouseWheelScrollDelta * 4));
                }

                _oldPos = e.Location;
            }
            if (e.Button == MouseButtons.Left)
            {
                if (!_oldAnglePos.IsEmpty)
                {
                    _angle.Y += (e.Location.X - _oldAnglePos.X) / 1.5f;
                    _angle.X += (e.Location.Y - _oldAnglePos.Y) / 1.5f;
                }

                _oldAnglePos = e.Location;
            }
            if (e.Button != MouseButtons.None)
                Render();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Render();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Render();
        }

        public void Render()
        {
            if (!_init || RenderCallback == null || Width == 0)
                return;
            try
            {
                if (!Context.IsCurrent)
                    MakeCurrent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (Height == 0)
                ClientSize = new Size(Width, 1);

            GL.Viewport(0, 0, Width, Height);

            HandleCamera();

            RenderCallback?.Invoke(_projectionMtx, _viewMtx);

            SwapBuffers();
        }

        void HandleCamera()
        {
            float aspectRatio = Width / (float)Height;
            _projectionMtx = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                aspectRatio,
                1,
                500000
            );

            _viewMtx = Matrix4.Identity;
            _viewMtx *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_angle.Y));
            _viewMtx *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_angle.X));
            _viewMtx *= Matrix4.CreateTranslation(_camPos.X, _camPos.Y, _camPos.Z);
        }

        public Bitmap CaptureScreen()
        {
            Invalidate();
            Update();
            Refresh();

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            Bitmap bmp = new Bitmap(w, h);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                ClientRectangle,
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
            );
            GL.ReadPixels(0, 0, w, h, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }
    }
}
