using System.Numerics;
using EscapefromUSSParkov.Sim.Utils;

namespace EscapefromUSSParkov.Sim.Player;

public sealed class PlayerMotion
{
    public const float WalkingSpeed = 150.0f;

    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Rotation { get; set; }

    public Vector2 FacingDirection => SimMath.FromAngle(Rotation);

    public void Tick(PlayerInput input, float deltaSeconds)
    {
        Move(input, deltaSeconds);
    }

    private void Move(PlayerInput input, float deltaSeconds)
    {
        Velocity = input.MoveDirection * WalkingSpeed;
        Position += Velocity * deltaSeconds;
    }
}
