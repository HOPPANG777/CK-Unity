using UnityEngine;

public sealed class PlayerArenaBounds : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private ScreenWorldBoundsView _bounds;
    // 화면 가장자리에서 띄울 월드 거리.
    [Min(0f)][SerializeField] private float _inset = 0.8f;

    private void LateUpdate()
    {
        if (_body == null || _bounds == null || !_bounds.TryGetBounds(out ScreenWorldBoundsArea area))
        {
            return;
        }

        float inset = Mathf.Min(_inset, Mathf.Min(area.Width, area.Height) * 0.49f);
        Vector2 position = area.ClampPosition(_body.position, inset);
        if (position != _body.position)
        {
            _body.position = position;
        }
    }
}
