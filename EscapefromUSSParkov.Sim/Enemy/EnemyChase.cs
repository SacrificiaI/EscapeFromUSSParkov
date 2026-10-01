using System.Numerics;
using EscapefromUSSParkov.Sim.Utils;

namespace EscapefromUSSParkov.Sim.Enemy;

public sealed class EnemyChase
{
    public Vector2 Position { get; set; }

    public void Tick(Vector2 targetPosition, float moveSpeed, float detectionRange, float deltaSeconds)
    {
        if (!IsWithinRange(targetPosition, detectionRange)) return;

        Position += Position.DirectionTo(targetPosition) * moveSpeed * deltaSeconds;
    }

    public bool IsWithinRange(Vector2 targetPosition, float detectionRange)
    {
        return (targetPosition - Position).Length() <= detectionRange;
    }
}
