using Godot;

public partial class EnemyWaveSystem : Node
{
    [Export] public EnemySpawner Spawner;
    [Export] public float TimeBetweenWaves = 5f;

    private int currentWave = 0;
    private float waveTimer = 0f;

    public override void _Process(double delta)
    {
        waveTimer -= (float)delta;

        if (waveTimer <= 0f)
        {
            StartNextWave();
            waveTimer = TimeBetweenWaves;
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        Spawner.MaxEnemiesAlive = 5 + currentWave * 3;
        Spawner.SpawningTime = Mathf.Max(0.5f, 2f - currentWave * 0.1f);

        GD.Print($"Wave {currentWave} started!");
    }
}
