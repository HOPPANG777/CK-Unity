using UnityEngine;

public interface IPointerAim
{
    bool TryGetAim(out Vector2 position, out Vector2 direction);
}
