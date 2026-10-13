using UnityEngine;

public sealed class PlayerMovementAnimation : MonoBehaviour
{
    private static readonly int WALK = Animator.StringToHash("Walk");
    private static readonly int IDLE = Animator.StringToHash("Idle");
    private static readonly int X = Animator.StringToHash("X");
    private static readonly int Y = Animator.StringToHash("Y");

    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private Animator _animator;

    private void LateUpdate()
    {
        if (_body == null || _animator == null || _animator.runtimeAnimatorController == null)
        {
            return;
        }

        Vector2 velocity = _body.linearVelocity;
        bool moving = velocity.sqrMagnitude > 0.01f;
        _animator.SetBool(WALK, moving);
        _animator.SetBool(IDLE, !moving);
        if (!moving)
        {
            return;
        }

        _animator.SetFloat(X, velocity.x);
        _animator.SetFloat(Y, velocity.y);
    }
}
