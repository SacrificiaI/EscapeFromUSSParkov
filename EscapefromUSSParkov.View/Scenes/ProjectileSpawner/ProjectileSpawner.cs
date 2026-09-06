using Godot;
using System;

namespace EscapefromUSSParkov.View;

public partial class ProjectileSpawner : Marker2D
{
    [Export] private PackedScene _projectileScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        EventBus.Instance.OnCreateBullet += Shoot;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnCreateBullet -= Shoot;
    }

    private void Shoot(Vector2 position, Vector2 direction, float speed, float lifetimeseconds, PackedScene projectileScene)
    {
        Node instance = projectileScene.Instantiate();
        if (instance is not ProjectileBase projectile)
        {
            GD.PrintErr($"ProjectileSpawner: {projectileScene} is not a ProjectileBase.");
            instance.QueueFree();
            return;
        }
        GD.Print("Player: shooting");

        // Parent first, then Setup — Setup writes GlobalPosition/GlobalRotation,
        // which only resolve correctly once the node has a real parent transform
        // to invert against. Doing it the other way round (as before) silently
        // wrote the world-space values into local space, then Godot re-applied
        // this spawner's own (scaled, offset, rotating) transform on top when it
        // was parented, landing the bullet far from where it was actually fired.
        AddChild(projectile);
        projectile.Setup(position, direction, speed, lifetimeseconds);
    }
}
