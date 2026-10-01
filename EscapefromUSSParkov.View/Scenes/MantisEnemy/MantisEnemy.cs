using Godot;

namespace EscapefromUSSParkov.View;

public partial class MantisEnemy : EnemyBase
{
    public bool LineOfSight { get; private set; }

    // Inherited chase, gated on line of sight rather than range alone.
    protected override void Chase(float deltaSeconds)
    {
        LineOfSight = HasLineOfSightToPlayer();
        if (!LineOfSight) return;

        base.Chase(deltaSeconds);
    }

    private bool HasLineOfSightToPlayer()
    {
        if (Player is null || !IsPlayerInDetectionRange()) return false;

        PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
        //PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(GlobalPosition, Player.GlobalPosition);
        //query.CollideWithAreas = true;

        PhysicsRayQueryParameters2D query = new PhysicsRayQueryParameters2D
        {
            From = GlobalPosition,
            To = Player.GlobalPosition,
            CollideWithAreas = true
        };

        Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
        return result.Count == 0 || result["collider"].As<Node>() == Player;
    }
}
