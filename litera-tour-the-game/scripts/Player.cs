using Godot;
using System;
using System.ComponentModel;
using System.Xml;

public partial class Player : CharacterBody3D
{
	[Export] public float PlayerMoveSpeed = 6f;
	[Export] public float RotationSpeed = 10f;
	[Export] public PackedScene BulletScene;

	public string moveLeft = "move_left";
	public string moveRight = "move_right";
	public string moveUp = "move_forward";
	public string moveDown = "move_backward";

	AudioStreamWav shootSound = GD.Load<AudioStreamWav>("res://sounds/gun-gunshot-01.wav");
	AudioStreamPlayer3D audioPlayer = new AudioStreamPlayer3D();




	private Marker3D gunPoint;

    public override void _Ready()
    {
		AddChild(audioPlayer);

		AddToGroup("players");
        gunPoint = GetNode<Marker3D>("GunPoint");
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement(delta);
		HandleAiming(delta);
		HandleShooting(delta);
		HandleAddtionalInputs(delta);
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
		Vector3 target = GlobalPosition + direction;
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

		audioPlayer.SetStream(shootSound);
		audioPlayer.Play();

		var bullet = BulletScene.Instantiate<Node3D>();
		AddSibling(bullet);
		
		bullet.GlobalPosition = gunPoint.GlobalPosition;
		bullet.GlobalRotation = GlobalRotation;
        bullet.LookAt(bullet.GlobalPosition + -GlobalTransform.Basis.Z, Vector3.Up);
	}

	private void HandleAddtionalInputs(double delta)
	{
		// Start button to go back to main menu
		if (Input.IsActionJustPressed("MenuHomeButton"))
		{
			GetTree().ChangeSceneToFile("uid://0586eog26yyw");
		}
	}

}