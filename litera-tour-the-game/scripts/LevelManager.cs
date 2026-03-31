using Godot;
using System;
using System.Collections.Generic;

public partial class LevelManager : Node
{

    public static LevelManager Instance;

    public override void _Ready()
    {
        Instance = this;
    }



    public List<string> levelList = ["uid://b12vb63dc5058"];

	PackedScene playerScene = GD.Load<PackedScene>("uid://c603fi583baqe");
	List<Player> currentPlayers = new List<Player>();



	public void StartFirstLevel(int numberOfPlayers)
	{
		currentPlayers.Clear();
		currentPlayers.Capacity = numberOfPlayers;
		PackedScene currentlevel = GD.Load<PackedScene>(levelList[0]);
		Node3D levelInstance = currentlevel.Instantiate() as Node3D;

		AddChild(levelInstance);


		for (int i = 0; i < numberOfPlayers; i++)
		{
			Player player = playerScene.Instantiate() as Player;
			AddChild(player);
			currentPlayers.Add(player);
		}


		SpawnPlayers();
	}




	

	private void SpawnPlayers()
	{
		for (int i = 0; i < currentPlayers.Count; i++)
		{
			currentPlayers[i].Position = new Vector3(0f, 1f, (float)i);
		}


	}


}
