using System;
using Godot;

namespace EscapefromUSSParkov.View;

public sealed partial class Player : CharacterBody2D
{
    private const float _velocity = 300.0f;

    [Export] private AnimatedSprite2D _sprite;
    [Export] private CollisionShape2D _collision;
    [Export] private Camera2D _camera;

    // Camera clamp box, applied in SetLimits(). Defaults are a huge unbounded box (matching
    // Camera2D's own engine defaults) so a level that doesn't override these in the editor
    // gets no clamping at all, rather than an accidentally tiny or inverted one.
    [Export] private int _cameraLeft = -5000000;
    [Export] private int _cameraRight = 5000000;
    [Export] private int _cameraTop = -5000000;
    [Export] private int _cameraBottom = 5000000;

    private Vector2 _direction;

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
