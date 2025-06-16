using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Rendering;
using OpenTK;
using OpenTK.Mathematics;

namespace Z64Utils_Avalonia;

public abstract class OpenTKControlBaseWithCamera : OpenTKControlBase, ICustomHitTest
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private CameraHandling _camera;

    protected Matrix4 Proj { get; private set; }
    protected Matrix4 View
    {
        get => _camera.View;
    }

    public OpenTKControlBaseWithCamera(CameraHandling? camera = null)
    {
        Logger.Debug("Name={Name}", Name);
        _camera = camera ?? new CameraHandling();
        _camera.PropertyChanged += OnCameraPropertyChanged;

        ClipToBounds = true; // cf HitTest

        UpdateProjectionMatrix();
    }

    private void OnCameraPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_camera.View):
                Logger.Trace("view changed View={View}", View);
                RequestNextFrameRenderingIfInitialized();
                break;
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        UpdateProjectionMatrix();

        // OpenGlControlBase already issues a redraw request on resize,
        // so this isn't strictly needed, but it doesn't hurt.
        // TODO test this is true
        RequestNextFrameRenderingIfInitialized();
    }

    private void UpdateProjectionMatrix()
    {
        Logger.Trace("Bounds WxH={BoundsWidth}x{BoundsHeight}", Bounds.Width, Bounds.Height);
        double aspectRatio = Bounds.Width / Bounds.Height;
        if (double.IsNaN(aspectRatio) || double.IsInfinity(aspectRatio) || aspectRatio <= 0.0)
        {
            aspectRatio = 1.0;
        }
        Proj = Matrix4.CreatePerspectiveFieldOfView(
            (float)(Math.PI / 4),
            (float)aspectRatio,
            1,
            500000
        );
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        var pos = new Vector2((float)point.Position.X, (float)point.Position.Y);

        if (point.Properties.IsLeftButtonPressed)
            _camera.OnMouseMoveWithLeftClickHeld(pos);
        if (point.Properties.IsRightButtonPressed)
            _camera.OnMouseMoveWithRightClickHeld(pos);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _camera.OnMouseUp();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        // TODO why is Delta a vector
        Logger.Trace("e.Delta={DeltaX},{DeltaY}", e.Delta.X, e.Delta.Y);
        _camera.OnMouseWheel((float)e.Delta.Y);
    }

    // Workaround https://github.com/AvaloniaUI/Avalonia/issues/10812
    // Without this, the pointer can't interact with an OpenGlControlBase
    public bool HitTest(Point p)
    {
        return true;
    }
}
