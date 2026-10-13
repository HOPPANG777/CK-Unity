using UnityEngine;

public sealed class EnemyActor : MonoBehaviour, IDamageable
{
    [SerializeField] private Rigidbody2D _body;
    // 사망 연출이 시작되면 충돌을 끈다.
    [SerializeField] private Collider2D _collider;
    [SerializeField] private EnemyMove _move;
    [SerializeField] private EnemyAnimation _animation;
    // 풀에서 꺼낼 때 복원할 체력.
    [Min(1)][SerializeField] private int _maxHealth = 1;
    [Min(1)][SerializeField] private int _contactDamage = 1;

    private IEnemyPool _pool;
    private IDamageable _contactTarget;
    private Transform _target;
    private float _speed;
    private int _health;

    public bool IsAlive { get; private set; }
    public bool IsReady => _body != null && _collider != null && _move != null &&
        _animation != null && _animation.IsReady && _maxHealth > 0;

    public void Init(IEnemyPool pool, Transform target, IDamageable contactTarget)
    {
        _pool = pool;
        _target = target;
        _contactTarget = contactTarget;
        _move.Init(_body);
    }

    public void Spawn(Vector2 position, float speed)
    {
        _health = _maxHealth;
        _speed = speed;
        IsAlive = true;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        _body.position = position;
        _body.simulated = true;
        _collider.enabled = true;
        gameObject.SetActive(true);
        _move.Stop();
        _animation.ResetAnimation();
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0)
        {
            return;
        }

        _health = Mathf.Max(0, _health - amount);
        if (_health > 0)
        {
            _animation.PlayDamage();
            return;
        }

        Die();
    }

    private void FixedUpdate()
    {
        if (!IsAlive || Time.timeScale <= 0f)
        {
            return;
        }

        _move.Follow(_target, _speed, Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        if (IsAlive)
        {
            _animation.UpdateMotion(_body.linearVelocity);
            return;
        }

        if (_pool != null && _animation.IsDeathComplete)
        {
            _pool.Return(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsAlive || _target == null || !collision.transform.IsChildOf(_target))
        {
            return;
        }

        _contactTarget.TakeDamage(_contactDamage);
        if (IsAlive)
        {
            Die();
        }
    }

    private void Die()
    {
        IsAlive = false;
        _move.Stop();
        _collider.enabled = false;
        _body.simulated = false;
        _animation.PlayDeath();
    }

    private void OnDisable()
    {
        IsAlive = false;
        if (_body != null)
        {
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
        }

        _pool?.Return(this);
    }
}
