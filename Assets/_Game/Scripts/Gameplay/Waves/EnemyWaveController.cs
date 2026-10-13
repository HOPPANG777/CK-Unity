using System;
using UnityEngine;

public sealed class EnemyWaveController : MonoBehaviour
{
    // IEnemyPool 구현 컴포넌트를 연결한다.
    [SerializeField] private MonoBehaviour _poolProvider;
    [SerializeField] private Transform _player;
    [SerializeField] private ScreenWorldBoundsView _bounds;
    // 웨이브 순서대로 대열·적 수·속도를 지정한다.
    [SerializeField] private EnemyWave[] _waves;
    // 첫 웨이브와 웨이브 사이 대기 시간(초).
    [Min(0f)][SerializeField] private float _delay = 2f;
    [Min(0f)][SerializeField] private float _inset = 1f;
    // 벽·장애물 레이어만 지정한다.
    [SerializeField] private LayerMask _obstacles = 4;
    [Min(0f)][SerializeField] private float _clearance = 0.35f;

    private readonly Vector2[] _positions = new Vector2[10];
    private IEnemyPool _pool;
    private float _remaining;
    private bool _stopped;

    public int CurrentWave { get; private set; }
    public int WaveCount => _waves?.Length ?? 0;
    public int ActiveEnemies => _pool?.ActiveCount ?? 0;
    public float WaitRemaining => _remaining;
    public event Action Completed;

    private void Awake()
    {
        _pool = _poolProvider as IEnemyPool;
        if (_pool == null || _player == null || _bounds == null || WaveCount == 0)
        {
            Fail("플레이어·적 풀·화면 경계와 웨이브 목록을 연결하세요.");
            return;
        }

        for (int i = 0; i < _waves.Length; i++)
        {
            EnemyWave wave = _waves[i];
            if (wave == null || wave.Formation == null || wave.Count < 1 || wave.Count > 10 ||
                wave.Speed < 0f || !MissileMove.IsFinite(wave.Speed))
            {
                Fail("웨이브의 대열·적 수(1~10)·속도를 확인하세요.");
                return;
            }
        }

        _remaining = _delay;
    }

    private void Update()
    {
        if (_stopped || Time.timeScale <= 0f || !_pool.TryInitialize())
        {
            return;
        }

        if (ActiveEnemies > 0)
        {
            _remaining = _delay;
            return;
        }

        if (CurrentWave == WaveCount)
        {
            _stopped = true;
            Completed?.Invoke();
            return;
        }

        _remaining -= Time.deltaTime;
        if (_remaining > 0f || !_bounds.TryGetBounds(out ScreenWorldBoundsArea area))
        {
            return;
        }

        EnemyWave wave = _waves[CurrentWave];
        Vector2 player = _player.position;
        for (int i = 0; i < wave.Count; i++)
        {
            if (!wave.Formation.TryGetPosition(i, wave.Count, player, out Vector2 position) ||
                !TryPlace(position, player, area, out _positions[i]))
            {
                _remaining = _delay;
                Debug.LogWarning("[EnemyWaveController] 배치 공간이 부족합니다. 플레이어 위치나 대열 반경을 조정하세요.", this);
                return;
            }
        }

        for (int i = 0; i < wave.Count; i++)
        {
            if (!_pool.TrySpawn(_positions[i], wave.Speed))
            {
                Stop();
                Fail("적 풀의 활성 한도와 프리팹 설정을 확인하세요.");
                return;
            }
        }

        CurrentWave++;
    }

    private bool TryPlace(Vector2 desired, Vector2 player, ScreenWorldBoundsArea area, out Vector2 position)
    {
        position = default;
        if (!MissileMove.IsFinite(desired.x) || !MissileMove.IsFinite(desired.y))
        {
            return false;
        }

        float inset = Mathf.Min(_inset, Mathf.Min(area.Width, area.Height) * 0.49f);
        Vector2 clamped = area.ClampPosition(desired, inset);
        float separation = _clearance * 2f + 0.2f;
        for (int attempt = 0; attempt < 8; attempt++)
        {
            Vector2 candidate = Vector2.Lerp(clamped, area.Center, attempt / 8f);
            if ((candidate - player).sqrMagnitude < separation * separation ||
                Physics2D.OverlapCircle(candidate, _clearance, _obstacles) != null)
            {
                continue;
            }

            position = candidate;
            return true;
        }

        return false;
    }

    public void Stop()
    {
        _stopped = true;
        _pool?.StopAll();
    }

    private void Fail(string message)
    {
        Debug.LogError("[EnemyWaveController] " + message, this);
        _stopped = true;
        enabled = false;
    }
}
