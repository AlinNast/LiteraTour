using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;

public partial class TestGun : Node3D
{
	public Random rng = new Random();

	[ExportGroup("Gun Setup")]
	[Export] public PackedScene bulletPrefab;
	[Export] public Marker3D shootPos;
	[ExportGroup("Upgradeable Data")]
	[Export] public bool isSemi, isAuto; // Weapon types
	[Export] public float bulletSpeed; // Speed of the bullet
	[Export] public float fireRate; // Time between shots
	[Export] public float spread; // The amount the bullets offset
	[Export] public int bulletAmount = 1; // How many bullets the weapons shoot

	private float shootTimer = 99999;
	private bool isShooting = false;


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("SwitchMode"))
		{
			isAuto = !isAuto;
			isSemi = !isSemi;
		}
		if(isSemi)
		{
			if(Input.IsActionJustPressed("shoot") && shootTimer > fireRate)
			{
				isShooting = true;
				int x = 0;
				while (x < bulletAmount)
				{
					Shoot();
					x++;
				}
				shootTimer = 0;
			}
		}
		else if(isAuto)
		{
			if(Input.IsActionPressed("shoot")&& shootTimer > fireRate)
			{
				isShooting = true;
				int x = 0;
				while (x < bulletAmount)
				{
					Shoot();
					x++;
				}
				shootTimer = 0;
			}
		}
		if(isShooting)
		{
			shootTimer += (float) delta;
			if(shootTimer > fireRate)
			{
				isShooting = false;
			}
		}
	}

	public void Shoot()
	{
		if (bulletPrefab == null) return;
		Bullet newBullet = bulletPrefab.Instantiate<Bullet>(); // Create bullet
		newBullet.GlobalTransform = shootPos.GlobalTransform; // Set position
		Vector3 forward = -shootPos.GlobalBasis.Z;
		Vector3 right = shootPos.GlobalBasis.X;
		float spreadAmount = (float)GD.RandRange(-spread, spread) * 2;
		newBullet.direction = (forward+(right*spreadAmount)).Normalized();
		newBullet.speed = bulletSpeed; // Set speed
		GetTree().Root.AddChild(newBullet); // Add to world
	}

	public void removeWeapon()
	{
		QueueFree();
	}
}
