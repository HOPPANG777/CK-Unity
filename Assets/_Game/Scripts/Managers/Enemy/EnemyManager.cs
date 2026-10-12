using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyManager : MonoBehaviour
{
    [Header("Spawn / Dependencies")]
    [Min(0)][SerializeField] private int _enemyCount = 3;
    [SerializeField] private EnemyController _enemyPrefab = null;
    [SerializeField] private Transform _enemyParent = null;
    [SerializeField] private Transform _target = null;
    [SerializeField] private Transform _origin = null;

    [Header("Movement / Grid")]
    [Min(0.0f)][SerializeField] private float _moveSpeed = 3.0f;
    [Min(0.01f)][SerializeField] private float _spacing = 1.0f;
    [Min(1)][SerializeField] private int _columnCount = 3;

    private EnemyList _list;

    private EnemyLayout _layout;

    private Transform _placementOrigin;

    private bool _isBusy;

    private bool _isShuttingDown;

    private bool _reportedError;

    public bool IsInitialized { get; private set; }

    public int EnemyCount
    {
        get
        {
            if (_list == null)
            {
                return 0;
            }

            return _list.Count;
        }
    }

    public bool TryInitialize()
    {
        if (_isShuttingDown)
        {
            return false;
        }

        if (_isBusy)
        {
            return false;
        }

        if (IsInitialized)
        {
            return true;
        }

        if (!isActiveAndEnabled)
        {
            return ReportFailure("초기화 담당 Manager가 비활성입니다. GameObject와 컴포넌트를 켠 뒤 호출하세요.");
        }

        _isBusy = true;

        try
        {
            if (!TryPrepareEnemies())
            {
                return false;
            }

            IsInitialized = true;
            _reportedError = false;

            return true;
        }
        catch (Exception exception)
        {
            return ReportFailure($"초기화 실패: {exception}. 설정과 적의 초기화 콜백을 확인하세요.");
        }
        finally
        {
            if (!IsInitialized)
            {
                ReleaseOwnedEnemies();
            }

            _isBusy = false;
        }
    }

    public bool TryRedistribute()
    {
        if (_isBusy)
        {
            return false;
        }

        if (_isShuttingDown)
        {
            return false;
        }

        if (!IsInitialized)
        {
            return ReportFailure("재배치 준비가 안 됐습니다. 먼저 초기화하고 Manager와 기준점을 확인하세요.");
        }

        if (!isActiveAndEnabled)
        {
            return ReportFailure("재배치 준비가 안 됐습니다. 먼저 초기화하고 Manager와 기준점을 확인하세요.");
        }

        if (EnemyCount > 0)
        {
            if (_placementOrigin == null)
            {
                return ReportFailure("재배치 기준점이 없습니다. Inspector에서 Origin을 연결하세요.");
            }
        }

        _isBusy = true;

        try
        {
            if (!_list.TryRespawn(_layout, GetCenter()))
            {
                return ReportFailure(
                    "재배치 실패입니다. 파괴,이동된 적, disabled 컴포넌트, 부모,목표,좌표를 확인하세요. 대체 생성하지 않습니다.");
            }

            _reportedError = false;
            return true;
        }
        catch (Exception exception)
        {
            _list.DeactivateAll();
            return ReportFailure($"재배치 예외: {exception}. 적 콜백을 확인하세요. 생존 적은 중지했습니다.");
        }
        finally
        {
            _isBusy = false;
        }
    }

    public void Redistribute()
    {
        TryRedistribute();
    }

    private void Start()
    {
        TryInitialize();
    }

    private void OnDestroy()
    {
        _isShuttingDown = true;
        ReleaseOwnedEnemies();
    }

    private bool TryPrepareEnemies()
    {
        _layout = new EnemyLayout(_enemyCount, _columnCount, _spacing);

        if (!IsFinite(_moveSpeed))
        {
            return ReportFailure("설정 오류입니다. 수치,비활성 루트 프리팹,부모,목표,기준점을 확인하세요.");
        }

        if (_moveSpeed < 0.0f)
        {
            return ReportFailure("설정 오류입니다. 수치,비활성 루트 프리팹,부모,목표,기준점을 확인하세요.");
        }

        if (!HasValidSpawnReferences())
        {
            return ReportFailure("설정 오류입니다. 수치,비활성 루트 프리팹,부모,목표,기준점을 확인하세요.");
        }

        _placementOrigin = _origin;
        _list = new EnemyList(_enemyCount, _enemyParent, _target);

        if (!_list.TryCreate(_enemyPrefab, _moveSpeed))
        {
            return ReportFailure("적 준비 또는 배치가 실패했습니다. EnemyController 설정과 실행 조건을 확인하세요.");
        }

        if (!_list.TryRespawn(_layout, GetCenter()))
        {
            return ReportFailure("적 준비 또는 배치가 실패했습니다. EnemyController 설정과 실행 조건을 확인하세요.");
        }

        return true;
    }

    private void ReleaseOwnedEnemies()
    {
        IsInitialized = false;
        if (_list != null)
        {
            _list.Release();
        }

        _list = null;
        _layout = null;
        _placementOrigin = null;
    }

    private bool HasValidSpawnReferences()
    {
        if (_enemyCount == 0)
        {
            return true;
        }

        if (!HasValidPrefab())
        {
            return false;
        }

        if (!IsValidParent(_enemyParent))
        {
            return false;
        }

        if (_target == null)
        {
            return false;
        }

        if (_origin == null)
        {
            return false;
        }

        return HasValidPositions();
    }

    private bool HasValidPrefab()
    {
        if (_enemyPrefab == null)
        {
            return false;
        }

        if (!_enemyPrefab.enabled)
        {
            return false;
        }

        if (_enemyPrefab.gameObject.activeSelf)
        {
            return false;
        }

        if (_enemyPrefab.transform.parent != null)
        {
            return false;
        }

        if (_enemyPrefab.GetComponent<Rigidbody2D>() == null)
        {
            return false;
        }

        if (_enemyPrefab.GetComponent<SpriteRenderer>() == null)
        {
            return false;
        }

        return _enemyPrefab.GetComponentInChildren<Collider2D>(true) != null;
    }

    private bool HasValidPositions()
    {
        Vector2 center = _origin.position;

        for (int i = 0; i < _enemyCount; i++)
        {
            if (!_layout.TryGetPosition(i, center, out _))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool IsValidParent(Transform parent)
    {
        if (parent == null)
        {
            return false;
        }

        if (!parent.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (parent.lossyScale != Vector3.one)
        {
            return false;
        }

        float angle = Quaternion.Angle(parent.rotation, Quaternion.identity);
        if (!IsFinite(angle))
        {
            return false;
        }

        if (angle >= 0.001f)
        {
            return false;
        }

        return parent.GetComponentInParent<Rigidbody2D>() == null;
    }

    private Vector2 GetCenter()
    {
        if (EnemyCount == 0)
        {
            return Vector2.zero;
        }

        return _placementOrigin.position;
    }

    private bool ReportFailure(string cause)
    {
        if (_reportedError)
        {
            return false;
        }

        Debug.LogError($"[EnemyManager] {cause}", this);
        _reportedError = true;

        return false;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
