using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyCollision : MonoBehaviour
{
    private IEnemyControl _owner;

    public event Action<Collider2D> Contact;

    public void Init(IEnemyControl owner)
    {
        _owner = owner;
    }

    public void Handle(Collider2D other)
    {
        if (_owner == null)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        if (other.GetComponentInParent<EnemyController>() != null)
        {
            return;
        }

        if (Contact != null)
        {
            Contact(other);
        }

        _owner.Deactivate();
    }
}
