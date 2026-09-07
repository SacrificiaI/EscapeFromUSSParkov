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
        GetTree().CurrentScene.AddChild(projectile);
        projectile.Setup(position, direction, speed, lifetimeseconds);
    }
}
