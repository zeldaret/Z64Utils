using System;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTK;
using OpenTK.Mathematics;

public partial class CameraHandling : ObservableObject
{
    private const float MOVE_SENSITIVITY_X = 1.0f;
    private const float MOVE_SENSITIVITY_Y = MOVE_SENSITIVITY_X;
    private const float MOVE_SENSITIVITY_Z = 1.0f;
    private const float TURN_SENSITIVITY_YAW = 1.0f;
    private const float TURN_SENSITIVITY_PITCH = TURN_SENSITIVITY_YAW;

    [ObservableProperty]
    private Matrix4 _view;

    private Vector3 _camPos;
    private Vector3 _angle;
    private Vector2? _oldPos = null;
    private Vector2? _oldAnglePos = null;

    public CameraHandling(Vector3? camPos = null, Vector3? angle = null)
    {
        _camPos = camPos ?? Vector3.Zero;
        _angle = angle ?? Vector3.Zero;
        ComputeView();
    }

    public void OnMouseWheel(float delta)
    {
        _camPos.Z +=
            delta * 400 * Math.Max(0.01f, Math.Abs(_camPos.Z) / 10000) * MOVE_SENSITIVITY_Z;

        ComputeView();
    }

    public void OnMouseUp()
    {
        _oldPos = null;
        _oldAnglePos = null;
    }

    public void OnMouseMoveWithRightClickHeld(Vector2 pos)
    {
        if (_oldPos != null)
        {
            var oldPos = (Vector2)_oldPos;
            _camPos.X +=
                (pos.X - oldPos.X)
                * Math.Max(0.01f, Math.Abs(_camPos.Z) / 1000)
                * MOVE_SENSITIVITY_X;
            _camPos.Y -=
                (pos.Y - oldPos.Y)
                * Math.Max(0.01f, Math.Abs(_camPos.Z) / 1000)
                * MOVE_SENSITIVITY_Y;

            ComputeView();
        }

        _oldPos = pos;
    }

    public void OnMouseMoveWithLeftClickHeld(Vector2 pos)
    {
        if (_oldAnglePos != null)
        {
            var oldAnglePos = (Vector2)_oldAnglePos;
            _angle.Y += (pos.X - oldAnglePos.X) * (360.0f / 1000) * TURN_SENSITIVITY_YAW;
            _angle.X += (pos.Y - oldAnglePos.Y) * (360.0f / 1000) * TURN_SENSITIVITY_PITCH;

            ComputeView();
        }

        _oldAnglePos = pos;
    }

    private const float DEG_TO_RAD = (float)(Math.PI / 180);

    private void ComputeView()
    {
        var view = Matrix4.Identity;
        view *= Matrix4.CreateRotationY(_angle.Y * DEG_TO_RAD);
        view *= Matrix4.CreateRotationX(_angle.X * DEG_TO_RAD);
        view *= Matrix4.CreateTranslation(_camPos);

        View = view;
    }
}
