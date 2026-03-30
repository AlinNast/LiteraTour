using Godot;
using System;

public partial class EnemyPieces : Node3D
{
    [Export] public float intensity = 10.0f;
    [Export] public float clearTime = 5.0f;

    public override async void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is RigidBody3D piece)
            {
                if (piece.GetChildCount() > 0 && piece.GetChild(0) is Node3D firstChild)
                {
                    Vector3 direction = firstChild.Position * intensity;

                    // Convert global position to local offset
                    Vector3 offset = GlobalPosition - piece.GlobalPosition;

                    piece.ApplyImpulse(direction, offset);
                }
            }
        }

        await ToSignal(GetTree().CreateTimer(clearTime), SceneTreeTimer.SignalName.Timeout);
        QueueFree();
    }
}
