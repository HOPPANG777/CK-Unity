using UnityEngine;

public sealed class CircleFormation : MonoBehaviour, IEnemyFormation
{
    [Min(0.1f)][SerializeField] private float _radius = 3f;
    // 시작 각도(도).
    [SerializeField] private float _startAngle;

    public bool TryGetPosition(int index, int count, Vector2 player, out Vector2 position)
    {
        position = default;
        if (index < 0 || index >= count || _radius <= 0f)
        {
            return false;
        }

        float angle = _startAngle * Mathf.Deg2Rad + Mathf.PI * 2f * index / count;
        position = player + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
        return true;
    }
}
