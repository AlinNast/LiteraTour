using Godot;
using System;
using System.Collections.Generic;

public partial class GameInstance
{
	
	// ** TEMPORARY **
	// ** TEMPORARY **
	// ** TEMPORARY **
	// ** TEMPORARY **
	// ** TEMPORARY **
	// ** TEMPORARY **




	// ** TEMPORARY **
	// ** TEMPORARY **
	// ** TEMPORARY **
	// ** TEMPORARY **
	// ** TEMPORARY **
	// ** TEMPORARY **






	private int numberOfPlayers = 2;




	// private List<Player> availablePlayers = new List<Player>();
    private static GameInstance gameInstance = null;

    private GameInstance()
    {
    }

    public static GameInstance Instance
    {
        get
        {
            if (gameInstance == null)
            {
                gameInstance = new GameInstance();
            }
            return gameInstance;
        }
    }



	public void StartGame(int numberOfPlayers)
	{
		this.numberOfPlayers = numberOfPlayers;
		SpawnRandomLevel();
		GD.Print("Level initialized with " + numberOfPlayers);
	}



	private void SpawnRandomLevel()
	{
		InitializePlayers();
		//mainLevel.InitializeLevel(availablePlayers);
	}

	private void InitializePlayers()
	{
		//availablePlayers.Capacity = numberOfPlayers;
		for(int i = 0; i < numberOfPlayers; i++)
		{
			// Player player = new Player();
			// availablePlayers.Add(player);
		}
	}

}
