using System;
using System.Collections.Generic;
using Avalonia.Media;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTK.Mathematics;
using Z64;

namespace Z64Utils_Avalonia;

public partial class CollisionViewerWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    private List<CollisionPolygon> _polygons = new();

    [ObservableProperty]
    private CollisionRenderSettings _renderSettings = new();

    // Provided by the view
    public Action<CollisionRenderSettings>? OpenCollisionRenderSettings;

    public void SetCollisionHeader(Z64Object.ColHeaderHolder collisionHeaderHolder)
    {
        Utils.Assert(collisionHeaderHolder.VerticesHolder != null);
        Utils.Assert(collisionHeaderHolder.PolygonsHolder != null);
        List<CollisionPolygon> polygons = new();
        var points = collisionHeaderHolder.VerticesHolder.Points;
        foreach (var poly in collisionHeaderHolder.PolygonsHolder.CollisionPolys)
        {
            var v0 = points[poly.Data[0] & 0x1FFF];
            var v1 = points[poly.Data[1] & 0x1FFF];
            var v2 = points[poly.Data[2] & 0x1FFF];
            polygons.Add(
                new()
                {
                    verts = new Vector3[]
                    {
                        new(v0.X, v0.Y, v0.Z),
                        new(v1.X, v1.Y, v1.Z),
                        new(v2.X, v2.Y, v2.Z),
                    },
                    normal = poly.Normal,
                }
            );
        }
        Polygons = polygons;
    }

    public void OpenCollisionRenderSettingsCommand()
    {
        Utils.Assert(OpenCollisionRenderSettings != null);
        OpenCollisionRenderSettings(RenderSettings);
    }
}

public struct CollisionPolygon
{
    public Vector3[] verts;
    public Vec3s normal;
}

public enum CollisionRenderMode
{
    Wireframe,
    Solid,
}

public partial class CollisionRenderSettings : ObservableObject
{
    [ObservableProperty]
    private Color _backgroundColor = Color.FromRgb(30, 144, 255);

    [ObservableProperty]
    private CollisionRenderMode _renderMode = CollisionRenderMode.Solid;
}
