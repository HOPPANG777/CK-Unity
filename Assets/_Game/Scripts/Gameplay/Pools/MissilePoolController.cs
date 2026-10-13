using UnityEngine;

public sealed class MissilePoolController : MonoBehaviour, IMissilePool
{
    // 비활성 루트로 저장한 미사일 프리팹.
    [SerializeField] private MissileController _prefab;
    // 미사일 부모는 플레이어 밖에 둔다.
    [SerializeField] private Transform _parent;
    [SerializeField] private Transform _launcher;
    // IScreenWorldBounds 구현 컴포넌트를 연결한다.
    [SerializeField] private MonoBehaviour _boundsProvider;
    // 부족하면 한 개씩 확장한다.
    [Min(1)][SerializeField] private int _initialCount = 50;
    [Min(1)][SerializeField] private int _damage = 1;

    private IScreenWorldBounds _bounds;
    private ComponentPool<MissileController> _pool;

    public int ActiveCount => _pool?.ActiveCount ?? 0;
    public int TotalCount => _pool?.TotalCount ?? 0;

    private void Update()
    {
        if (_pool == null)
        {
            TryInitialize();
        }
    }

    private bool TryInitialize()
    {
        _bounds = _boundsProvider as IScreenWorldBounds;
        if (_prefab == null || _prefab.gameObject.activeSelf || _parent == null ||
            _launcher == null || _bounds == null || _initialCount < 1)
        {
            Debug.LogError("[MissilePoolController] 비활성 미사일과 부모·발사자·화면 경계를 연결하세요.", this);
            enabled = false;
            return false;
        }

        if (!_bounds.TryGetBounds(out _))
        {
            return false;
        }

        try
        {
            _pool = new ComponentPool<MissileController>(Create, _initialCount);
        }
        catch (System.Exception exception)
        {
            Debug.LogError("[MissilePoolController] " + exception.Message, this);
            enabled = false;
            return false;
        }

        return true;
    }

    public bool TryFire(Vector2 worldPosition, Vector2 direction)
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f || (_pool == null && !TryInitialize()))
        {
            return false;
        }

        MissileController missile = _pool.Rent();
        if (missile.TryFire(worldPosition, direction))
        {
            return true;
        }

        _pool.Return(missile);
        return false;
    }

    public void StopAll()
    {
        _pool?.ReturnAll();
    }

    private MissileController Create()
    {
        MissileController missile = Instantiate(_prefab, _parent, false);
        if (!missile.Init(_bounds, _launcher))
        {
            Destroy(missile.gameObject);
            throw new System.InvalidOperationException("미사일 프리팹의 물리 설정을 확인하세요.");
        }

        missile.Hit += OnHit;
        missile.FlightEnded += OnFlightEnded;
        return missile;
    }

    private void OnHit(Collider2D collider)
    {
        collider.GetComponentInParent<IDamageable>()?.TakeDamage(_damage);
    }

    private void OnFlightEnded(MissileController missile)
    {
        _pool?.Return(missile);
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
