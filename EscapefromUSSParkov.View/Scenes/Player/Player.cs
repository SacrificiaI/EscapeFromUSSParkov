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

    [Export] private Marker2D _frontArmPivot;
    [Export] private Sprite2D _frontArm;
    [Export] private Line2D _aimLine;

    // Camera limits that constrain the camera to the level bounds.
    [Export] private int _cameraLeft = -5000000;
    [Export] private int _cameraRight = 5000000;
    [Export] private int _cameraTop = -5000000;
    [Export] private int _cameraBottom = 5000000;

    private readonly PlayerMotion _player = new();
    private PlayerInput _input;

    private Vector2 _direction;

    // Left-facing rest pose, captured in _Ready() for ApplyFacing() to mirror.
    private bool _facingRight;
    private float _frontArmPivotRestX;
    private float _frontArmRestRotation;
    #endregion

    public override void _Ready()
    {
        SetLimits();

        // Sets initial position to the node's position in-engine
        _player.Position = SimVector.ToSim(Position);

        // Movement animation
        _sprite.Play("idle");

        _aimLine.AddPoint(Vector2.Zero);
        _aimLine.AddPoint(Vector2.Zero);

        _frontArmPivotRestX = _frontArmPivot.Position.X;
        _frontArmRestRotation = _frontArm.Rotation;
    }

    private void SetLimits()
    {
        _camera.LimitLeft = _cameraLeft;
        _camera.LimitRight = _cameraRight;
        _camera.LimitTop = _cameraTop;
        _camera.LimitBottom = _cameraBottom;
    }

    public override void _Process(double delta)
    {
        MoveIn2D();

        bool aiming = Input.IsActionPressed("aim");
        AnimateMoveSideways(aiming);

        _aimLine.Visible = aiming;
        if (aiming)
        {
            Vector2 mouseGlobalPosition = GetGlobalMousePosition();
            _aimLine.SetPointPosition(1, ToLocal(mouseGlobalPosition));
            _frontArmPivot.LookAt(mouseGlobalPosition);
            _facingRight = mouseGlobalPosition.X > GlobalPosition.X;
        }

        ApplyFacing();
    }

    // Mirrors the body and arm across x=0 when facing right.
    private void ApplyFacing()
    {
        _sprite.FlipH = _facingRight;

        // Hand-tuned: the shoulder isn't quite symmetric about x=0, so the
        // mirrored pivot needs a small nudge back onto it.
        const float frontArmFlipCorrectionX = 0.1f;
        _frontArmPivot.Position = _frontArmPivot.Position with
        {
            X = _facingRight ? -_frontArmPivotRestX + frontArmFlipCorrectionX : _frontArmPivotRestX,
        };

        _frontArm.FlipH = _facingRight;
        _frontArm.Rotation = _facingRight ? MirrorRotation(_frontArmRestRotation) : _frontArmRestRotation;
    }

    // Mirroring maps angle θ to (π - θ), so re-aligning to local +X needs -θ - π.
    private static float MirrorRotation(float restRotation)
    {
        return Mathf.Wrap(-restRotation - Mathf.Pi, -Mathf.Pi, Mathf.Pi);
    }

    private void MoveIn2D()
    {
        // Move inputs are stored to the private field, used in _PhysicsProcess()
        _input = new(
            SimVector.ToSim(
                Input.GetVector("move_left", "move_right", "move_up", "move_down")
            ));
    }

    private void AnimateMoveSideways(bool aiming)
    {
        // Gets velocity from the sim, used to determine sprite direction
        Vector2 velocity = SimVector.ToGodot(_player.Velocity);
        bool movingSideways = velocity.X != 0;
        if (movingSideways)
        {
            _sprite.Play("move_side");

            // While aiming, facing follows the mouse instead (set in _Process).
            if (!aiming)
            {
                _facingRight = velocity.X > 0;
            }
        }
        else
        {
            _sprite.Play("idle");
        }

        // Arm has no idle pose yet, so hide it outside aiming/movement.
        _frontArmPivot.Visible = aiming || movingSideways;
    }


    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Advances the Sim using the private input and physics timestep.
        // Mirrors the Sim position onto the Godot node.
        _player.Tick(_input, dt);
        Position = SimVector.ToGodot(_player.Position);
    }
}
