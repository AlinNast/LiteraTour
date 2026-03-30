using Godot;

public partial class PrototypeCrate : RigidBody3D
{
    [Export] public PackedScene BrokenModel;

    private void Break()
    {
        if (BrokenModel == null)
            return;

        Node3D brokenModelInstantiate = BrokenModel.Instantiate<Node3D>();
        GetParent().AddChild(brokenModelInstantiate);
        brokenModelInstantiate.Transform = this.Transform;

        QueueFree();
    }

    private void OnAreaEntered(Area3D area)
    {
        if (area is Bullet bullet)
        {
            bullet.QueueFree();
            Break();
        }
    }
}