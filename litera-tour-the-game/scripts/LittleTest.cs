using Godot;

public partial class LittleTest : RigidBody3D
{
    [Export] public PackedScene BrokenModel;
    [Export] public PackedScene GroundEffects;

    private void Break()
    {
        if (BrokenModel == null)
            return;

        Node3D brokenModelInstantiate = BrokenModel.Instantiate<Node3D>();
        GetParent().AddChild(brokenModelInstantiate);
        brokenModelInstantiate.Transform = this.Transform;

        Node3D GroundEff = GroundEffects.Instantiate<Node3D>();
        GetParent().AddChild(GroundEff);
        GroundEff.Transform = this.Transform;


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