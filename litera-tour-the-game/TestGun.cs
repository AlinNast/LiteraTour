using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;

public partial class TestGun : Node3D
{

	public Random rng = new Random();
	[Export] public float bulletSpeed;
	[Export] public PackedScene bulletPrefab;
	[Export] public Marker3D shootPos;
	[Export] public int shotgunBullets;
	[Export] public int gunType;// Semi = 0, Auto = 1, Shotgun = 2
	[Export] public float fireRate;
	[Export] public float spread;

	private float shootTimer = 99999;
	private bool isShooting = false;
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(gunType == 0)
		{
			if(Input.IsActionJustPressed("shoot") && shootTimer > fireRate)
			{
				isShooting = true;
				Shoot();
				shootTimer = 0;
			}
		}
		else if(gunType == 1)
		{
			if(Input.IsActionPressed("shoot")&& shootTimer > fireRate)
			{
				isShooting = true;
				Shoot();
				shootTimer = 0;
			}
		}
		else if (gunType == 2)
		{
			if(Input.IsActionJustPressed("shoot")&& shootTimer > fireRate)
			{
				isShooting = true;
				int x = 0;
				while (x < shotgunBullets)
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
		float spreadAmount = (float)GD.RandRange(-spread, spread) * gunType;
		newBullet.direction = (forward+(right*spreadAmount)).Normalized();
		newBullet.speed = bulletSpeed; // Set speed
		GetTree().Root.AddChild(newBullet); // Add to world
	}
}
