using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] public float PlayerMoveSpeed = 6f;
	[Export] public float RotationSpeed = 10f;
	[Export] public PackedScene BulletScene;

	private Marker3D gunPoint;
	private Vector3 lastPosition;

	public string moveLeft = "move_left";
	public string moveRight = "move_right";
	public string moveUp = "move_forward";
	public string moveDown = "move_backward";


    public override void _Ready()
    {
		AddToGroup("players");
        gunPoint = GetNode<Marker3D>("GunPoint");
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement(delta);

		if (GlobalPosition != lastPosition)
		{
			lastPosition = GlobalPosition;
		}
		
		HandleAiming(delta);
		HandleShooting(delta);
		HandlePlayerDead(delta);
    }
	
	/// <summary>
	/// handle player movement
	/// </summary>
	/// <param name="delta"> delta is godot run time</param>
	private void HandleMovement(double delta)
	{
		Vector2 input = Input.GetVector(moveLeft, moveRight, moveUp, moveDown);

		Vector3 direction = new Vector3(input.X, 0, input.Y);

		// Normalize diagonal movement
		if (direction.Length() > 1f)
			direction = direction.Normalized();

		Velocity = direction * PlayerMoveSpeed;
		MoveAndSlide();
	}

	private void HandleAiming(double delta)
	{
		Vector2 aimStick = new Vector2(Input.GetJoyAxis(0, JoyAxis.RightX), Input.GetJoyAxis(0, JoyAxis.RightY));

		if (aimStick.Length() > 0.2f)
		{
			// Controller aiming
			Vector3 aimDirection = new Vector3(aimStick.X, 0, aimStick.Y).Normalized();
			RotateToward(aimDirection, delta);
		}
	}

	private void RotateToward(Vector3 direction, double delta)
	{
		//Vector3 target = GlobalPosition + direction;
		Vector3 current = GlobalTransform.Basis.Z * -1f;

		Vector3 newDirection = current.Lerp(direction, (float)(RotationSpeed * delta)).Normalized();
		LookAt(GlobalPosition + newDirection, Vector3.Up);
	}

	private float shootCooldown = 0.15f;
	private float shootTimer = 0f;
	private void HandleShooting(double delta)
	{
		shootTimer -= (float)delta;

		if (!Input.IsActionPressed("shoot") || shootTimer > 0)
			return;
		
		shootTimer = shootCooldown;

		var bullet = BulletScene.Instantiate<Node3D>();
		GetTree().CurrentScene.AddChild(bullet);
		
		bullet.GlobalPosition = gunPoint.GlobalPosition;
		bullet.GlobalRotation = GlobalRotation;
        bullet.LookAt(bullet.GlobalPosition + -GlobalTransform.Basis.Z, Vector3.Up);
	}

	private void HandlePlayerDead(double delta)
	{
		//DEAD
	}
}