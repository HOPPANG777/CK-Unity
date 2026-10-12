using UnityEngine;
using UnityPlayerInput = UnityEngine.InputSystem.PlayerInput;

public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("이 플레이어의 Dynamic 물리 본체이며 중력·감쇠 0, 회전 고정을 사용합니다.")]
    [SerializeField] private Rigidbody2D _body;
    [Tooltip("물리 본체에 연결된 활성 비트리거 충돌체입니다.")]
    [SerializeField] private Collider2D _collider;
    [Tooltip("기존 Player 맵을 소유하는 자신의 Unity PlayerInput입니다.")]
    [SerializeField] private UnityPlayerInput _playerInput;
    [Tooltip("물리 컴포넌트가 없는 자신의 직접 자식 총구입니다.")]
    [SerializeField] private Transform _muzzle;
    [Tooltip("선택적인 IPlayerFire 구현 컴포넌트이며 미사일을 연결할 수 있습니다.")]
    [SerializeField] private MonoBehaviour _fireReceiver;

    [Header("Movement / Aim")]
    [Tooltip("유한한 0 이상 최대 이동 속도이며 월드 단위/초입니다.")]
    [Min(0.0f)][SerializeField] private float _moveSpeed = 5.0f;
    [Tooltip("본체 중심에서 총구까지의 유한한 0 이상 월드 거리입니다.")]
    [Min(0.0f)][SerializeField] private float _muzzleDistance = 0.65f;

    [Header("Input Actions")]
    [Tooltip("Value/Vector2 이동 입력의 기존 map/action 경로입니다.")]
    [SerializeField] private string _moveActionPath = "Player/Move";
    [Tooltip("Button 발사 입력의 기존 map/action 경로입니다.")]
    [SerializeField] private string _fireActionPath = "Player/Attack";

    [Tooltip("자신의 본체 또는 자식에 연결한 기존 이동 컴포넌트입니다.")]
    [SerializeField] private PlayerMove _move;

    [Tooltip("자신의 본체 또는 자식에 연결한 기존 조준 컴포넌트입니다.")]
    [SerializeField] private PlayerAim _aim;

    [Tooltip("자신의 본체 또는 자식에 연결한 기존 발사 위임 컴포넌트입니다.")]
    [SerializeField] private PlayerFire _fire;

    private PlayerInput _input;
    private bool _isReady;
    public bool IsReady
    {
        get
        {
            return _isReady;
        }
    }
    public bool IsFireConnected
    {
        get
        {
            if (_fire == null)
            {
                return false;
            }

            return _fire.IsConnected;
        }
    }

    private void OnEnable()
    {
        if (_input == null)
        {
            if (!Ready())
            {
                Debug.LogError("[PlayerController] Inspector 참조와 이동·조준 설정을 확인하세요.", this);
                enabled = false;
                return;
            }
        }

        _isReady = _input.StartListening();
        if (!_isReady)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (!_isReady)
        {
            return;
        }

        if (Time.timeScale > 0.0f)
        {
            _aim.Refresh(_input.Move);
        }

        while (_input.TryConsumeFireRequest())
        {
            // 일시정지 중 입력은 버려 재개 후 발사가 몰리지 않게 한다.
            if (Time.timeScale <= 0.0f)
            {
                continue;
            }

            _fire.TryFire(_aim.WorldPosition, _aim.Direction);
        }
    }

    private void FixedUpdate()
    {
        if (!_isReady)
        {
            return;
        }

        _move.Move(_input.Move);
    }

    private void OnDisable()
    {
        _isReady = false;
        if (_input != null)
        {
            _input.StopListening();
        }

        if (_move != null)
        {
            _move.Stop();
        }
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (_input == null)
        {
            return;
        }

        OnDisable();
        if (hasFocus)
        {
            OnEnable();
        }
    }

    private bool Ready()
    {
        _isReady = false;
        if (_body == null)
        {
            return false;
        }

        if (_collider == null)
        {
            return false;
        }

        if (_playerInput == null)
        {
            return false;
        }

        if (_muzzle == null)
        {
            return false;
        }

        if (_move == null)
        {
            return false;
        }

        if (_aim == null)
        {
            return false;
        }

        if (_fire == null)
        {
            return false;
        }

        if (!IsNonNegativeFinite(_moveSpeed))
        {
            return false;
        }

        if (!IsNonNegativeFinite(_muzzleDistance))
        {
            return false;
        }

        _input = new PlayerInput(_playerInput, _moveActionPath, _fireActionPath);
        _move.Init(_body, _moveSpeed);
        _aim.Init(_muzzle, _muzzleDistance);
        _fire.Init(_fireReceiver as IPlayerFire, this);

        return true;
    }

    public void Init(IPlayerFire fireReceiver)
    {
        if (_fire == null)
        {
            return;
        }

        _fire.SetReceiver(fireReceiver);
    }

    private static bool IsNonNegativeFinite(float value)
    {
        if (value < 0.0f)
        {
            return false;
        }

        if (float.IsNaN(value))
        {
            return false;
        }

        return !float.IsInfinity(value);
    }
}
