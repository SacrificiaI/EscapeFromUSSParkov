using Godot;

namespace EscapefromUSSParkov.View;

public abstract partial class BulletBase : ProjectileBase
{
    [Export] protected int Damage { get; set; } = 10;

    protected override void OnHit(Node2D other)
    {
        GD.Print($"PistolBullet hit {other.Name}!");
        QueueFree();
    }
}
