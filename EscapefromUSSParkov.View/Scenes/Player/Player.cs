using System;
using EscapefromUSSParkov.Classes.Bridge;
using EscapefromUSSParkov.Sim.Player;
using Godot;

namespace EscapefromUSSParkov.View;

public sealed partial class Player : CharacterBody2D
{
    #region Properties
    [Export] private AnimatedSprite2D _sprite;
    [Export] private CollisionShape2D _collision;
    [Export] private Camera2D _camera;

    [Export] private Line2D _aimLine;

    // Camera clamping variables to prevent camera from exiting level borders
    [Export] private int _cameraLeft = -5000000;
    [Export] private int _cameraRight = 5000000;
    [Export] private int _cameraTop = -5000000;
    [Export] private int _cameraBottom = 5000000;

    private readonly PlayerMotion _player = new();
    private PlayerInput _input;

    private Vector2 _direction;
    #endregion

    public override void _Ready()
    {
        SetLimits();
        _aimLine.AddPoint(Vector2.Zero);
        _aimLine.AddPoint(Vector2.Zero);
    }

    public override void _Process(double delta)
    {
        _input = new(
            SimVector.ToSim(
                Input.GetVector("move_left", "move_right", "move_up", "move_down")
            ));

        bool aiming = Input.IsActionPressed("aim");
        _aimLine.Visible = aiming;
        if (aiming)
        {
            _aimLine.SetPointPosition(1, ToLocal(GetGlobalMousePosition()));
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        _player.Tick(_input, dt);
        Position = SimVector.ToGodot(_player.Position);
    }

    private void SetLimits()
    {
        _camera.LimitLeft = _cameraLeft;
        _camera.LimitRight = _cameraRight;
        _camera.LimitTop = _cameraTop;
        _camera.LimitBottom = _cameraBottom;
    }
}
