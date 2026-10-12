using System.Collections.Generic;
using UnityEngine;

public sealed class MissileCollision
{
    private readonly Rigidbody2D _body;
    private readonly Collider2D _collider;
    private readonly Transform _launcher;
    private readonly int _hitLayers;
    private readonly List<RaycastHit2D> _results = new List<RaycastHit2D>(16);
    private ContactFilter2D _filter;

    public Collider2D LastHit { get; private set; }

    public MissileCollision(
        Rigidbody2D body,
        Collider2D collider,
        Transform launcher,
        LayerMask hitLayers)
    {
        _body = body;
        _collider = collider;
        _launcher = launcher;
        _hitLayers = hitLayers.value;
        _filter.useTriggers = true;
    }

    public bool IsValidTarget(Collider2D other)
    {
        if (_body == null)
        {
            return false;
        }

        if (_collider == null)
        {
            return false;
        }

        if (_launcher == null)
        {
            return false;
        }

        if (other == null)
        {
            return false;
        }

        if (!other.enabled)
        {
            return false;
        }

        if (!other.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (other.attachedRigidbody == _body)
        {
            return false;
        }

        if (other.transform.IsChildOf(_launcher))
        {
            return false;
        }

        if ((_hitLayers & (1 << other.gameObject.layer)) == 0)
        {
            return false;
        }

        if (Physics2D.GetIgnoreLayerCollision(_body.gameObject.layer, other.gameObject.layer))
        {
            return false;
        }

        if (Physics2D.GetIgnoreCollision(_collider, other))
        {
            return false;
        }

        Rigidbody2D otherBody = other.attachedRigidbody;
        if (otherBody == null)
        {
            return true;
        }

        return otherBody.GetComponent<MissileController>() == null;
    }

    public bool HasHitAlongStep(Vector2 direction, float distance)
    {
        LastHit = null;
        _filter.SetLayerMask(_hitLayers & Physics2D.GetLayerCollisionMask(_body.gameObject.layer));
        int count = _body.Cast(direction, _filter, _results, distance);
        bool hasHit = false;
        for (int i = 0; i < count; i++)
        {
            if (!IsValidTarget(_results[i].collider))
            {
                continue;
            }

            hasHit = true;
            LastHit = _results[i].collider;
            break;
        }

        _results.Clear();
        return hasHit;
    }
}
