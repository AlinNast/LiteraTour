using Godot;
using System;

public partial class FlameGround : Node3D
{
    [Export] public float clearTime = 10.0f;
	public override async void _Ready()
	{
		this.Position = new Vector3(0, 0, 0);
		await ToSignal(GetTree().CreateTimer(clearTime), SceneTreeTimer.SignalName.Timeout);
        QueueFree();
	}

}

