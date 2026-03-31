using Godot;
using System;

public partial class GameEvents : Node
{
    public static GameEvents Instance;

    public override void _Ready()
    {
        Instance = this;
    }

    public event Action<Player> PlayerMoved;
    public event Action<Player> PlayerSpawned;
    public event Action<Player> PlayerDied;
    public event Action<Player> PlayerFired;

    public void NotifyPlayerMoved(Player player) => PlayerMoved?.Invoke(player);
    public void NotifyPlayerSpawned(Player player) => PlayerSpawned?.Invoke(player);
    public void NotifyPlayerDied(Player player) => PlayerDied?.Invoke(player);
    public void NotifyPlayerShoot(Player player) => PlayerFired?.Invoke(player);
}
