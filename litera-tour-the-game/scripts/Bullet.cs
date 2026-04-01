using Godot;
using System;

public partial class Bullet : Area3D
{
    public float speed = 30f;
    [Export] public float BulletLifeTime = 10f;
    [Export] public int Damage = 1;

    public Vector3 direction;


    private float bulletTimer = 0;

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += direction * speed * (float)delta;

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