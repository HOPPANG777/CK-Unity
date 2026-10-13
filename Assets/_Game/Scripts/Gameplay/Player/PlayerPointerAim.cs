using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerPointerAim : MonoBehaviour, IPointerAim
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _origin;
    [SerializeField] private Transform _muzzle;
    // 플레이어 중심에서 총구까지의 월드 거리.
    [Min(0f)][SerializeField] private float _muzzleDistance = 0.65f;

    public bool TryGetAim(out Vector2 position, out Vector2 direction)
    {
        position = default;
        direction = default;
        if (_camera == null || !_camera.isActiveAndEnabled || _origin == null || Mouse.current == null)
        {
            return false;
        }

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.forward, _origin.position);
        if (!plane.Raycast(ray, out float distance))
        {
            return false;
        }

        Vector2 offset = (Vector2)ray.GetPoint(distance) - (Vector2)_origin.position;
        if (offset.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        direction = offset.normalized;
        position = (Vector2)_origin.position + direction * Mathf.Min(_muzzleDistance, offset.magnitude);
        return true;
    }

    private void LateUpdate()
    {
        if (_muzzle == null || !TryGetAim(out Vector2 position, out Vector2 direction))
        {
            return;
        }

        _muzzle.position = new Vector3(position.x, position.y, _origin.position.z);
        _muzzle.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
    }
}
