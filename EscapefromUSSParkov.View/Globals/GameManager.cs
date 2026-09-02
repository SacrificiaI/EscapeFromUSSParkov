using System;
using Godot;

namespace EscapeFromUSSParkov.View;

public sealed partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public override void _EnterTree()
    {
        if (Instance is not null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }
}
