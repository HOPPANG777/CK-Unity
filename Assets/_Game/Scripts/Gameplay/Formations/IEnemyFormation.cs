using UnityEngine;

public interface IEnemyFormation
{
    bool TryGetPosition(int index, int count, Vector2 player, out Vector2 position);
}
