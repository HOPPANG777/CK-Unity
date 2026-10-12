using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyMove : MonoBehaviour
{
    private Rigidbody2D _body;

    public void Init(Rigidbody2D body)
    {
        _body = body;
    }

    public void Follow(Transform target, float speed, float fixedDeltaTime)
    {
        if (_body == null)
        {
            Stop();
            return;
        }

        if (target == null)
        {
            Stop();
            return;
        }

        if (speed == 0.0f)
        {
            Stop();
            return;
        }

        if (fixedDeltaTime <= 0.0f)
        {
            Stop();
            return;
        }

        Vector2 offset = (Vector2)target.position - _body.position;
        float distance = offset.magnitude;
        if (distance == 0.0f)
        {
            Stop();
            return;
        }

        if (float.IsNaN(distance))
        {
            Stop();
            return;
        }

        if (float.IsInfinity(distance))
        {
            Stop();
            return;
        }

        float arrivalSpeed = Mathf.Min(speed, distance / fixedDeltaTime);
        _body.linearVelocity = (offset / distance) * arrivalSpeed;
    }

    public void Stop()
    {
        if (_body == null)
        {
            return;
        }

        _body.linearVelocity = Vector2.zero;
        _body.angularVelocity = 0.0f;
    }

    public void Relocate(Vector2 worldPosition)
    {
        Stop();
        _body.position = worldPosition;
        _body.WakeUp();
    }
}
