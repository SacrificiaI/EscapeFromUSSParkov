using System;
using Godot;

namespace EscapefromUSSParkov.View;

public partial class EventBus : Node
{


    public event Action<Vector2, Vector2, float, float, PackedScene> OnCreateBullet;

    public static EventBus Instance { get; private set; }

    public override void _EnterTree()
    {
        if (Instance is not null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public static void EmitOnCreateBullet(Vector2 position, Vector2 direction, float speed, float lifetimeSeconds, PackedScene projectileScene)
    {
        Instance.OnCreateBullet?.Invoke(position, direction, speed, lifetimeSeconds, projectileScene);
    }

}
