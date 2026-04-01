using Godot;
using System;

public partial class TestLevel : Node3D
{
    [Export] public EnemySpawnData[] EnemySpawns;

    public override void _Ready()
    {
        StartSpawning();
    }

    public void StartSpawning()
    {
        foreach (var spawnEnemy in EnemySpawns)
        {
            Node3D point = GetNode<Node3D>(spawnEnemy.SpawnPointPath);

            for (int i = 0; i < spawnEnemy.MaxActiveEnemies; i++)
            {
                SpawnOneEnemy(spawnEnemy, point, i);
            }
        }
    }

    private void SpawnOneEnemy(EnemySpawnData enemySpawnData, Node3D point, int index = 0)
    {
        if (enemySpawnData.TotalEnemySpawnLimit != -1 && enemySpawnData.TotalEnemySpawned >= enemySpawnData.TotalEnemySpawnLimit)
            return;
        
        enemySpawnData.TotalEnemySpawned++;

        Enemy enemy = enemySpawnData.EnemyType.Instantiate<Enemy>();
        AddChild(enemy);

        Vector3 offset = new Vector3(index * 1.5f, 0, 0);
        enemy.GlobalPosition = point.GlobalPosition + offset;

        enemySpawnData.ActiveEnemies++;

        enemy.Died += (e) => OnEnemyDied(enemySpawnData, point);
    }

    private async void OnEnemyDied(EnemySpawnData enemySpawnData, Node3D point)
    {
        enemySpawnData.ActiveEnemies--;

        if (!enemySpawnData.RespawnEnemy)
            return;
        
        if (enemySpawnData.ActiveEnemies < enemySpawnData.MaxActiveEnemies)
        {
            await ToSignal(GetTree().CreateTimer(enemySpawnData.RespawnDelay), "timeout");
            SpawnOneEnemy(enemySpawnData, point);
        }
    }
}
