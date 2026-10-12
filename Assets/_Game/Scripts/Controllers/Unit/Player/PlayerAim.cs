using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private Transform _muzzle;
    private float _distance;
    private Vector2 _direction = Vector2.up;

    public void Init(Transform muzzle, float distance)
    {
        _muzzle = muzzle;
        _distance = distance;
        Refresh(Vector2.zero);
    }

    public Vector2 Direction
    {
        get
        {
            return _direction;
        }
    }
    public Vector2 WorldPosition
    {
        get
        {
            return _muzzle.position;
        }
    }

    public void Refresh(Vector2 input)
    {
        if (input.sqrMagnitude > 0.0f)
        {
            _direction = input / Mathf.Sqrt(input.sqrMagnitude);
        }

        Vector3 worldOffset = _direction * _distance;
        _muzzle.localPosition = _muzzle.parent.InverseTransformVector(worldOffset);
    }
}
