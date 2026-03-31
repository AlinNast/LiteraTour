using Godot;
using System;

public partial class EnemySpawner : Node3D
{
    [Export] public EnemyPool Pool;
    [Export] public Node3D SpawnPointsParent;
    [Export] public float SpawningTime = 1f;
    [Export] public int MaxEnemiesAlive = 10;
    
    private Node3D[] spawnPoints;

    private float spawnTimer = 0f;
    private int aliveEnemies = 0;

    public override void _Ready()
    {
        spawnPoints = new Node3D[SpawnPointsParent.GetChildCount()];

        for (int i = 0; i < spawnPoints.Length; i++)
            spawnPoints[i] = SpawnPointsParent.GetChild<Node3D>(i);
    }


    public override void _Process(double delta)
    {
        spawnTimer -= (float)delta;

        if (spawnTimer <= 0f && aliveEnemies < MaxEnemiesAlive)
        {
            SpawnEnemy();
            spawnTimer = SpawningTime;
        }
    }

    private void SpawnEnemy()
    {
        var point = spawnPoints[GD.Randi() % spawnPoints.Length];

        Enemy enemy = Pool.SpawnEnemy(point.GlobalPosition);
        enemy.Pool = Pool;

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
