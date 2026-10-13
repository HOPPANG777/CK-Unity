using UnityEngine;

public sealed class EnemyAnimation : MonoBehaviour
{
    private static readonly int X = Animator.StringToHash("X");
    private static readonly int Y = Animator.StringToHash("Y");
    private static readonly int WALK = Animator.StringToHash("Walk");
    private static readonly int IDLE = Animator.StringToHash("Idle");
    private static readonly int DIE = Animator.StringToHash("Die");
    private static readonly int DAMAGE = Animator.StringToHash("Damage");

    [SerializeField] private Animator _animator;
    // 이동·피격·사망은 별도 Animator Controller를 사용한다.
    [SerializeField] private RuntimeAnimatorController _movement;
    [SerializeField] private RuntimeAnimatorController _damage;
    // Die 상태는 반복을 끈다.
    [SerializeField] private RuntimeAnimatorController _death;

    private Vector2 _direction = Vector2.down;
    private bool _damageStarted;

    public bool IsReady => _animator != null && _movement != null && _damage != null && _death != null;
    public bool IsDeathComplete
    {
        get
        {
            if (!_animator.isInitialized || _animator.runtimeAnimatorController != _death)
            {
                return false;
            }

            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.shortNameHash == DIE && state.normalizedTime >= 1f;
        }
    }

    public void ResetAnimation()
    {
        _direction = Vector2.down;
        _damageStarted = false;
        Switch(_movement);
        _animator.SetBool(WALK, false);
        _animator.SetBool(IDLE, true);
        _animator.Play(IDLE, 0, 0f);
    }

    public void UpdateMotion(Vector2 velocity)
    {
        if (_animator.runtimeAnimatorController == _death)
        {
            return;
        }

        bool moving = velocity.sqrMagnitude > 0.01f;
        if (moving)
        {
            _direction = velocity.normalized;
        }

        _animator.SetFloat(X, _direction.x);
        _animator.SetFloat(Y, _direction.y);
        if (_animator.runtimeAnimatorController == _damage)
        {
            if (!_animator.isInitialized)
            {
                return;
            }

            int state = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            _damageStarted |= state == DAMAGE;
            if (!_damageStarted || state != IDLE)
            {
                return;
            }

            Switch(_movement);
        }

        _animator.SetBool(WALK, moving);
        _animator.SetBool(IDLE, !moving);
    }

    public void PlayDamage()
    {
        _damageStarted = false;
        Switch(_damage);
        _animator.Play(DAMAGE, 0, 0f);
    }

    public void PlayDeath()
    {
        Switch(_death);
        _animator.Play(DIE, 0, 0f);
    }

    private void Switch(RuntimeAnimatorController controller)
    {
        _animator.runtimeAnimatorController = controller;
        _animator.SetFloat(X, _direction.x);
        _animator.SetFloat(Y, _direction.y);
    }
}
