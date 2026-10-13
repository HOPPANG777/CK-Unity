using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerPointerFire : MonoBehaviour, IPlayerFire
{
    [SerializeField] private PlayerController _player;
    // IPointerAim·IMissilePool 구현 컴포넌트를 연결한다.
    [SerializeField] private MonoBehaviour _aimProvider;
    [SerializeField] private MonoBehaviour _poolProvider;
    // 발사 간격(초).
    [Min(0.01f)][SerializeField] private float _interval = 0.1f;
    [SerializeField] private bool _holdToFire = true;

    private IPointerAim _aim;
    private IMissilePool _pool;
    private float _nextFireTime;

    private void Awake()
    {
        _aim = _aimProvider as IPointerAim;
        _pool = _poolProvider as IMissilePool;
        if (_player == null || _aim == null || _pool == null || _interval <= 0f)
        {
            Debug.LogError("[PlayerPointerFire] 플레이어·조준·미사일 풀과 발사 간격을 확인하세요.", this);
            enabled = false;
            return;
        }

        _player.Init(this);
    }

    public bool TryFire(Vector2 worldPosition, Vector2 direction)
    {
        if (!isActiveAndEnabled || !_player.IsReady || Time.timeScale <= 0f || Time.time < _nextFireTime)
        {
            return false;
        }

        // 기존 이동 방향 대신 발사 순간의 마우스 조준을 사용한다.
        if (!_aim.TryGetAim(out Vector2 position, out Vector2 aimDirection) || !_pool.TryFire(position, aimDirection))
        {
            return false;
        }

        _nextFireTime = Time.time + _interval;
        return true;
    }

    private void Update()
    {
        if (!_holdToFire || Mouse.current == null || !Mouse.current.leftButton.isPressed)
        {
            return;
        }

        TryFire(default, default);
    }

    private void OnDisable()
    {
        _nextFireTime = 0f;
    }
}
