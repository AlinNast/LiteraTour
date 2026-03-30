using Godot;
using System;

public partial class MainMenu : Control
{
	[Export]
	private Button startButton;
	[Export]
	private Button optionsButton;
	[Export]
	private Button quitButton;

	private Button currentHighlightedButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Connect button signals, basically thier behavior when pressed
		startButton.Pressed += () =>  GetTree().ChangeSceneToFile("Scenes/PlayerSelectionScreen.tscn");
		optionsButton.Pressed += () => GD.Print("Options button pressed");
		quitButton.Pressed += () => GetTree().Quit();

		// Set the initial highlighted button and hide the highlight for the other buttons
		currentHighlightedButton = startButton;
		optionsButton.GetChild<Control>(0).Visible = false; // the child of the button is the image with the button
		quitButton.GetChild<Control>(0).Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if( Input.IsActionJustPressed("MenuSelectUp") )
		{
			if (currentHighlightedButton == startButton)
			{
				//SwitchHighlightButton(quitButton);
			}
			else if (currentHighlightedButton == optionsButton)
			{
				SwitchHighlightButton(startButton);
			}
			else if (currentHighlightedButton == quitButton)
			{
				SwitchHighlightButton(optionsButton);
			}
			
		}
		if( Input.IsActionJustPressed("MenuSelectDown") )
		{
			if (currentHighlightedButton == startButton)
			{
				SwitchHighlightButton(optionsButton);
			}
			else if (currentHighlightedButton == optionsButton)
			{
				SwitchHighlightButton(quitButton);
			}
			else if (currentHighlightedButton == quitButton)
			{
				//SwitchHighlightButton(startButton);
			}
		}
		if( Input.IsActionJustPressed("MenuPressButton") )
		{
			currentHighlightedButton.EmitSignal("pressed");
		}
	}

	private void SwitchHighlightButton(Button newButton)
	{
		if (currentHighlightedButton != null)
		{
			currentHighlightedButton.GetChild<Control>(0).Visible = false; // Hide the highlight of the current button
		}

		currentHighlightedButton = newButton;
		currentHighlightedButton.GetChild<Control>(0).Visible = true; // Show the highlight of the new button
	}
}
