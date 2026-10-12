using UnityEngine;

public interface IPlayerFire
{
    bool TryFire(Vector2 worldPosition, Vector2 direction);
}
