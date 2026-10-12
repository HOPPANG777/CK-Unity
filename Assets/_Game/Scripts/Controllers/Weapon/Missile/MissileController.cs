using System;

using UnityEngine;

public sealed class MissileController : MonoBehaviour, IPlayerFire
{
    [Header("Dependencies")]
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Transform _launcher;
    [SerializeField] private MonoBehaviour _boundsProvider;

    [Header("Flight")]
    [Min(0.01f)][SerializeField] private float _moveSpeed = 10.0f;
    [SerializeField] private LayerMask _hitLayers = 1;
    [Min(0.0f)][SerializeField] private float _outsidePadding;

    private MissileMove _move;

    private MissileCollision _collision;

    private IScreenWorldBounds _bounds;

    private bool _isFlying;

    private bool _isEnding;

    public bool IsInitialized
    {
        get
        {
            return _move != null;
        }
    }

    public bool IsFlying
    {
        get
        {
            return _isFlying;
        }
    }

    public Vector2 Direction
    {
        get
        {
            if (_move == null)
            {
                return Vector2.zero;
            }

            return _move.Direction;
        }
    }

    public bool CanFire
    {
        get
        {
            if (!enabled)
            {
                return false;
            }

            if (_isFlying)
            {
                return false;
            }

            if (_isEnding)
            {
                return false;
            }

            if (_move == null)
            {
                return false;
            }

            if (_launcher == null)
            {
                return false;
            }

            if (!_move.IsUsable)
            {
                return false;
            }

            if (transform.parent != null)
            {
                if (!transform.parent.gameObject.activeInHierarchy)
                {
                    return false;
                }
            }

            return TryCheckBounds(_move.Position, out _);
        }
    }

    public event Action<MissileController> FlightEnded;

    public event Action<Collider2D> Hit;

    public bool Init(IScreenWorldBounds bounds, Transform launcher)
    {
        if (this == null)
        {
            return false;
        }

        if (_isFlying)
        {
            return false;
        }

        if (_isEnding)
        {
            return false;
        }

        Rigidbody2D body = _body;
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        Collider2D collider = _collider;
        if (collider == null)
        {
            collider = GetComponent<Collider2D>();
        }

        MissileMove move = new MissileMove(body, collider, _moveSpeed);

        if (body == null)
        {
            return false;
        }

        if (body.gameObject != gameObject)
        {
            return false;
        }

        if (!move.IsUsable)
        {
            return false;
        }

        if (launcher == null)
        {
            return false;
        }

        if (transform.IsChildOf(launcher))
        {
            return false;
        }

        if (GetComponentsInChildren<Collider2D>(true).Length != 1)
        {
            return false;
        }

        if (_hitLayers.value == 0)
        {
            return false;
        }

        if (!MissileMove.IsFinite(_outsidePadding))
        {
            return false;
        }

        if (_outsidePadding < 0.0f)
        {
            return false;
        }

        if (!TryReadBounds(bounds, move.Position, _outsidePadding, out _))
        {
            return false;
        }

        if (_move != null)
        {
            _move.Stop();
        }

        _body = body;
        _collider = collider;
        _launcher = launcher;
        _bounds = bounds;
        _move = move;

        _collision = new MissileCollision(body, collider, launcher, _hitLayers);

        _move.Stop();

        return true;
    }

    public bool TryFire(Vector2 worldPosition, Vector2 direction)
    {
        if (!CanFire)
        {
            return false;
        }

        if (!MissileMove.TryNormalize(direction, out Vector2 normalized))
        {
            return false;
        }

        if (!TryCheckBounds(worldPosition, out bool inside))
        {
            return false;
        }

        if (!inside)
        {
            return false;
        }

        float stepDistance = _move.StepDistance;

        if (!MissileMove.IsFinite(stepDistance))
        {
            return false;
        }

        if (stepDistance <= 0.0f)
        {
            return false;
        }

        gameObject.SetActive(true);
        _move.Launch(worldPosition, normalized);

        _isFlying = true;
        return true;
    }

    private void Awake()
    {
        if (!IsInitialized)
        {
            Init(_boundsProvider as IScreenWorldBounds, _launcher);
        }
    }

    private void Update()
    {
        if (!_isFlying)
        {
            return;
        }

        if (Time.timeScale <= 0.0f)
        {
            return;
        }

        if (!_move.IsUsable)
        {
            EndFlight(true);
            return;
        }

        if (!_body.simulated)
        {
            EndFlight(true);
            return;
        }

        if (_launcher == null)
        {
            EndFlight(true);
            return;
        }

        if (!TryCheckBounds(_move.Position, out bool inside))
        {
            EndFlight(true);
            return;
        }

        if (!inside)
        {
            EndFlight(true);
        }
    }

    private void FixedUpdate()
    {
        Update();

        if (!_isFlying)
        {
            return;
        }

        if (Time.timeScale <= 0.0f)
        {
            return;
        }

        if (!MissileMove.IsFinite(_move.StepDistance))
        {
            EndFlight(true);
            return;
        }

        if (_move.StepDistance <= 0.0f)
        {
            EndFlight(true);
            return;
        }

        if (_collision.HasHitAlongStep(_move.Direction, _move.StepDistance))
        {
            // 빠른 미사일은 실제 접촉 전에 스윕 검사로 명중할 수 있다.
            if (Hit != null)
            {
                Hit(_collision.LastHit);
            }

            EndFlight(true);
            return;
        }

        _move.MaintainVelocity();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isFlying)
        {
            return;
        }

        if (!_collision.IsValidTarget(other))
        {
            return;
        }

        if (Hit != null)
        {
            Hit(other);
        }

        EndFlight(true);
    }

    private void OnDisable()
    {
        if (_move != null)
        {
            _move.Stop();
        }

        EndFlight(false);
    }

    private void OnDestroy()
    {
        if (_move != null)
        {
            _move.Stop();
        }

        FlightEnded = null;
        Hit = null;
    }

    private void EndFlight(bool deactivate)
    {
        if (!_isFlying)
        {
            return;
        }

        if (_isEnding)
        {
            return;
        }

        _isEnding = true;
        _isFlying = false;
        _move.Stop();

        try
        {
            if (deactivate)
            {
                gameObject.SetActive(false);
            }

            if (FlightEnded != null)
            {
                FlightEnded(this);
            }
        }
        finally
        {
            _isEnding = false;
        }
    }

    private bool TryCheckBounds(Vector2 position, out bool inside)
    {
        return TryReadBounds(_bounds, position, _outsidePadding, out inside);
    }

    private static bool TryReadBounds(IScreenWorldBounds bounds, Vector2 position,
        float padding, out bool inside)
    {
        inside = false;
        if (!MissileMove.IsFinite(position.x))
        {
            return false;
        }

        if (!MissileMove.IsFinite(position.y))
        {
            return false;
        }

        if (!MissileMove.IsFinite(padding))
        {
            return false;
        }

        if (padding < 0.0f)
        {
            return false;
        }

        if (bounds == null)
        {
            return false;
        }

        if (bounds is UnityEngine.Object)
        {
            UnityEngine.Object unityObject = (UnityEngine.Object)bounds;
            if (unityObject == null)
            {
                return false;
            }
        }

        if (!bounds.TryGetBounds(out ScreenWorldBoundsArea area))
        {
            return false;
        }

        if (!area.IsValid)
        {
            return false;
        }

        try
        {
            inside = area.IsInside(position, padding);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

}
