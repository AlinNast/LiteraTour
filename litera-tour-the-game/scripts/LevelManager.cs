using Godot;
using System;
using System.Collections.Generic;



public partial class LevelManager
{
    
    





<<<<<<< Updated upstream
}
=======

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
		SpawnCamera();
	}




	

	private void SpawnPlayers()
	{
		for (int i = 0; i < currentPlayers.Count; i++)
		{
			currentPlayers[i].Position = new Vector3(0f, 1f, (float)i);
			currentPlayers[i].UpdateMapping(i); // Update the input mapping for each player based on their index
		}
	}

	private void SpawnCamera()
	{
		
		PackedScene cameraScene = GD.Load<PackedScene>("uid://bdxqld3fhk1qo");
		CameraNode camera = cameraScene.Instantiate() as CameraNode;
		camera.InitializeCamera(currentPlayers);
		AddChild(camera);

		
	}


}
>>>>>>> Stashed changes
