using Godot;
using System;

public partial class EnemySpawnData : Node
{
    [Export] public PackedScene EnemyType;
    [Export] public Node3D SpawnPoint;
    [Export] public int MaxActiveEnemies = 10;
    [Export] public bool RespawnEnemy = false;
    [Export] public float RespawnDelay = 1f;
    [Export] public int TotalEnemySpawnLimit = -1;


    public int ActiveEnemies = 0;
    public int TotalEnemySpawned = 0;

    public override void _Ready()
    {
        AddToGroup("enemy_spawn_data");
    }

}
