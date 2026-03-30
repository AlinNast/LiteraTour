using Godot;
using System;

public partial class PlayerSelectionScreen : Control
{
	[Export]
	private Button playerButton2;
	[Export]
	private Button playerButton3;
	[Export]
	private Button playerButton4;

	private Button currentHighlightedButton;

	private int numPlayers = 2;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Connect button signals, basically thier behavior when pressed
		playerButton2.Pressed += () =>  numPlayers = 2;
		playerButton3.Pressed += () =>  numPlayers = 3;
		playerButton4.Pressed += () =>  numPlayers = 4;

		// Set the initial highlighted button and hide the highlight for the other buttons
		currentHighlightedButton = playerButton2;
		playerButton3.GetChild<Control>(0).Visible = false; // the child of the button is the image with the button
		playerButton4.GetChild<Control>(0).Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// go back to main manu on press B
		if( Input.IsActionJustPressed("MenuPressBack") )
		{
			GetTree().ChangeSceneToFile("Scenes/MainMenu.tscn");
		}

		// navigate through the buttons with the arrow keys
		if( Input.IsActionJustPressed("MenuSelectLeft") )
		{
			if (currentHighlightedButton == playerButton2)
			{
				//SwitchHighlightButton(playerButton4);
			}
			else if (currentHighlightedButton == playerButton3)
			{
				SwitchHighlightButton(playerButton2);
			}
			else if (currentHighlightedButton == playerButton4)
			{
				SwitchHighlightButton(playerButton3);
			}	
		}
		if( Input.IsActionJustPressed("MenuSelectRight") )
		{
			if (currentHighlightedButton == playerButton2)
			{
				SwitchHighlightButton(playerButton3);
			}
			else if (currentHighlightedButton == playerButton3)
			{
				SwitchHighlightButton(playerButton4);
			}
			else if (currentHighlightedButton == playerButton4)
			{
				//SwitchHighlightButton(playerButton2);
			}
		}
		if( Input.IsActionJustPressed("MenuPressButton") )
		{
			currentHighlightedButton.EmitSignal("pressed");
			SelectNrOfPlayers();
		}
	}

	/// <summary>
	/// Switches the highlight to the new button and hides the highlight of the previous button
	/// </summary>
	/// <param name="newButton"></param>
	private void SwitchHighlightButton(Button newButton)
	{
		if (currentHighlightedButton != null)
		{
			currentHighlightedButton.GetChild<Control>(0).Visible = false; // Hide the highlight of the current button
		}

		currentHighlightedButton = newButton;
		currentHighlightedButton.GetChild<Control>(0).Visible = true; // Show the highlight of the new button
	}

	/// <summary>
	/// Place holder for a function that will swtich scene to the playable level and pass the number of players to that scene
	/// </summary> <param name="newButton"></param>
	private void SelectNrOfPlayers()
	{
		
		GD.Print("Selected number of players: " + numPlayers);
		// Then change to the next scene, such as the level selection screen
		GetTree().ChangeSceneToFile("Scenes/Test_level.tscn");
	}
}
