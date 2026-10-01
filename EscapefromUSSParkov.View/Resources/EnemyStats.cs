using Godot;
using System;

namespace EscapefromUSSParkov.View;

[GlobalClass]
public partial class EnemyStats : Resource
{
    [Export] public int CollisionDamage { get; set; } = 10;
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public float MoveSpeed { get; set; } = 20.0f;
    [Export] public float MeleeCooldown { get; set; } = 3.0f;
    [Export] public float RangedCooldown { get; set; } = 3.0f;
    [Export] public float DetectionRange { get; set; } = 100.0f;
}
