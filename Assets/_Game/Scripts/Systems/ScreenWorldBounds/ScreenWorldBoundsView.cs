using UnityEngine;

public sealed class ScreenWorldBoundsView : MonoBehaviour, IScreenWorldBounds
{
    [Header("Camera / Plane")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _worldZ;

    [Header("Scene View")]
    [SerializeField] private Color _gizmoColor = Color.yellow;

    private ScreenWorldBounds _service;

    private Camera _boundCamera;

    private float _boundWorldZ;

    private string _reportedError;

    public bool IsValid
    {
        get
        {
            return TryGetBounds(out _);
        }
    }

    public bool Init(Camera camera, float worldZ = 0.0f)
    {
        _camera = camera;
        _worldZ = worldZ;

        return TryGetBounds(out _);
    }

    public bool TryGetBounds(out ScreenWorldBoundsArea bounds)
    {
        bounds = default;

        if (!Application.isPlaying)
        {
            return false;
        }

        if (!isActiveAndEnabled)
        {
            return false;
        }

        BindService();

        if (!_service.TryGetBounds(out bounds))
        {
            ReportError(_service.LastError);
            return false;
        }

        _reportedError = null;
        return true;
    }

    private void OnEnable()
    {
        TryGetBounds(out _);
    }

    private void OnDisable()
    {
        _service = null;
        _boundCamera = null;
        _reportedError = null;
    }

    private void OnDrawGizmos()
    {
        if (!TryGetGizmoBounds(out ScreenWorldBoundsArea bounds))
        {
            return;
        }

        DrawBoundsGizmo(bounds);
    }

    private void BindService()
    {
        if (IsServiceCurrent())
        {
            return;
        }

        if (_service == null)
        {
            _service = new ScreenWorldBounds(_camera, _worldZ);
        }
        else
        {
            _service.Init(_camera, _worldZ);
        }

        _boundCamera = _camera;
        _boundWorldZ = _worldZ;
    }

    private bool IsServiceCurrent()
    {
        if (_service == null)
        {
            return false;
        }

        if (_boundCamera != _camera)
        {
            return false;
        }

        return _boundWorldZ.Equals(_worldZ);
    }

    private void ReportError(string error)
    {
        if (error == _reportedError)
        {
            return;
        }

        Debug.LogWarning("[ScreenWorldBoundsView] " + error, this);
        _reportedError = error;
    }

    private bool TryGetGizmoBounds(out ScreenWorldBoundsArea bounds)
    {
        bounds = default;

        if (!Application.isPlaying)
        {
            return ScreenWorldBounds.TryCalculate(_camera, _worldZ, out bounds, out _);
        }

        if (_service == null)
        {
            return false;
        }

        return TryGetBounds(out bounds);
    }

    private void DrawBoundsGizmo(ScreenWorldBoundsArea bounds)
    {
        Color previousColor = Gizmos.color;
        Matrix4x4 previousMatrix = Gizmos.matrix;

        try
        {
            Gizmos.color = _gizmoColor;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireCube(bounds.Center, new Vector3(bounds.Width, bounds.Height, 0));
        }
        finally
        {
            Gizmos.color = previousColor;
            Gizmos.matrix = previousMatrix;
        }
    }
}
