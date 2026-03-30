using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Enemy : CharacterBody3D
{
	public enum EnemyState
	{
		IDLE,
		CHASING,
		WAITING,
		DEAD
	}

	[ExportGroup("Enemy Setting")]
	[Export] public float MoveSpeed = 3f;
	[Export] public int MaxHealth = 3;
	[Export] public float ChaseRange = 10f;
	[Export] public float RetargetTime = 2f;
	[Export] public float LookRotateSpeed = 5f;
	[Export] public PackedScene BrokenModel;

	private int health;
	private bool isDead = false;
	private Player targetPlayer;
	private float retargetTimer = 0f;

	private EnemyState currentState = EnemyState.IDLE;

    public override void _Ready()
    {
        health = MaxHealth;
		HandleChoosingTarget();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (targetPlayer == null)
		{
			HandleChoosingTarget();
			return;
		}

		// Retarget.
		retargetTimer += (float)delta;
		
		if (retargetTimer >= RetargetTime)
		{
			HandleChoosingTarget();
			retargetTimer = 0f;
		}
		
		// Enemy move toward player.
		Vector3 direction = (targetPlayer.GlobalPosition - GlobalPosition).Normalized();
		Velocity = direction * MoveSpeed;
		MoveAndSlide();

		// Rotate enemy to face player.
		SmoothLookAt(targetPlayer.GlobalPosition, (float)delta);

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
				UpdateChasing(delta);
				break;
			case EnemyState.DEAD:
				UpdateDead(delta);
				break;
		}
	}

	private void UpdateIdle(double delta)
	{
		Velocity = Vector3.Down;
		MoveAndSlide();

		if (IsTargetPlayerInRange())
			currentState = EnemyState.CHASING;
	}

	private void UpdateChasing(double delta)
	{
		if (Velocity.Length() < 0.1f)
		{
			Velocity = Vector3.Down;
		}

		if (!IsTargetPlayerInRange())
		{
			currentState = EnemyState.IDLE;
			return;
		}

		/*if (IsPlayerInside)
		{
			OnHitPlayer();
			return;
		} */

		Vector3 direction = targetPlayer.GlobalTransform.Origin - GlobalTransform.Origin;
		direction.Y = 0;

		if (direction.Length() < 0.1f)
			return;
		Vector3 move = direction.Normalized() * MoveSpeed;
		Velocity = new Vector3(move.X, 0, move.Z);

		MoveAndSlide();
		//Chasing Animation
	}

	private void UpdateDead(double delta)
	{
		Velocity = Vector3.Zero;
		MoveAndSlide();
	}

	private void HandleChoosingTarget()
	{
		var players = GetTree().GetNodesInGroup("players");

		if (players.Count == 0)
			return;
		
		// Choose closet player.
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
		if (isDead)
			return;
			
		health -= damage;

		if (health <= 0)
			Die();
	}

	private void Die()
	{
		if (isDead)
			return;

		isDead = true;

		if (BrokenModel == null)
            return;

        Node3D brokenModelInstantiate = BrokenModel.Instantiate<Node3D>();
        GetParent().AddChild(brokenModelInstantiate);
        brokenModelInstantiate.Transform = this.Transform;

		QueueFree();
	}

	private void OnHitBoxAreaEntered(Area3D area)
	{
		if (area is Bullet bullet)
		{
			bullet.QueueFree();
			TakeDamage(bullet.Damage);
		}
	}
}
