using Godot;

namespace EscapefromUSSParkov.View;

public abstract partial class ProjectileBase : Area2D
{
    [Export] private float _speed = 800f;
    [Export] private float _lifetimeSeconds = 3f;


    private Vector2 _direction;
    private float _timeAlive;

    // Called by the spawner right after Instantiate(), before AddChild() —
    // mirrors ProjectileSpawner.Shoot()'s bullet.Setup(GlobalPosition, GlobalRotation).
    public void Setup(Vector2 globalPosition, Vector2 direction, float speed, float lifetimeSeconds)
    {
        GlobalPosition = globalPosition;
        GlobalRotation = direction.Angle();
        _direction = direction;
        _speed = speed;
        _lifetimeSeconds = lifetimeSeconds;
    }

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;
    }

    // No matching -= in _ExitTree here, unlike the usual pairing rule: that
    // rule protects a longer-lived emitter (e.g. the EventBus autoload) from
    // holding a dangling delegate into a node that outlived its subscription.
    // AreaEntered/BodyEntered are signals this node emits on itself, handled
    // by a method also on itself — emitter and subscriber are freed together,
    // so there's no surviving object left to hold a stale reference.

    public override void _PhysicsProcess(double delta)
    {
        float deltaSeconds = (float)delta;

        Position += _direction * _speed * deltaSeconds;

        _timeAlive += deltaSeconds;
        if (_timeAlive >= _lifetimeSeconds)
        {
            QueueFree();
        }
    }

    private void OnAreaEntered(Area2D area) => OnHit(area);

    private void OnBodyEntered(Node2D body) => OnHit(body);

    protected abstract void OnHit(Node2D other);
}
