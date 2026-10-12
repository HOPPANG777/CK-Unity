using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody2D _body;

    private float _moveSpeed;

    public void Init(Rigidbody2D body, float moveSpeed)
    {
        _body = body;
        _moveSpeed = moveSpeed;
    }

    public void Move(Vector2 input)
    {
        _body.linearVelocity = Vector2.ClampMagnitude(input, 1.0f) * _moveSpeed;
    }

    public void Stop()
    {
        if (_body == null)
        {
            return;
        }

        _body.linearVelocity = Vector2.zero;
    }
}
