using UnityEngine;

public sealed class DotFormation : MonoBehaviour, IEnemyFormation
{
    // 생성 순간 플레이어 위치를 기준으로 방향·거리·반경을 적용한다.
    [SerializeField] private Vector2 _direction = Vector2.up;
    [Min(0f)][SerializeField] private float _distance = 3f;
    [Min(0f)][SerializeField] private float _radius = 1f;

    public bool TryGetPosition(int index, int count, Vector2 player, out Vector2 position)
    {
        position = default;
        if (index < 0 || index >= count || _distance < 0f || _radius < 0f)
        {
            return false;
        }

        position = player + _direction.normalized * _distance + Random.insideUnitCircle * _radius;
        return true;
    }
}
