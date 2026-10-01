using Godot;
using System;
using EscapefromUSSParkov.Classes.Bridge;
using EscapefromUSSParkov.Sim.Enemy;

namespace EscapefromUSSParkov.View;

public partial class EnemyBase : Area2D
{
    [Export] public EnemyStats Stats { get; set; }
    [Export] public int CurrentHealth { get; set; }

    private readonly EnemyChase _chase = new();
    private Node2D _player;

    // Subclass-facing accessor for the tracked player node, e.g. Mantis's
    // line-of-sight check needs the player's position.
    protected Node2D Player => _player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        CurrentHealth = Stats.MaxHealth;
        _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        _chase.Position = SimVector.ToSim(GlobalPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
        Chase((float)delta);
    }

    //Most basic chase logic, can be overridden for more complex behavior
    protected virtual void Chase(float deltaSeconds)
    {
        if (_player is null) return;

        _chase.Tick(SimVector.ToSim(_player.GlobalPosition), Stats.MoveSpeed, Stats.DetectionRange, deltaSeconds);
        GlobalPosition = SimVector.ToGodot(_chase.Position);
    }

    // Same range rule Chase() uses internally, exposed so a subclass can
    // gate its own logic (Mantis's line-of-sight check) on it too.
    protected bool IsPlayerInDetectionRange()
    {
        return _player is not null && _chase.IsWithinRange(SimVector.ToSim(_player.GlobalPosition), Stats.DetectionRange);
    }

}
