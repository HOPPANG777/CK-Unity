using UnityEngine;
using UnityEngine.InputSystem;
using UnityPlayerInput = UnityEngine.InputSystem.PlayerInput;

public class PlayerInput : IPlayerInput
{
    private readonly UnityPlayerInput _playerInput;
    private readonly string _movePath;
    private readonly string _firePath;
    private InputAction _moveAction;
    private InputAction _fireAction;
    private Vector2 _move;
    private int _pendingFireCount;
    private bool _isFireHeld;
    private bool _isListening;

    public PlayerInput(UnityPlayerInput playerInput, string movePath, string firePath)
    {
        _playerInput = playerInput;
        _movePath = movePath;
        _firePath = firePath;
    }

    public Vector2 Move
    {
        get
        {
            if (!CanRead(_moveAction))
            {
                return Vector2.zero;
            }

            return _move;
        }
    }

    public bool StartListening()
    {
        StopListening();

        if (_playerInput == null)
        {
            return false;
        }

        if (_playerInput.actions == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_movePath))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_firePath))
        {
            return false;
        }

        _moveAction = _playerInput.actions.FindAction(_movePath);
        _fireAction = _playerInput.actions.FindAction(_firePath);

        if (_moveAction == null)
        {
            return false;
        }

        if (_fireAction == null)
        {
            return false;
        }

        if (_moveAction.type != InputActionType.Value)
        {
            return false;
        }

        if (_moveAction.expectedControlType != "Vector2")
        {
            return false;
        }

        if (_fireAction.type != InputActionType.Button)
        {
            return false;
        }

        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMove;
        _fireAction.performed += OnFire;
        _fireAction.canceled += OnFireCanceled;

        _isListening = true;
        if (CanRead(_moveAction))
        {
            _move = _moveAction.ReadValue<Vector2>();
        }

        if (CanRead(_fireAction))
        {
            _isFireHeld = _fireAction.IsPressed();
        }
        return true;
    }

    public void StopListening()
    {
        if (_isListening)
        {
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;
            _fireAction.performed -= OnFire;
            _fireAction.canceled -= OnFireCanceled;
        }

        _isListening = false;
        _move = Vector2.zero;
        _pendingFireCount = 0;
        _isFireHeld = false;
    }

    public bool TryConsumeFireRequest()
    {
        if (!CanRead(_fireAction))
        {
            _pendingFireCount = 0;
        }

        if (_pendingFireCount == 0)
        {
            return false;
        }

        _pendingFireCount--;
        return true;
    }

    private bool CanRead(InputAction action)
    {
        if (!_isListening)
        {
            return false;
        }

        if (_playerInput == null)
        {
            return false;
        }

        if (!_playerInput.isActiveAndEnabled)
        {
            return false;
        }

        if (!_playerInput.inputIsActive)
        {
            return false;
        }

        if (action == null)
        {
            return false;
        }

        return action.enabled;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _move = Vector2.zero;
            return;
        }

        _move = context.ReadValue<Vector2>();
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        if (!CanRead(_fireAction))
        {
            return;
        }

        if (_isFireHeld)
        {
            return;
        }

        if (!context.ReadValueAsButton())
        {
            return;
        }

        _isFireHeld = true;
        _pendingFireCount++;
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        _isFireHeld = false;
    }
}
