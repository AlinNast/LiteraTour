using Godot;
using System;

public partial class Bullet : Area3D
{
	public float speed;
	public Vector3 direction;

	public override void _Process(double delta)
	{
		GlobalPosition += direction * speed * (float) delta;
	}

	public void Delete()
	{
		QueueFree();
	}
}
