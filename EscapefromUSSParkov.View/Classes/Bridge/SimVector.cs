using NumVector2 = System.Numerics.Vector2;

namespace EscapefromUSSParkov.Classes.Bridge;

// Needs to live in Project.View because it needs the Godot reference
// which is only available within this project.
public static class SimVector
{
    public static NumVector2 ToSim(Godot.Vector2 value)
        => new(value.X, value.Y);

    public static Godot.Vector2 ToGodot(NumVector2 value)
        => new(value.X, value.Y);
}
