using System;
using Godot;

namespace EscapefromUSSParkov.View;

public sealed partial class Player : CharacterBody2D
{
    #region Properties
    private const float _velocity = 150.0f;

    [Export] private AnimatedSprite2D _sprite;
    [Export] private CollisionShape2D _collision;
    [Export] private Camera2D _camera;

    // Camera clamping variables to prevent camera from exiting level borders
    [Export] private int _cameraLeft = -5000000;
    [Export] private int _cameraRight = 5000000;
    [Export] private int _cameraTop = -5000000;
    [Export] private int _cameraBottom = 5000000;

    private Vector2 _direction;
    #endregion

    public override void _Ready()
    {
        SetLimits();
    }

    public override void _Process(double delta)
    {
        _direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        Position += _velocity * _direction * dt;
        MoveAndSlide();
    }

    private void SetLimits()
    {
        _camera.LimitLeft = _cameraLeft;
        _camera.LimitRight = _cameraRight;
        _camera.LimitTop = _cameraTop;
        _camera.LimitBottom = _cameraBottom;
    }
}
