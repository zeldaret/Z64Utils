using System.Collections.Generic;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTK.Mathematics;
using Z64;

namespace Z64Utils_Avalonia;

public partial class CollisionViewerWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    private List<CollisionPolygon> _polygons;

    public CollisionViewerWindowViewModel()
    {
        Polygons = new();
    }

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
}

public struct CollisionPolygon
{
    public Vector3[] verts;
    public Vec3s normal;
}
