using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyController : MonoBehaviour, IEnemyControl
{
    [Header("Dependencies")]
    [Tooltip("EnemyController와 같은 오브젝트의 물리 본체입니다.")]
    [SerializeField] private Rigidbody2D _body;

    [Tooltip("이 적의 본체 또는 자식에 있는 표시 컴포넌트입니다.")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Tooltip("이 적의 본체 또는 자식에 추가한 이동 컴포넌트입니다.")]
    [SerializeField] private EnemyMove _move;

    [Tooltip("이 적의 본체 또는 자식에 추가한 충돌 처리 컴포넌트입니다.")]
    [SerializeField] private EnemyCollision _collision;

    private Transform _target;

    [Header("Movement")]
    [Tooltip("초당 월드 이동 거리이며, 0이면 정지합니다.")]
    [Min(0.0f)][SerializeField] private float _moveSpeed;

    private bool _isInitialized;

    private bool _isRunning;

    public bool IsInitialized
    {
        get
        {
            if (!_isInitialized)
            {
                return false;
            }

            if (_body == null)
            {
                return false;
            }

            if (_spriteRenderer == null)
            {
                return false;
            }

            if (_move == null)
            {
                return false;
            }

            return _collision != null;
        }
    }

    public bool IsRunning
    {
        get
        {
            if (!IsInitialized)
            {
                return false;
            }

            if (!_isRunning)
            {
                return false;
            }

            return isActiveAndEnabled;
        }
    }

    public bool Init()
    {
        if (IsInitialized)
        {
            return true;
        }

        if (!HasValidDependencies())
        {
            return FailInitialization();
        }

        if (_moveSpeed < 0.0f)
        {
            return FailInitialization();
        }

        if (!IsFinite(_moveSpeed))
        {
            return FailInitialization();
        }

        _move.Init(_body);
        _move.Stop();

        if (!HasValidPhysics())
        {
            return FailInitialization();
        }

        _collision.Init(this);
        _isInitialized = true;

        return true;
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        if (_target != null)
        {
            return;
        }

        StopMovement();
    }

    public bool SetMoveSpeed(float speed)
    {
        if (speed < 0.0f)
        {
            return false;
        }

        if (!IsFinite(speed))
        {
            return false;
        }

        _moveSpeed = speed;

        if (speed != 0.0f)
        {
            return true;
        }

        StopMovement();
        return true;
    }

    public bool SetColor(Color color)
    {
        if (!IsFinite(color.r))
        {
            return false;
        }

        if (!IsFinite(color.g))
        {
            return false;
        }

        if (!IsFinite(color.b))
        {
            return false;
        }

        if (!IsFinite(color.a))
        {
            return false;
        }

        if (!Init())
        {
            return false;
        }

        _spriteRenderer.color = color;
        return true;
    }

    public bool TryRespawn(Vector2 worldPosition)
    {
        if (!CanRespawn(worldPosition))
        {
            return false;
        }

        if (!Init())
        {
            return false;
        }

        ClearExecution();

        gameObject.SetActive(true);

        if (!isActiveAndEnabled)
        {
            return false;
        }

        _move.Relocate(worldPosition);
        _isRunning = true;

        return true;
    }

    public void Deactivate()
    {
        ClearExecution();

        if (!gameObject.activeSelf)
        {
            return;
        }

        gameObject.SetActive(false);
    }

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }

    private void FixedUpdate()
    {
        if (!IsRunning)
        {
            StopMovement();
            return;
        }

        if (Time.timeScale <= 0.0f)
        {
            StopMovement();
            return;
        }

        _move.Follow(_target, _moveSpeed, Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsRunning)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        _collision.Handle(other.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsRunning)
        {
            return;
        }

        _collision.Handle(other);
    }

    private void OnDisable()
    {
        ClearExecution();
    }

    private void ClearExecution()
    {
        _isRunning = false;
        StopMovement();
    }

    private void StopMovement()
    {
        if (_move == null)
        {
            return;
        }

        _move.Stop();
    }

    private bool HasValidDependencies()
    {
        if (_body == null)
        {
            return false;
        }

        if (_spriteRenderer == null)
        {
            return false;
        }

        if (_move == null)
        {
            return false;
        }

        if (_collision == null)
        {
            return false;
        }

        if (_body.gameObject != gameObject)
        {
            return false;
        }

        if (!_spriteRenderer.transform.IsChildOf(transform))
        {
            return false;
        }

        if (!_move.transform.IsChildOf(transform))
        {
            return false;
        }

        return _collision.transform.IsChildOf(transform);
    }

    private bool FailInitialization()
    {
        _isInitialized = false;
        _isRunning = false;

        return false;
    }

    private bool CanRespawn(Vector2 worldPosition)
    {
        if (!IsFinite(worldPosition.x))
        {
            return false;
        }

        if (!IsFinite(worldPosition.y))
        {
            return false;
        }

        if (!enabled)
        {
            return false;
        }

        Transform parent = transform.parent;
        if (parent == null)
        {
            return true;
        }

        return parent.gameObject.activeInHierarchy;
    }

    private bool HasValidPhysics()
    {
        if (!HasValidBody())
        {
            return false;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        bool hasCollider = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];

            if (collider.attachedRigidbody != _body)
            {
                continue;
            }

            if (!collider.enabled)
            {
                continue;
            }

            if (!IsColliderPrepared(collider.transform))
            {
                continue;
            }

            if (collider.isTrigger)
            {
                return false;
            }

            hasCollider = true;
        }

        return hasCollider;
    }

    private bool HasValidBody()
    {
        if (_body == null)
        {
            return false;
        }

        if (!_body.simulated)
        {
            return false;
        }

        if (_body.bodyType != RigidbodyType2D.Dynamic)
        {
            return false;
        }

        if (_body.gravityScale != 0.0f)
        {
            return false;
        }

        if (_body.linearDamping != 0.0f)
        {
            return false;
        }

        return _body.constraints == RigidbodyConstraints2D.FreezeRotation;
    }

    private bool IsColliderPrepared(Transform colliderTransform)
    {
        while (colliderTransform != transform)
        {
            if (!colliderTransform.gameObject.activeSelf)
            {
                return false;
            }

            colliderTransform = colliderTransform.parent;
        }

        return true;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
