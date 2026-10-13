using UnityEngine;

public sealed class GridFormation : MonoBehaviour, IEnemyFormation
{
    [Min(1)][SerializeField] private int _columns = 4;
    [Min(0.1f)][SerializeField] private float _spacing = 1.2f;
    // 플레이어에서 격자 중심까지의 오프셋.
    [SerializeField] private Vector2 _offset = new Vector2(0f, 2.5f);

    public bool TryGetPosition(int index, int count, Vector2 player, out Vector2 position)
    {
        position = default;
        if (index < 0 || index >= count || _columns < 1 || _spacing <= 0f)
        {
            return false;
        }

        int columns = Mathf.Min(count, _columns);
        int rows = (count - 1) / columns + 1;
        Vector2 grid = new Vector2(index % columns - (columns - 1) * 0.5f,
            index / columns - (rows - 1) * 0.5f);
        position = player + _offset + grid * _spacing;
        return true;
    }
}
