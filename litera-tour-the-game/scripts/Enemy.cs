using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	[Export] public float MoveSpeed = 3f;
	[Export] public int MaxHealth = 3;
	[Export] public float RetargetTime = 2f;
	[Export] public float LookRotateSpeed = 5f;
	[Export] public PackedScene BrokenModel;

	private int health;
	private bool isDead = false;
	private Player targetPlayer;
	private float retargetTimer = 0f;

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
			TakeDamage(bullet.Damage);
		}
	}
}
