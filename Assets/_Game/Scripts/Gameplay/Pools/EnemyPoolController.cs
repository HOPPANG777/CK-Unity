using UnityEngine;

public sealed class EnemyPoolController : MonoBehaviour, IEnemyPool
{
    // 비활성 루트로 저장한 적 프리팹.
    [SerializeField] private EnemyActor _prefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private Transform _target;
    // IDamageable 구현 컴포넌트를 연결한다.
    [SerializeField] private MonoBehaviour _contactReceiver;
    [Min(1)][SerializeField] private int _initialCount = 10;
    // 사망 연출 중인 적도 활성 수에 포함한다.
    [Range(1, 10)][SerializeField] private int _maxActive = 10;

    private ComponentPool<EnemyActor> _pool;
    private IDamageable _contactTarget;

    public int ActiveCount => _pool?.ActiveCount ?? 0;
    public int TotalCount => _pool?.TotalCount ?? 0;

    public bool TryInitialize()
    {
        if (!isActiveAndEnabled)
        {
            return false;
        }

        if (_pool != null)
        {
            return true;
        }

        _contactTarget = _contactReceiver as IDamageable;
        if (_prefab == null || !_prefab.IsReady || _prefab.gameObject.activeSelf ||
            _parent == null || _target == null || _contactTarget == null || _initialCount < 1 ||
            _maxActive < 1 || _maxActive > 10)
        {
            Debug.LogError("[EnemyPoolController] 비활성 적 프리팹과 부모·플레이어·피해 수신자를 연결하세요.", this);
            enabled = false;
            return false;
        }

        _pool = new ComponentPool<EnemyActor>(Create, _initialCount);
        return true;
    }

    public bool TrySpawn(Vector2 position, float speed)
    {
        if (!isActiveAndEnabled || _pool == null || ActiveCount >= _maxActive)
        {
            return false;
        }

        _pool.Rent().Spawn(position, speed);
        return true;
    }

    public void Return(EnemyActor enemy)
    {
        _pool?.Return(enemy);
    }

    public void StopAll()
    {
        _pool?.ReturnAll();
    }

    private EnemyActor Create()
    {
        EnemyActor enemy = Instantiate(_prefab, _parent, false);
        enemy.Init(this, _target, _contactTarget);
        return enemy;
    }

    private void OnDisable()
    {
        StopAll();
    }

    private void OnDestroy()
    {
        _pool?.Dispose();
    }
}
