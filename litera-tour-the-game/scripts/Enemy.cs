using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	public EnemyPool Pool { get; set; }
	public event Action<Enemy> Died;
	public enum EnemyState
	{
		IDLE,
		CHASING,
		WAITING,
		RETURNING,
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

	[Export] public float ChaseCooldown = 4f;
	public Vector3 enemyHomePosition;
	private int health;
	private bool IsPlayerInside = false;
	private Player targetPlayer = null;
	private float retargetTimer = 0f;
	private float waitTimer = 0f;
	private float chaseCooldownTimer = 0f;

	private EnemyState currentState = EnemyState.IDLE;

    public override void _Ready()
    {
		GameEvents.Instance.PlayerMoved += OnPlayerMoved;
		GameEvents.Instance.PlayerFired += OnPlayerShoot;
		//enemyHomePosition = GlobalPosition;		
        health = MaxHealth;
		HandleChoosingTarget();
    }

    public override void _ExitTree()
    {
        GameEvents.Instance.PlayerMoved -= OnPlayerMoved;
		GameEvents.Instance.PlayerFired -= OnPlayerShoot;
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
			case EnemyState.RETURNING:
				UpdateReturning(delta);
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

		velocity.X = 0;
		velocity.Z = 0;

		Velocity = velocity;
		MoveAndSlide();

		if (IsAnyPlayerInRange())
			currentState = EnemyState.CHASING;
	}

	private void UpdateChasing(double delta)
	{
		if (targetPlayer == null)
		{
			currentState = EnemyState.RETURNING;
			return;
		}

		if (!IsTargetPlayerInRange())
		{
			//currentState = EnemyState.IDLE;

			chaseCooldownTimer += (float)delta;
			
			if (chaseCooldownTimer >= ChaseCooldown)
			{
				chaseCooldownTimer = 0f;
				currentState = EnemyState.RETURNING;
				return;
			}	
		}
		else
		{
			chaseCooldownTimer = 0f; // reset if player re enter enemy range
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
			velocity.X = move.X;
			velocity.Z = move.Z;
			//MoveAndSlide();
			//Chasing Animation

			SmoothLookAt(targetPlayer.GlobalPosition, (float)delta);
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

		velocity.X = 0;
		velocity.Z = 0;

		Velocity = velocity;
		MoveAndSlide();

		if (waitTimer <= 0f)
			currentState = EnemyState.CHASING;
		
		// Enemy waiting or attack cooldown animation
	}

	private void UpdateReturning(double delta)
	{
		if (IsTargetPlayerInRange())
		{
			chaseCooldownTimer = 0f;
			currentState = EnemyState.CHASING;
			return;
		}

		if (IsAnyPlayerInRange())
		{
			currentState = EnemyState.CHASING;
			return;
		}

		Vector3 gravity = GetGravity();
		Vector3 velocity = Velocity;
		velocity += gravity * (float)delta;

		Vector3 direction = enemyHomePosition - GlobalPosition;
		direction.Y = 0;

		if (direction.Length() < 0.2f)
		{
			currentState = EnemyState.IDLE;
			velocity.X = 0;
			velocity.Z = 0;
			MoveAndSlide();
			return;
		}
		
		Vector3 move = direction.Normalized() * MoveSpeed;
		velocity.X = move.X;
		velocity.Z = move.Z;
		
		Velocity = velocity;
		MoveAndSlide();

		SmoothLookAt(enemyHomePosition, (float)delta);
	}

	private bool isDead = false;

	private void UpdateDead()
	{
		if (isDead)
			return;
		
		isDead = true;
		GetNode<CollisionShape3D>("HitBox/CollisionShape3D").Disabled = true;

		GameEvents.Instance.PlayerMoved -= OnPlayerMoved;
		GameEvents.Instance.PlayerFired -= OnPlayerShoot;

		if (currentState == EnemyState.DEAD)
			return;

		currentState = EnemyState.DEAD;

		if (BrokenModel != null)
		{
			Node3D brokenModelInstantiate = BrokenModel.Instantiate<Node3D>();
        	GetParent().AddChild(brokenModelInstantiate);
        	brokenModelInstantiate.Transform = this.Transform;
		}

		// Notify enemy spawner script
		Died?.Invoke(this);

		// Fixed joit physics engine screaming error by disable Area3D moitoring
		var hitbox = GetNode<Area3D>("HitBox");
		hitbox.SetDeferred("monitoring", false);
		hitbox.SetDeferred("monitorable", false);

		// Return dead enemy to pool
		Pool.ReturnEnemy(this);
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
		
		chaseCooldownTimer = 0f;
		waitTimer = 0f;
		retargetTimer = 0f;

		Velocity = Vector3.Zero;
		targetPlayer = null;

		GameEvents.Instance.PlayerMoved += OnPlayerMoved;
		GameEvents.Instance.PlayerFired += OnPlayerShoot;
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
			bullet.QueueFree();
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
