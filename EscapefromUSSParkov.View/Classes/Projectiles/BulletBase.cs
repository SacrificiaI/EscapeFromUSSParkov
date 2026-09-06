using Godot;

namespace EscapefromUSSParkov.View;

// Shared base for straight-flying, single-hit projectiles (as opposed to a
// future projectile type that arcs, explodes, or pierces). ProjectileBase
// owns movement/lifetime/signal wiring; this layer fulfils its OnHit
// contract with the behavior every bullet variant shares, and exposes the
// bullet-specific data (currently just Damage) for those variants to tune.
public abstract partial class BulletBase : ProjectileBase
{
    // Protected + PascalCase: this is the subclass-facing export surface for
    // bullet variants, not private state (see godot-csharp-conventions.md).
    [Export] protected int Damage { get; private set; } = 10;

    // Bullets don't pierce, so touching anything ends their life. Damage
    // isn't applied yet — there's no health/damage component to receive it;
    // wiring Damage through to one is tracked by the brief's still-open
    // "extract one Sim rule, preferably accuracy/spread or damage" item.
    protected override void OnHit(Node2D other)
    {
        QueueFree();
    }
}
