using UnityEngine;

public interface IEnemyPool
{
    int ActiveCount { get; }
    bool TryInitialize();
    bool TrySpawn(Vector2 position, float speed);
    void Return(EnemyActor enemy);
    void StopAll();
}
