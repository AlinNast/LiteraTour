using Godot;
using System.Collections.Generic;

public partial class EnemyPool : Node3D
{
    [Export] public PackedScene EnemyScene;
    [Export] public int EnemyPoolSize = 10;

    private Queue<Enemy> pool = new Queue<Enemy>();

    public override void _Ready()
    {
        for (int i = 0; i < EnemyPoolSize; i++)
        {
            Enemy enemy = CreateEnemy();
            enemy.Visible = false;
            pool.Enqueue(enemy);
        }
    }

    private Enemy CreateEnemy()
    {
        Enemy enemy = EnemyScene.Instantiate<Enemy>();
        AddChild(enemy);
        enemy.Pool = this;
        enemy.SetPhysicsProcess(false);
        return enemy;
    }

    public Enemy SpawnEnemy(Vector3 position)
    {
        Enemy enemy = pool.Count > 0 ? pool.Dequeue() : CreateEnemy();

        enemy.GlobalPosition = position;
        enemy.enemyHomePosition = position;
        enemy.ResetEnemy();
        enemy.Visible = true;
        enemy.SetPhysicsProcess(true);

        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        enemy.GetNode<CollisionShape3D>("HitBox/CollisionShape3D").Disabled = true;
        enemy.Velocity = Vector3.Zero;
        enemy.GlobalPosition = new Vector3(9999, 9999, 9999); // move far away

        enemy.Visible = false;
        enemy.SetPhysicsProcess(false);
        pool.Enqueue(enemy);
    }

}
