using UnityEngine;

public interface IEnemyControl
{
    bool IsInitialized { get; }

    bool IsRunning { get; }

    bool Init();

    void SetTarget(Transform target);

    bool SetMoveSpeed(float speed);

    bool SetColor(Color color);

    bool TryRespawn(Vector2 worldPosition);

    void Deactivate();
}
