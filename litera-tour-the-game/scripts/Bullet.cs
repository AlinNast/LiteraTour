using Godot;
using System;

public partial class Bullet : Area3D
{
    [Export] public float BulletSpeed = 30f;
    [Export] public float BulletLifeTime = 10f;
    [Export] public int Damage = 1;

    private float bulletTimer = 0;

    public override void _PhysicsProcess(double delta)
    {
        // Shoot bullet forward
        Vector3 forward = -Transform.Basis.Z; // local forward
        GlobalPosition += forward * BulletSpeed * (float)delta;

        // Bullet lifetime
        bulletTimer += (float)delta;
        
        if (bulletTimer >= BulletLifeTime)
            QueueFree();
    }

    private void OnBodyEntered(Node3D body)
    {
        QueueFree();
    }

}