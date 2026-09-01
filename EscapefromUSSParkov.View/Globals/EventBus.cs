using Godot;
using System;

namespace EscapefromUSSParkov;

public partial class EventBus : Node
{

    public static EventBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

}
