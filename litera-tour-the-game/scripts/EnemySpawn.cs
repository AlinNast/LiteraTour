using Godot;
using System;

public partial class EnemySpawn : Node3D
{
	[Export] PackedScene EnemyType;
	[Export] Node3D SpawnLocation;
	[Export] public float SpawningTime = 1f;
	[Export] public int MaxEnemySpawn = 10;
	
	private float spawnTimer = 0f;
	private int aliveEnemies = 0;

	public override void _Process(double delta)
	{
		spawnTimer -= (float)delta;

        if (spawnTimer <= 0f && aliveEnemies < MaxEnemySpawn)
        {
            SpawnEnemy();
            spawnTimer = SpawningTime;
        }
	}

	private void SpawnEnemy()
	{
		Enemy enemy = EnemyType.Instantiate<Enemy>();
        AddChild(enemy);
		enemy.GlobalPosition = SpawnLocation.GlobalPosition;

		// Tracking alive enemies
        enemy.Died += OnEnemyDead;
        aliveEnemies++;
	}

	private void OnEnemyDead(Enemy enemy)
    {
        aliveEnemies--;
        enemy.Died -= OnEnemyDead;
    }
}
