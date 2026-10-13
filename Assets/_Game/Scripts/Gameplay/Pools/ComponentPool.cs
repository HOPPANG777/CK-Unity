using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ComponentPool<T> : IDisposable where T : Component
{
    private readonly Func<T> _create;
    private readonly Queue<T> _available;
    private readonly HashSet<T> _rented;
    private readonly List<T> _items;
    private bool _disposed;

    public int ActiveCount => _rented.Count;
    public int TotalCount => _items.Count;

    public ComponentPool(Func<T> create, int initialCount)
    {
        _create = create ?? throw new ArgumentNullException(nameof(create));
        _available = new Queue<T>(initialCount);
        _rented = new HashSet<T>(initialCount);
        _items = new List<T>(initialCount);
        try
        {
            for (int i = 0; i < initialCount; i++)
            {
                _available.Enqueue(Create());
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public T Rent()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ComponentPool<T>));
        }

        T item = null;
        while (_available.Count > 0 && item == null)
        {
            item = _available.Dequeue();
        }

        if (item == null)
        {
            item = Create();
        }

        _rented.Add(item);
        return item;
    }

    public void Return(T item)
    {
        if (_disposed || item == null || !_rented.Remove(item))
        {
            return;
        }

        // OnDisable의 반환 요청보다 먼저 대여 상태를 해제한다.
        item.gameObject.SetActive(false);
        _available.Enqueue(item);
    }

    public void ReturnAll()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            Return(_items[i]);
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _rented.Clear();
        _available.Clear();
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null)
            {
                UnityEngine.Object.Destroy(_items[i].gameObject);
            }
        }

        _items.Clear();
    }

    private T Create()
    {
        T item = _create();
        if (item == null)
        {
            throw new InvalidOperationException("풀 팩토리가 유효한 컴포넌트를 반환해야 합니다.");
        }

        _items.Add(item);
        return item;
    }
}
