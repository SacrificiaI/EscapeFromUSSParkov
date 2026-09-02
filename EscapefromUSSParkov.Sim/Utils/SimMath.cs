using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace EscapefromUSSParkov.Sim.Utils;

public static class SimMath
{
    /// <summary>
    /// Creates a unit direction vector from an angle in radians.
    /// </summary>
    /// <param name="radians">The angle in radians.</param>
    /// <returns>The corresponding unit direction vector.</returns>
    public static Vector2 FromAngle(float radians) =>
        new(MathF.Cos(radians), MathF.Sin(radians));

    /// <summary>
    /// Returns the angle represented by a vector in radians.
    /// </summary>
    /// <param name="v">The direction vector.</param>
    /// <returns>The vector's angle in radians.</returns>
    public static float Angle(this Vector2 v) =>
        MathF.Atan2(v.Y, v.X);

    /// <summary>
    /// Returns a unit vector with the same direction as the input.
    /// Returns <see cref="Vector2.Zero"/> for a zero vector.
    /// </summary>
    /// <param name="v">The vector to normalize.</param>
    /// <returns>The normalized vector.</returns>
    public static Vector2 Normalized(this Vector2 v)
    {
        float lengthSquared = v.LengthSquared();
        if (lengthSquared == 0f) return Vector2.Zero;
        return v / MathF.Sqrt(lengthSquared);
    }

    /// <summary>
    /// Returns the unit direction from one position to another.
    /// Returns <see cref="Vector2.Zero"/> when both positions are equal.
    /// </summary>
    /// <param name="from">The starting position.</param>
    /// <param name="to">The destination position.</param>
    /// <returns>The direction from <paramref name="from"/> to <paramref name="to"/>.</returns>
    public static Vector2 DirectionTo(this Vector2 from, Vector2 to) =>
        (to - from).Normalized();

    /// <summary>
    /// Limits a vector's magnitude to the specified maximum.
    /// </summary>
    /// <param name="v">The vector to limit.</param>
    /// <param name="maxLength">The nonnegative maximum magnitude.</param>
    /// <returns>The original or length-limited vector.</returns>
    public static Vector2 LimitLength(this Vector2 v, float maxLength)
    {
        float length = v.Length();
        if (length > 0f && maxLength < length)
        {
            return v / length * maxLength;
        }

        return v;
    }

    /// <summary>
    /// Returns the signed shortest angular difference from one angle to another.
    /// </summary>
    /// <param name="from">The starting angle in radians.</param>
    /// <param name="to">The target angle in radians.</param>
    /// <returns>The shortest signed difference in radians.</returns>
    public static float AngleDifference(float from, float to)
    {
        float difference = (to - from) % MathF.Tau;
        return (2.0f * difference % MathF.Tau) - difference;
    }

    /// <summary>
    /// Moves an angle toward a target by at most the specified step.
    /// </summary>
    /// <param name="from">The starting angle in radians.</param>
    /// <param name="to">The target angle in radians.</param>
    /// <param name="delta">The nonnegative maximum step in radians.</param>
    /// <returns>The resulting angle in radians.</returns>
    public static float RotateToward(float from, float to, float delta)
    {
        float difference = AngleDifference(from, to);
        float absDifference = MathF.Abs(difference);
        return from
            + (Math.Clamp(delta, absDifference - MathF.PI, absDifference)
            * difference >= 0.0f ? 1.0f : -1.0f);
    }

    /// <summary>
    /// Linearly interpolates between two scalar values.
    /// Values outside the range [0, 1] extrapolate beyond the endpoints.
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="weight">The interpolation factor.</param>
    /// <returns>The interpolated value.</returns>
    public static float Lerp(float from, float to, float weight) =>
        from + ((to - from) * weight);
}
