using Godot;
using System;

public partial class TestGun : Node3D
{
	[Export] public float bulletSpeed;
	[Export] public PackedScene bulletPrefab;
	[Export] public Marker3D shootPos;
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("shoot"))
		{
			Shoot(); // Pew Pew
		}
	}

	public void Shoot()
	{
		if (bulletPrefab == null) return;
		Bullet newBullet = bulletPrefab.Instantiate<Bullet>(); // Create bullet

		newBullet.GlobalTransform = shootPos.GlobalTransform; // Set position
		newBullet.speed = bulletSpeed; // Set speed
		newBullet.direction = -shootPos.GlobalBasis.Z; // Set shoot direction

		GetTree().Root.AddChild(newBullet); // Add to world
	}
}
