using Godot;
using System;

[GlobalClass]
public partial class EnemySpawnData : Resource
{
    [Export(PropertyHint.NodePathValidTypes, "Node3D")]
    public NodePath SpawnPointPath;
    
    [Export] public PackedScene EnemyType;
    //[Export] public Node3D SpawnPoint;
    [Export] public int MaxActiveEnemies = 10;
    [Export] public bool RespawnEnemy = false;
    [Export] public float RespawnDelay = 1f;
    [Export] public int TotalEnemySpawnLimit = -1;

    public int ActiveEnemies = 0;
    public int TotalEnemySpawned = 0;
}
