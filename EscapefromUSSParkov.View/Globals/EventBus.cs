using System;
using Godot;

namespace EscapefromUSSParkov.View;

public partial class EventBus : Node
{

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

}
