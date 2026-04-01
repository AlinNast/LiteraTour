using Godot;
using System;
using System.ComponentModel;
using System.Data;
using System.Xml;

public partial class Player : CharacterBody3D
{
	public enum PlayerState
	{
		NORMAL,
		HURT,
		DEAD
	}

	public int PlayerHealth;


	[Export] AnimationPlayer dead;

	[Export] public int PlayerMaxhealth = 2;
	[Export] public float PlayerMoveSpeed = 6f;
	[Export] public float RotationSpeed = 10f;
	[Export] public PackedScene BulletScene;

	public string moveLeft = "move_left";
	public string moveRight = "move_right";
	public string moveUp = "move_forward";
	public string moveDown = "move_backward";

	public PlayerState CurrentState = PlayerState.NORMAL;

	AudioStreamWav shootSound = GD.Load<AudioStreamWav>("res://sounds/gun-gunshot-01.wav");
	AudioStreamPlayer3D audioPlayer = new AudioStreamPlayer3D();




	private Marker3D gunPoint;

    public override void _Ready()
    {
		PlayerHealth = PlayerMaxhealth;



		AddChild(audioPlayer);

		AddToGroup("players");
        gunPoint = GetNode<Marker3D>("GunPoint");
    }

    public override void _PhysicsProcess(double delta)
    {
		HandlePlayerState(delta);
		
        // HandleMovement(delta);
		// HandleAiming(delta);
		// HandleShooting(delta);
		HandleAddtionalInputs(delta);
    }
	/// <summary>
	/// handle player movement
	/// </summary>
	/// <param name="delta"> delta is godot run time</param>
	


	//finite state
	public void HandlePlayerState(double delta)
	{
		switch (CurrentState)
		{
			case PlayerState.NORMAL:
				UpdateNormal(delta);
				break;
			case PlayerState.HURT:
				UpdateHurt(delta);
				break;
			case PlayerState.DEAD:
				UpdateDead();
				break;
		}
	}



	public void UpdateNormal(double delta)
	{
		HandleMovement(delta);
		HandleAiming(delta);
		HandleShooting(delta);
	}


	public void UpdateHurt(double delta)
	{
		// for polishing and stuff	
	}

	private bool isDead;
	public void UpdateDead()
	{
		if (isDead)
			return;
		
		isDead = true;
		GetNode<CollisionShape3D>("HitBox/CollisionShape3D").Disabled = true;

		if (CurrentState == PlayerState.DEAD)
			return;

		CurrentState = PlayerState.DEAD;

		//play dead animation, and stop moving
		dead.Play("dead");
		Velocity = Vector3.Zero;

		var hitbox = GetNode<Area3D>("HitBox");
		hitbox.SetDeferred("monitoring", false);
		hitbox.SetDeferred("monitorable", false);



		//for later respawn
		// if(revive or respawn condition)
		// {
		// 	CurrentState = PlayerState.NORMAL;
		// }
		
	}



	private void HandleMovement(double delta)
	{
		Vector2 input = Input.GetVector(moveLeft, moveRight, moveUp, moveDown);

		Vector3 direction = new Vector3(input.X, 0, input.Y);

		// Normalize diagonal movement
		if (direction.Length() > 1f)
			direction = direction.Normalized();

		//add gravity
		Vector3 velocity = Velocity;
		velocity.X = direction.X * PlayerMoveSpeed;
		velocity.Z = direction.Z * PlayerMoveSpeed;
		if(!IsOnFloor())
			velocity += GetGravity() * (float)delta;
		Velocity = velocity;

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


	public void TakeDamage(int damage)
	{
		if (CurrentState == PlayerState.DEAD)
			return;
			
		PlayerHealth -= damage;

		if (PlayerHealth <= 0)
			UpdateDead();
	}


	private void OnHitBoxAreaEntered(Area3D area)
	{

		if (CurrentState == PlayerState.DEAD)
			return;

		if (area is Bullet bullet)
		{
			TakeDamage(bullet.Damage);
		}
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