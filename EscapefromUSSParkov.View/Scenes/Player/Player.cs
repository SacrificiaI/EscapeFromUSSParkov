using System;
using EscapefromUSSParkov.Classes.Bridge;
using EscapefromUSSParkov.Sim.Player;
using Godot;

namespace EscapefromUSSParkov.View;

public sealed partial class Player : CharacterBody2D
{
    #region Properties
    // The front arm's texture is rotated 180deg; correct with a half turn.
    private const float FrontArmRotationCorrection = Mathf.Pi;

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
    private float _frontArmRestPositionX;
    private float _frontArmRestOffsetX;
    #endregion

    public override void _Ready()
    {
        SetLimits();

        // Sets initial position to the node's position in-engine
        _player.Position = SimVector.ToSim(Position);

        // Movement animation
        // _sprite.Play("idle");
        _sprite.Play("move_side");

        _aimLine.AddPoint(Vector2.Zero);
        _aimLine.AddPoint(Vector2.Zero);

        _frontArmPivotRestX = _frontArmPivot.Position.X;
        _frontArmRestRotation = _frontArm.Rotation + FrontArmRotationCorrection;
        _frontArmRestPositionX = _frontArm.Position.X;
        _frontArmRestOffsetX = _frontArm.Offset.X;
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
        else
        {
            // LookAt only runs while aiming, so reset to facing instead of
            // leaving the pivot frozen at the last aim angle.
            _frontArmPivot.Rotation = _facingRight ? 0f : Mathf.Pi - (Mathf.Pi / 2);
        }

        ApplyFacing();
    }

    // Mirrors the body and arm across x=0 when facing right.
    private void ApplyFacing()
    {
        // Body art now rests facing +X, so flip only when facing left.
        _sprite.FlipH = !_facingRight;

        float pivotX = _facingRight ? -_frontArmPivotRestX : _frontArmPivotRestX;
        _frontArmPivot.Position = new Vector2(pivotX, _frontArmPivot.Position.Y);

        _frontArm.FlipH = _facingRight;
        _frontArm.Rotation = _facingRight ? MirrorRotation(_frontArmRestRotation) : _frontArmRestRotation;

        // FlipH mirrors the drawn texture but not Position/Offset, so those
        // need mirroring by hand or the art swings way off during rotation.
        float armX = _facingRight ? -_frontArmRestPositionX : _frontArmRestPositionX;
        _frontArm.Position = new Vector2(armX, _frontArm.Position.Y);

        float armOffsetX = _facingRight ? -_frontArmRestOffsetX : _frontArmRestOffsetX;
        _frontArm.Offset = new Vector2(armOffsetX, _frontArm.Offset.Y);
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
        // else
        // {
        //     _sprite.Play("idle");
        // }

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
