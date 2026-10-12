using UnityEngine;

public sealed class MissileMove
{
    private readonly Rigidbody2D _body;
    private readonly Collider2D _collider;
    private readonly float _speed;
    private Vector2 _direction;

    public MissileMove(Rigidbody2D body, Collider2D collider, float speed)
    {
        _body = body;
        _collider = collider;
        _speed = speed;
    }

    public bool IsUsable
    {
        get
        {
            if (_body == null)
            {
                return false;
            }

            if (_collider == null)
            {
                return false;
            }

            if (_collider.gameObject != _body.gameObject)
            {
                return false;
            }

            if (_collider.attachedRigidbody != _body)
            {
                return false;
            }

            if (!_collider.enabled)
            {
                return false;
            }

            if (!_collider.isTrigger)
            {
                return false;
            }

            if (_body.bodyType != RigidbodyType2D.Dynamic)
            {
                return false;
            }

            if (_body.collisionDetectionMode != CollisionDetectionMode2D.Continuous)
            {
                return false;
            }

            if (_body.gravityScale != 0f)
            {
                return false;
            }

            if (_body.linearDamping != 0f)
            {
                return false;
            }

            RigidbodyConstraints2D constraints = _body.constraints;

            if ((constraints & RigidbodyConstraints2D.FreezeRotation) == 0)
            {
                return false;
            }

            if ((constraints & RigidbodyConstraints2D.FreezePosition) != 0)
            {
                return false;
            }

            if (_body.includeLayers.value != 0)
            {
                return false;
            }

            if (_body.excludeLayers.value != 0)
            {
                return false;
            }

            if (_collider.includeLayers.value != 0)
            {
                return false;
            }

            if (_collider.excludeLayers.value != 0)
            {
                return false;
            }

            if (!IsFinite(_speed))
            {
                return false;
            }

            if (_speed <= 0f)
            {
                return false;
            }

            return _speed <= Physics2D.maxTranslationSpeed;
        }
    }

    public Vector2 Position
    {
        get
        {
            return _body.position;
        }
    }
    public Vector2 Direction
    {
        get
        {
            return _direction;
        }
    }
    public float StepDistance
    {
        get
        {
            return _speed * Time.fixedDeltaTime;
        }
    }

    public static bool TryNormalize(Vector2 direction, out Vector2 normalized)
    {
        normalized = Vector2.zero;
        if (!IsFinite(direction.x))
        {
            return false;
        }

        if (!IsFinite(direction.y))
        {
            return false;
        }

        float scale = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
        if (scale == 0.0f)
        {
            return false;
        }

        Vector2 scaled = new Vector2(direction.x / scale, direction.y / scale);
        normalized = scaled / scaled.magnitude;

        return true;
    }

    public void Launch(Vector2 position, Vector2 direction)
    {
        Stop();

        _body.simulated = true;
        _body.position = position;
        _direction = direction;

        MaintainVelocity();
    }

    public void MaintainVelocity()
    {
        _body.linearVelocity = _direction * _speed;
        _body.angularVelocity = 0.0f;
    }

    public void Stop()
    {
        _direction = Vector2.zero;

        if (_body != null)
        {
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0.0f;
            _body.simulated = false;
        }
    }
    public static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

}
