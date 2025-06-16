using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using Avalonia;
using F3DZEX.Command;
using F3DZEX.Render;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Z64Utils_Avalonia;

public class CollisionViewerControl : OpenTKControlBaseWithCamera
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public static readonly StyledProperty<List<CollisionPolygon>?> PolygonsProperty =
        AvaloniaProperty.Register<DLViewerControl, List<CollisionPolygon>?>(
            nameof(Polygons),
            defaultValue: null
        );
    public List<CollisionPolygon>? Polygons
    {
        get => GetValue(PolygonsProperty);
        set => SetValue(PolygonsProperty, value);
    }

    public CollisionVertexDrawer? collisionVertexDrawer;

    public CollisionViewerControl()
        : base(
            new CameraHandling(
                camPos: new Vector3(0, -2000, -15000),
                angle: new Vector3(20, -30, 0)
            )
        )
    {
        Logger.Debug("Name={Name}", Name);

        PropertyChanged += (sender, e) =>
        {
            if (e.Property == PolygonsProperty)
            {
                // TODO build data
                collisionVertexDrawer = null; // TODO hack for now
                RequestNextFrameRenderingIfInitialized();
            }
        };
    }

    protected override void OnOpenTKInit() { }

    protected override void OnOpenTKRender()
    {
        Logger.Trace("Name={Name} in", Name);
        SetFullViewport();

        // TODO understand lol

        GL.ClearColor(0.5f, 0.5f, 0.0f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        bool drawWireframe = true;

        if (collisionVertexDrawer == null)
        {
            collisionVertexDrawer = new();

            if (Polygons != null)
            {
                if (drawWireframe)
                {
                    collisionVertexDrawer.SetDataLines(
                        Polygons.ToArray(), // TODO avoid ToArray()
                        BufferUsageHint.StaticDraw
                    );
                }
                else
                {
                    collisionVertexDrawer.SetDataTriangles(
                        Polygons.ToArray(),
                        BufferUsageHint.StaticDraw
                    );
                }
            }
        }

        Matrix4 proj = Proj,
            view = View;
        //Matrix4 proj = Matrix4.Identity, view = Matrix4.Identity;
        collisionVertexDrawer.SendProjViewMatrices(ref proj, ref view);
        collisionVertexDrawer.SendModelMatrix(Matrix4.Identity);
        collisionVertexDrawer.SendColor(Color.DarkCyan);

        if (drawWireframe)
        {
            GL.LineWidth(10);
            collisionVertexDrawer.Draw(PrimitiveType.Lines);
        }
        else
        {
            GL.Enable(EnableCap.CullFace);
            collisionVertexDrawer.Draw(PrimitiveType.Triangles);
        }

        Logger.Trace("Name={Name} out", Name);
    }
}
