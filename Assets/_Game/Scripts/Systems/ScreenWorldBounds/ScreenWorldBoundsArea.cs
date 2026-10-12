using System;
using UnityEngine;

public readonly struct ScreenWorldBoundsArea
{
    private readonly Vector3 _min;

    private readonly Vector3 _max;

    public bool IsValid
    {
        get
        {
            if (!IsFinite(_min))
            {
                return false;
            }

            if (!IsFinite(_max))
            {
                return false;
            }

            if (_min.z != _max.z)
            {
                return false;
            }

            float width = _max.x - _min.x;
            float height = _max.y - _min.y;

            if (width <= 0.0f)
            {
                return false;
            }

            if (height <= 0.0f)
            {
                return false;
            }

            if (!IsFinite(width))
            {
                return false;
            }

            return IsFinite(height);
        }
    }

    public float MinX
    {
        get
        {
            return Min.x;
        }
    }

    public float MaxX
    {
        get
        {
            return Max.x;
        }
    }

    public float MinY
    {
        get
        {
            return Min.y;
        }
    }

    public float MaxY
    {
        get
        {
            return Max.y;
        }
    }

    public float Width
    {
        get
        {
            return MaxX - MinX;
        }
    }

    public float Height
    {
        get
        {
            return MaxY - MinY;
        }
    }

    public float WorldZ
    {
        get
        {
            return Min.z;
        }
    }

    public Vector3 Center
    {
        get
        {
            return Min + (Max - Min) * 0.5f;
        }
    }

    public Vector3 Min
    {
        get
        {
            EnsureValid();
            return _min;
        }
    }

    public Vector3 Max
    {
        get
        {
            EnsureValid();
            return _max;
        }
    }

    public ScreenWorldBoundsArea(Vector3 min, Vector3 max)
    {
        _min = min;
        _max = max;

        if (!IsValid)
        {
            throw new ArgumentException("유한한 동일 Z와 양수 폭,높이의 경계가 필요합니다.");
        }
    }

    public bool IsInside(Vector3 position, float margin = 0.0f)
    {
        GetRange(position, margin, out Vector2 min, out Vector2 max);

        if (position.x < min.x)
        {
            return false;
        }

        if (position.x > max.x)
        {
            return false;
        }

        if (position.y < min.y)
        {
            return false;
        }

        return position.y <= max.y;
    }

    public bool IsOutside(Vector3 position, float margin = 0.0f)
    {
        return !IsInside(position, margin);
    }

    public bool IsOutLeft(Vector3 position, float margin = 0.0f)
    {
        GetRange(position, margin, out Vector2 min, out _);
        return position.x < min.x;
    }

    public bool IsOutRight(Vector3 position, float margin = 0.0f)
    {
        GetRange(position, margin, out _, out Vector2 max);
        return position.x > max.x;
    }

    public bool IsOutBottom(Vector3 position, float margin = 0.0f)
    {
        GetRange(position, margin, out Vector2 min, out _);
        return position.y < min.y;
    }
    public bool IsOutTop(Vector3 position, float margin = 0.0f)
    {
        GetRange(position, margin, out _, out Vector2 max);
        return position.y > max.y;
    }
    public Vector3 ClampPosition(Vector3 position, float inset = 0.0f)
    {
        GetRange(position, -inset, out Vector2 min, out Vector2 max);

        return new Vector3(
            Mathf.Clamp(position.x, min.x, max.x),
            Mathf.Clamp(position.y, min.y, max.y),
            position.z);
    }

    public Vector3 GetRandomPositionOutside(float padding)
    {
        GetOutsideRange(padding, out Vector2 min, out Vector2 max);

        return SampleOutsidePosition(min, max);
    }

    private void GetOutsideRange(float padding, out Vector2 min, out Vector2 max)
    {
        GetRange(_min, padding, out min, out max);

        if (padding <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(padding), "float 좌표에서 표현 가능한 양수 외부 거리가 필요합니다.");
        }

        if (min.x >= _min.x)
        {
            throw new ArgumentOutOfRangeException(nameof(padding), "float 좌표에서 표현 가능한 양수 외부 거리가 필요합니다.");
        }

        if (min.y >= _min.y)
        {
            throw new ArgumentOutOfRangeException(nameof(padding), "float 좌표에서 표현 가능한 양수 외부 거리가 필요합니다.");
        }

        if (max.x <= _max.x)
        {
            throw new ArgumentOutOfRangeException(nameof(padding), "float 좌표에서 표현 가능한 양수 외부 거리가 필요합니다.");
        }

        if (max.y <= _max.y)
        {
            throw new ArgumentOutOfRangeException(nameof(padding), "float 좌표에서 표현 가능한 양수 외부 거리가 필요합니다.");
        }
    }

    private Vector3 SampleOutsidePosition(Vector2 min, Vector2 max)
    {
        int side = UnityEngine.Random.Range(0, 4);

        switch (side)
        {
            case 0:
                return new Vector3(min.x, UnityEngine.Random.Range(_min.y, _max.y), _min.z);
            case 1:
                return new Vector3(max.x, UnityEngine.Random.Range(_min.y, _max.y), _min.z);
            case 2:
                return new Vector3(UnityEngine.Random.Range(_min.x, _max.x), min.y, _min.z);
            default:
                return new Vector3(UnityEngine.Random.Range(_min.x, _max.x), max.y, _min.z);
        }
    }

    private void GetRange(Vector3 position, float margin, out Vector2 min, out Vector2 max)
    {
        EnsureValid();

        if (!IsFinite(position))
        {
            throw new ArgumentException("XYZ가 유한한 점이 필요합니다.", nameof(position));
        }

        min = new Vector2(_min.x - margin, _min.y - margin);
        max = new Vector2(_max.x + margin, _max.y + margin);

        if (!IsFinite(margin))
        {
            throw new ArgumentOutOfRangeException(nameof(margin), "유한하고 뒤집히지 않는 여백이 필요합니다.");
        }

        if (!IsValidRange(min, max))
        {
            throw new ArgumentOutOfRangeException(nameof(margin), "유한하고 뒤집히지 않는 여백이 필요합니다.");
        }
    }

    private static bool IsValidRange(Vector2 min, Vector2 max)
    {
        if (!IsFinite(min.x))
        {
            return false;
        }

        if (!IsFinite(min.y))
        {
            return false;
        }

        if (!IsFinite(max.x))
        {
            return false;
        }

        if (!IsFinite(max.y))
        {
            return false;
        }

        if (min.x > max.x)
        {
            return false;
        }

        if (min.y > max.y)
        {
            return false;
        }

        return IsFinite(max.x - min.x) && IsFinite(max.y - min.y);
    }

    private void EnsureValid()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException("TryGetBounds 성공 후 유효한 경계를 사용하세요.");
        }
    }

    internal static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    internal static bool IsFinite(Vector3 value)
    {
        if (!IsFinite(value.x))
        {
            return false;
        }

        if (!IsFinite(value.y))
        {
            return false;
        }

        return IsFinite(value.z);
    }
}
