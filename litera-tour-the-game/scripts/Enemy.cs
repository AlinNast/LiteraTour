using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	public event Action<Enemy> Died;
	public enum EnemyState
	{
		IDLE,
		CHASING,
		WAITING,
		DEAD
	}

	[ExportGroup("Enemy Setting")]
	[Export] public float MoveSpeed = 3f;
	[Export] public int MaxHealth = 1;
	[Export] public float ChaseRange = 10f;
	[Export] public float RetargetTime = 2f;
	[Export] public float WaitTime = 1f;
	[Export] public float LookRotateSpeed = 5f;
	[Export] public PackedScene BrokenModel;
	
	private int health;
	private bool IsPlayerInside = false;
	private Player targetPlayer = null;
	private float retargetTimer = 0f;
	private float waitTimer = 0f;

	private EnemyState currentState = EnemyState.IDLE;

    public override void _Ready()
    {
        health = MaxHealth;
		HandleChoosingTarget();
    }

    public override void _PhysicsProcess(double delta)
    {	
		HandleEnemyState(delta);

		// Debug
		/*if (currentState != EnemyState.DEAD)
			DebugDraw3D.DrawSphere(GlobalTransform.Origin, ChaseRange);*/

		if (currentState == EnemyState.DEAD)
			return;

		// Retarget logic.
        if (targetPlayer == null)
		{
			HandleChoosingTarget();
			return;
		}
		
		retargetTimer += (float)delta;
		
		if (retargetTimer >= RetargetTime)
		{
			HandleChoosingTarget();
			retargetTimer = 0f;
		}

		// Debug
		//GD.Print("Target: ", targetPlayer?.Name);
		//GD.Print("Distance: ", GlobalPosition.DistanceTo(targetPlayer.GlobalPosition));
		DebugDraw3D.DrawLine(GlobalPosition, targetPlayer.GlobalPosition, Colors.Red);
    }

	/// <summary>
	/// Enemy range
	/// </summary>
	/// <returns> distance to targetplayer true/false </returns>
	private bool IsTargetPlayerInRange()
	{
		if (targetPlayer == null)
			return false;
		
		return GlobalTransform.Origin.DistanceTo(targetPlayer.GlobalTransform.Origin) <= ChaseRange;
	}

	private bool IsAnyPlayerInRange()
	{
		var players = GetTree().GetNodesInGroup("players");

		foreach (Node node in players)
		{
			if (node is Player player)
			{
				if (GlobalPosition.DistanceTo(player.GlobalPosition) <= ChaseRange)
				{
					targetPlayer = player;
					return true;
				}
			}
		}

		return false;
	}

	public void HandleEnemyState(double delta)
	{
		switch (currentState)
		{
			case EnemyState.IDLE:
				UpdateIdle(delta);
				break;
			case EnemyState.CHASING:
				UpdateChasing(delta);
				break;
			case EnemyState.WAITING:
				UpdateWaiting(delta);
				break;
			case EnemyState.DEAD:
				UpdateDead();
				break;
		}
	}

	private void UpdateIdle(double delta)
	{
		// Apply gravity
		Vector3 gravity = GetGravity();
		Vector3 velocity = Velocity;
		velocity += gravity * (float)delta;
		
		if (IsOnFloor())
		{
			velocity.X = 0;
			velocity.Z = 0;
		}
		
		Velocity = velocity;
		MoveAndSlide();

		if (IsAnyPlayerInRange())
			currentState = EnemyState.CHASING;
	}

	private void UpdateChasing(double delta)
	{
		if (targetPlayer == null)
		{
			return;
		}

		if (IsPlayerInside)
		{
			OnHitPlayer();
			return;
		}

		if (targetPlayer == null)
			return;

		// Apply gravity
		Vector3 gravity = GetGravity();
		Vector3 velocity = Velocity;
		velocity += gravity * (float)delta;

		Vector3 direction = targetPlayer.GlobalTransform.Origin - GlobalTransform.Origin;
		direction.Y = 0;

		if (direction.Length() > 0.1f)
		{
			Vector3 move = direction.Normalized() * MoveSpeed;
			
			if (IsOnFloor())
			{
				velocity.X = move.X;
				velocity.Z = move.Z;

				//Chasing Animation
				SmoothLookAt(targetPlayer.GlobalPosition, (float)delta);
			}	
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void UpdateWaiting(double delta)
	{
		// Apply gravity
		Vector3 gravity = GetGravity();
		Vector3 velocity = Velocity;
		velocity += gravity * (float)delta;

		if (IsOnFloor())
		{
			velocity.X = 0;
			velocity.Z = 0;
		}
		
		Velocity = velocity;
		MoveAndSlide();

		if (waitTimer <= 0f)
			currentState = EnemyState.CHASING;
		
		// Enemy waiting or attack cooldown animation
	}

	private bool isDead = false;

	private void UpdateDead()
	{
		if (isDead)
			return;
		
		isDead = true;
		GetNode<CollisionShape3D>("HitBox/CollisionShape3D").Disabled = true;

		if (currentState == EnemyState.DEAD)
			return;

		currentState = EnemyState.DEAD;

		if (BrokenModel != null)
		{
			Node3D brokenModelInstantiate = BrokenModel.Instantiate<Node3D>();
        	GetParent().AddChild(brokenModelInstantiate);
        	brokenModelInstantiate.Transform = this.Transform;
		}

		// Notify LevelManager script
		Died?.Invoke(this);

		// Fixed joit physics engine screaming error by disable Area3D moitoring
		var hitbox = GetNode<Area3D>("HitBox");
		hitbox.SetDeferred("monitoring", false);
		hitbox.SetDeferred("monitorable", false);

		QueueFree();
	}

	public void ResetEnemy()
	{
		// Re enable Area3D moitoring
		var hitbox = GetNode<Area3D>("HitBox");
		hitbox.SetDeferred("monitoring", true);
		hitbox.SetDeferred("monitorable", true);
		
		GetNode<CollisionShape3D>("HitBox/CollisionShape3D").Disabled = false;

		isDead = false;
		health = MaxHealth;
		currentState = EnemyState.IDLE;
		
		waitTimer = 0f;
		retargetTimer = 0f;

		Velocity = Vector3.Zero;
		targetPlayer = null;
	}

	private void HandleChoosingTarget()
	{
		var players = GetTree().GetNodesInGroup("players");

		if (players.Count == 0)
		{
			targetPlayer = null;
			return;
		}	
		
		// Choose closest player.
		float closestDistance = float.MaxValue;

		foreach (Node node in players)
		{
			if (node is Player player)
			{
				float distance = GlobalPosition.DistanceTo(player.GlobalPosition);

				if (distance < closestDistance)
				{
					closestDistance = distance;
					targetPlayer = player;
				}
			}
		}

	}

	private void SmoothLookAt(Vector3 targetPosition, float delta)
	{
		Vector3 toTarget = (targetPosition - GlobalPosition).Normalized();

		// Remove vertical direction
		toTarget.Y = 0;

		Vector3 forward = -GlobalTransform.Basis.Z;

		Vector3 targetForward = forward.Lerp(toTarget, delta * LookRotateSpeed).Normalized();

		LookAt(GlobalPosition + targetForward, Vector3.Up);
	}

	public void TakeDamage(int damage)
	{
		if (currentState == EnemyState.DEAD)
			return;
			
		health -= damage;

		if (health <= 0)
			UpdateDead();
	}


	private void OnHitBoxAreaEntered(Area3D area)
	{

		if (currentState == EnemyState.DEAD)
			return;

		if (area is Bullet bullet)
		{
			TakeDamage(bullet.Damage);
		}
	}

	public void OnHitPlayer()
	{
		if (currentState == EnemyState.DEAD)
			return;
		
		if (currentState == EnemyState.WAITING)
			return;

	    //targetPlayer.TakeDamage(1);
		currentState = EnemyState.WAITING;
		waitTimer = WaitTime;
	}

	private void OnPlayerMoved(Player player)
	{
		if (currentState == EnemyState.DEAD)
			return;
		
		// If this player is closer than current target player then switch target to this player.
		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
		float currentDistance = targetPlayer == null? float.MaxValue : GlobalPosition.DistanceTo(targetPlayer.GlobalPosition);

		if (distance < currentDistance - 0.5f)
			targetPlayer = player;
	}

	private void OnPlayerShoot(Player player)
	{
		if (currentState == EnemyState.IDLE && targetPlayer != null)
			currentState = EnemyState.CHASING;
	}

	public void OnPlayerEnter()
	{
		IsPlayerInside = true;
	}

	public void OnPlayerExit()
	{
		IsPlayerInside = false;
	}
}