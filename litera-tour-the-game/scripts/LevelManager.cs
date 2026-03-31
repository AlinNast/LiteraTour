using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class LevelManager : Node
{

    public static LevelManager Instance;

    public override void _Ready()
    {
        Instance = this;
    }


    public List<string> levelList = ["uid://b12vb63dc5058"];
	List<Player> currentPlayers = new List<Player>();


	PackedScene playerScene = GD.Load<PackedScene>("uid://c603fi583baqe");
	PackedScene cameraScene = GD.Load<PackedScene>("uid://bdxqld3fhk1qo");






	public void StartFirstLevel(int numberOfPlayers)
	{
		currentPlayers.Clear();
		currentPlayers.Capacity = numberOfPlayers;
		PackedScene currentlevel = GD.Load<PackedScene>(levelList[0]);
		Node3D levelInstance = currentlevel.Instantiate() as Node3D;

		AddChild(levelInstance);

		SpawnEnemiesFromLevel(levelInstance);


		for (int i = 0; i < numberOfPlayers; i++)
		{
			Player player = playerScene.Instantiate() as Player;
			AddChild(player);
			currentPlayers.Add(player);
		}


		SpawnPlayers();
		SpawnCamera();
	}




	

	private void SpawnPlayers()
	{
		for (int i = 0; i < currentPlayers.Count; i++)
		{
			currentPlayers[i].Position = new Vector3(0f, 1f, (float)i);
		}
	}

	private void SpawnCamera()
	{
		
		PackedScene cameraScene = GD.Load<PackedScene>("uid://bdxqld3fhk1qo");
		CameraNode camera = cameraScene.Instantiate() as CameraNode;
		camera.InitializeCamera(currentPlayers);
		AddChild(camera);

		
	}

	public void SpawnEnemiesFromLevel(Node3D level)
	{
		foreach (Node child in level.GetTree().GetNodesInGroup("enemy_spawn_data"))
		{
			if (child is EnemySpawnData enemySpawnData)
			{
				for (int i = 0; i < enemySpawnData.MaxActiveEnemies; i++)
				{
					Enemy enemy = enemySpawnData.EnemyType.Instantiate<Enemy>();
					AddChild(enemy);

					Vector3 offset = new Vector3(i * 1.5f, 0, 0);
					enemy.GlobalPosition = enemySpawnData.SpawnPoint.GlobalPosition + offset;

					enemySpawnData.ActiveEnemies++;
					enemy.Died += (enemy) => OnEnemyDied(enemySpawnData);
				}
			}
		}
	}

	private async void OnEnemyDied(EnemySpawnData enemySpawnData)
	{
		enemySpawnData.ActiveEnemies--;
	
		if (!enemySpawnData.RespawnEnemy)
			return;

		if (enemySpawnData.ActiveEnemies < enemySpawnData.MaxActiveEnemies)
		{
			await ToSignal(GetTree().CreateTimer(enemySpawnData.RespawnDelay), "timeout");
			SpawnOneEnemy(enemySpawnData);
		}
	}

	private void SpawnOneEnemy(EnemySpawnData enemySpawnData)
	{
		if (enemySpawnData.TotalEnemySpawnLimit != -1 && enemySpawnData.TotalEnemySpawned >= enemySpawnData.TotalEnemySpawnLimit)
			return;
		
		enemySpawnData.TotalEnemySpawned++;

		Enemy enemy = enemySpawnData.EnemyType.Instantiate<Enemy>();
		AddChild(enemy);

		enemy.GlobalPosition = enemySpawnData.SpawnPoint.GlobalPosition;
		enemySpawnData.ActiveEnemies++;

		enemy.Died += (enemy) => OnEnemyDied(enemySpawnData);
	}

}
