using Godot;
using System;

public partial class Bullet : Area3D
{
	public float speed;
	public Vector3 direction;
	private float dieTime;

	public override void _Process(double delta)
	{
		GlobalPosition += direction * speed * (float) delta;

		dieTime += (float) delta;
		if(dieTime > 10)
		{
			Delete();
		}
	}

	public void Delete()
	{
		QueueFree();
	}
}
