using Godot;

namespace EscapefromUSSParkov.View;

// Shared base for every projectile variant (Bullet and whatever follows it).
// Owns movement, lifetime, and hit detection; a subclass only decides what a
// hit actually does (damage, VFX, self-destruction, ...) via OnHit.
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

    public override void _ExitTree()
    {
        AreaEntered -= OnAreaEntered;
        BodyEntered -= OnBodyEntered;
    }

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

    // Subclass hook: react to whatever this projectile just touched (deal
    // damage, spawn an impact effect, QueueFree itself, ...). Movement,
    // lifetime, and signal wiring are handled here so a variant only needs
    // to implement this one thing.
    protected abstract void OnHit(Node2D other);
}
