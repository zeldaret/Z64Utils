using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Avalonia;
using Common;
using F3DZEX.Render;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Z64Utils_Avalonia;

public class CollisionViewerControl : OpenTKControlBaseWithCamera
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public static readonly StyledProperty<CollisionRenderSettings?> RenderSettingsProperty =
        AvaloniaProperty.Register<CollisionViewerControl, CollisionRenderSettings?>(
            nameof(RenderSettings),
            defaultValue: null
        );
    public CollisionRenderSettings? RenderSettings
    {
        get => GetValue(RenderSettingsProperty);
        set => SetValue(RenderSettingsProperty, value);
    }

    public static readonly StyledProperty<List<CollisionPolygon>?> PolygonsProperty =
        AvaloniaProperty.Register<CollisionViewerControl, List<CollisionPolygon>?>(
            nameof(Polygons),
            defaultValue: null
        );
    public List<CollisionPolygon>? Polygons
    {
        get => GetValue(PolygonsProperty);
        set => SetValue(PolygonsProperty, value);
    }

    private CollisionVertexDrawer? _collisionVertexDrawer;

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
            if (e.Property == PolygonsProperty || e.Property == RenderSettingsProperty)
            {
                if (_collisionVertexDrawer != null && RenderSettings != null && Polygons != null)
                {
                    SetDrawerData();
                    RequestNextFrameRenderingIfInitialized();
                }
            }
            if (e.Property == RenderSettingsProperty)
            {
                if (e.OldValue != null)
                {
                    var prev = (CollisionRenderSettings)e.OldValue;
                    prev.PropertyChanged -= OnRenderSettingsChanged;
                }
                if (RenderSettings != null)
                    RenderSettings.PropertyChanged += OnRenderSettingsChanged;
            }
        };
    }

    private void OnRenderSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        Utils.Assert(RenderSettings != null);
        if (_collisionVertexDrawer != null && Polygons != null)
        {
            SetDrawerData();
            RequestNextFrameRenderingIfInitialized();
        }
    }

    public void SetDrawerData()
    {
        Utils.Assert(RenderSettings != null);
        Utils.Assert(Polygons != null);
        Utils.Assert(_collisionVertexDrawer != null);

        switch (RenderSettings.RenderMode)
        {
            case CollisionRenderMode.Wireframe:
                _collisionVertexDrawer.SetDataLines(Polygons, BufferUsageHint.StaticDraw);
                break;

            case CollisionRenderMode.Solid:
                _collisionVertexDrawer.SetDataTriangles(Polygons, BufferUsageHint.StaticDraw);
                break;

            default:
                throw new NotImplementedException($"{RenderSettings.RenderMode}");
        }
    }

    protected override void OnOpenTKInit()
    {
        _collisionVertexDrawer = new();

        if (RenderSettings != null && Polygons != null)
        {
            SetDrawerData();
        }
    }

    protected override void OnOpenTKRender()
    {
        Logger.Trace("Name={Name} in", Name);

        if (RenderSettings == null)
            return;

        SetFullViewport();

        GL.ClearColor(
            RenderSettings.BackgroundColor.R / 255.0f,
            RenderSettings.BackgroundColor.G / 255.0f,
            RenderSettings.BackgroundColor.B / 255.0f,
            1.0f
        );
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Utils.Assert(_collisionVertexDrawer != null);

        Matrix4 proj = Proj,
            view = View;
        _collisionVertexDrawer.SendProjViewMatrices(ref proj, ref view);
        _collisionVertexDrawer.SendModelMatrix(Matrix4.Identity);
        _collisionVertexDrawer.SendColor(Color.LightGray);

        switch (RenderSettings.RenderMode)
        {
            case CollisionRenderMode.Wireframe:
                GL.LineWidth(3);
                _collisionVertexDrawer.Draw(PrimitiveType.Lines);
                break;

            case CollisionRenderMode.Solid:
                GL.Enable(EnableCap.CullFace);
                _collisionVertexDrawer.Draw(PrimitiveType.Triangles);
                break;

            default:
                throw new NotImplementedException($"{RenderSettings.RenderMode}");
        }

        Logger.Trace("Name={Name} out", Name);
    }
}
