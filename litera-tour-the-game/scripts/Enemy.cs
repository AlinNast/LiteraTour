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
	[Export] public float ChaseRange = 20f;
	[Export] public float RetargetTime = 2f;
	[Export] public float WaitTime = 1f;
	[Export] public float LookRotateSpeed = 5f;
	[Export] public PackedScene BrokenModel;
	[Export] public NavigationAgent3D navigationAgent;
	
	private int health;
	private bool IsPlayerInside = false;
	private Player targetPlayer = null;
	private float retargetTimer = 0f;
	private float waitTimer = 0f;

	private EnemyState currentState = EnemyState.IDLE;

    public override void _Ready()
    {
        health = MaxHealth;
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
			targetPlayer = FindClosestPlayer();
			//HandleChoosingTarget();
			return;
		}
		
		retargetTimer += (float)delta;
		
		if (retargetTimer >= RetargetTime)
		{
			targetPlayer = FindClosestPlayer();
			navigationAgent.TargetPosition = targetPlayer.Position;

			//HandleChoosingTarget();
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
		{
			currentState = EnemyState.CHASING;
		}
			

		// Idle animation
	}

	private void UpdateChasing(double delta)
	{
		if (targetPlayer == null)
		{
			targetPlayer = FindClosestPlayer();
			return;
		}

		if (IsPlayerInside)
		{
			OnHitPlayer();
			return;
		}

		if (targetPlayer == null)
			return;

		navigationAgent.TargetPosition = targetPlayer.GlobalPosition;
		
		// Apply gravity
		Vector3 gravity = GetGravity();
		Vector3 velocity = Velocity;
		velocity += gravity * (float)delta;

		// Nav mesh only apply horizontal movement to avoid conflicting with gravity system
		/*Vector3 nextPosition = navigationAgent.GetNextPathPosition();
		Vector3 currentPosition = GlobalTransform.Origin;
		Vector3 newVelocity = (nextPosition - currentPosition).Normalized() * MoveSpeed;
		
		velocity = velocity.MoveToward(newVelocity, 1);*/


		// Nav mesh only apply horizontal movement to avoid conflicting with gravity system
		Vector3 nextPosition = navigationAgent.GetNextPathPosition();
		Vector3 direction = nextPosition - GlobalPosition;
		direction.Y = 0; // no vertical movement as gravity will handle it

		Vector3 horizontalVelocity = direction.Normalized() * MoveSpeed;

		// Nav movement
		velocity.X = horizontalVelocity.X;
		velocity.Z = horizontalVelocity.Z;

		SmoothLookAt(targetPlayer.Position, LookRotateSpeed * (float)delta);

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

		// Fixed joit physics engine screaming error by disable Area3D moitoring (partial fix, joit still scream out error sometime but the game work fine)
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

	// TODO: Enemy attack and more player detection stuff

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

	private Player FindClosestPlayer()
	{
		var players = GetTree().GetNodesInGroup("players");
		Player closest = null;
		float closestDistance = float.MaxValue;

		foreach (Node node in players)
		{
			if (node is Player player)
			{
				float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closest = player;
					GD.Print("closest player position: " + closest.Position);
				}
			}
		}

		return closest;
	}
}