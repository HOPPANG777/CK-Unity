using System;
using UnityEngine;

public sealed class EnemyLayout
{
    private readonly int _count;

    private readonly int _columns;

    private readonly float _spacing;

    public EnemyLayout(int count, int columns, float spacing)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "적 수,열 수,간격을 확인하세요.");
        }

        if (columns < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(columns), "열 수는 1 이상이어야 합니다.");
        }

        if (spacing <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(spacing), "간격은 0보다 커야 합니다.");
        }

        if (!IsFinite(spacing))
        {
            throw new ArgumentOutOfRangeException(nameof(spacing), "간격은 유한한 값이어야 합니다.");
        }

        _count = count;
        _columns = columns;
        _spacing = spacing;
    }

    public bool TryGetPosition(int index, Vector2 center, out Vector2 position)
    {
        position = Vector2.zero;

        if (index < 0)
        {
            return false;
        }

        if (index >= _count)
        {
            return false;
        }

        if (!IsFinite(center.x))
        {
            return false;
        }

        if (!IsFinite(center.y))
        {
            return false;
        }

        int width = Math.Min(_count, _columns);
        int rows = (_count - 1) / _columns + 1;
        double x = center.x + (index % _columns - (width - 1) * 0.5) * _spacing;
        double y = center.y + (index / _columns - (rows - 1) * 0.5) * _spacing;

        if (Math.Abs(x) > float.MaxValue)
        {
            return false;
        }

        if (Math.Abs(y) > float.MaxValue)
        {
            return false;
        }

        position = new Vector2((float)x, (float)y);
        return true;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
