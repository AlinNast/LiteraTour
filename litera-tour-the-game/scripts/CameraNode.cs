using Godot;
using System;
using System.Collections.Generic;

public partial class CameraNode : Node3D
{
	[Export]
	Node3D cameraDisplacement;
	[Export]
	Node3D cameraTilt;
	[Export]
	Node3D cameraZoom;


	private const float minCameraZoom = 15.0f;
	private const float maxCameraZoom = 30.0f;

	private const float maxXDistance = 70.0f;
	private const float maxZDistance = 40.0f;


	private float cameraHeight = 5f;
	private float cameraDisplacementValue = 5f;
	private float cameraTiltValue = Mathf.DegToRad(-70);

	//Node3D cameraZoom;

	List<Player> availablePlayers;

    public override void _Ready()
    {
		//cameraZoom = GetNode<Node3D>("CameraDisplacement/CameraTilt/CameraZoom");
		cameraDisplacement.Position = new Vector3(0f, 0f, cameraDisplacementValue);
		cameraTilt.Rotation = new Vector3(cameraTiltValue, 0f, 0f);
    }


    public override void _Process(double delta)
    {
        CameraPosition();
    }



	public void InitializeCamera(List<Player> players)
	{
		availablePlayers = players;
	}




	/// <summary>
	/// 
	/// Logic for the camera position and zoom based on player distance from each other.
	/// 
	/// </summary>
	public void CameraPosition()
	{
		// Find Rightmost, leftmost, furthest and closest players
		float leftmost = float.MaxValue;
		float rightmost = float.MinValue;
		float furthest = float.MinValue;
		float closest = float.MaxValue;

		for (int i = 0; i < availablePlayers.Count; i++)
		{
			if (availablePlayers[i].GlobalPosition.X < leftmost)
			{
				leftmost = availablePlayers[i].GlobalPosition.X;
			}

			if (availablePlayers[i].GlobalPosition.X > rightmost)
			{
				rightmost = availablePlayers[i].GlobalPosition.X;
			}

			if (availablePlayers[i].GlobalPosition.Z > furthest)
			{
				furthest = availablePlayers[i].GlobalPosition.Z;
			}

			if (availablePlayers[i].GlobalPosition.Z < closest)
			{
				closest = availablePlayers[i].GlobalPosition.Z;
			}

		}


		// Take the middle between the furthest players in X and Z positions respectively and use that as base for camera X and Z position
		float positionX = (leftmost + rightmost) * 0.5f;
		float positionZ = (closest + furthest) * 0.5f;

		GlobalPosition = new Vector3(positionX, 0f, positionZ);


		// Camera zoom

		float horizontalDistance = (rightmost - leftmost) / maxXDistance;
		float verticalDistance = (furthest - closest) / maxZDistance;

		float zoomValue = Mathf.Min(Mathf.Max(horizontalDistance, verticalDistance) * maxCameraZoom + minCameraZoom, maxCameraZoom);

		cameraZoom.Position = new Vector3(0f, 0f, zoomValue);

		GD.Print(rightmost - leftmost, " ", furthest - closest);

		

	}
}
