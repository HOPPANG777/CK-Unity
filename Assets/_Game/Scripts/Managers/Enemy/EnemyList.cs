using UnityEngine;

internal sealed class EnemyList
{
    private readonly EnemyController[] _enemies;

    private readonly GameObject[] _objects;

    private readonly Transform _parent;

    private readonly Transform _target;

    public EnemyList(int count, Transform parent, Transform target)
    {
        _enemies = new EnemyController[count];
        _objects = new GameObject[count];
        _parent = parent;
        _target = target;
    }

    public int Count
    {
        get
        {
            return _enemies.Length;
        }
    }

    public bool TryCreate(EnemyController prefab, float speed)
    {
        for (int i = 0; i < _enemies.Length; i++)
        {
            if (!TryCreateEnemy(i, prefab, speed))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryRespawn(EnemyLayout layout, Vector2 center)
    {
        if (!CanRespawn(layout, center))
        {
            return false;
        }

        for (int i = 0; i < _enemies.Length; i++)
        {
            layout.TryGetPosition(i, center, out Vector2 position);
            IEnemyControl enemy = _enemies[i];

            if (!enemy.TryRespawn(position))
            {
                DeactivateAll();
                return false;
            }
        }

        return true;
    }

    public void DeactivateAll()
    {
        for (int i = 0; i < _enemies.Length; i++)
        {
            DeactivateEnemy(i);
        }
    }

    public void Release()
    {
        DeactivateAll();

        for (int i = 0; i < _enemies.Length; i++)
        {
            ReleaseEnemy(i);
        }
    }

    private bool TryCreateEnemy(int index, EnemyController prefab, float speed)
    {
        _objects[index] = Object.Instantiate(prefab.gameObject, _parent, false);
        _enemies[index] = _objects[index].GetComponent<EnemyController>();

        if (_enemies[index] == null)
        {
            return false;
        }

        IEnemyControl enemy = _enemies[index];

        if (!enemy.Init())
        {
            return false;
        }

        if (!enemy.SetMoveSpeed(speed))
        {
            return false;
        }

        Color color = new Color(Random.value, Random.value, Random.value, 1.0f);

        if (!enemy.SetColor(color))
        {
            return false;
        }

        enemy.SetTarget(_target);
        return true;
    }

    private bool CanRespawn(EnemyLayout layout, Vector2 center)
    {
        if (_enemies.Length > 0)
        {
            if (_target == null)
            {
                return false;
            }

            if (!EnemyManager.IsValidParent(_parent))
            {
                return false;
            }
        }

        for (int i = 0; i < _enemies.Length; i++)
        {
            if (_enemies[i] == null)
            {
                return false;
            }

            if (!_enemies[i].enabled)
            {
                return false;
            }

            if (_enemies[i].transform.parent != _parent)
            {
                return false;
            }

            if (!layout.TryGetPosition(i, center, out _))
            {
                return false;
            }
        }

        return true;
    }

    private void DeactivateEnemy(int index)
    {
        if (_enemies[index] != null)
        {
            ((IEnemyControl)_enemies[index]).Deactivate();
            return;
        }

        if (_objects[index] == null)
        {
            return;
        }

        _objects[index].SetActive(false);
    }

    private void ReleaseEnemy(int index)
    {
        if (_objects[index] == null)
        {
            _enemies[index] = null;
            return;
        }

        Object.Destroy(_objects[index]);
        _objects[index] = null;
        _enemies[index] = null;
    }
}
