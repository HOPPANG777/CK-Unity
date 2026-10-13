using System;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour, IDamageable
{
    [Min(1)][SerializeField] private int _maxHealth = 5;
    // 피격 후 무적 시간(초).
    [Min(0f)][SerializeField] private float _invulnerability = 0.75f;
    [SerializeField] private SpriteRenderer _visual;

    private float _nextDamageTime;
    private Color _normalColor;

    public int Health { get; private set; }
    public int MaxHealth => _maxHealth;
    public event Action Died;

    private void Awake()
    {
        Health = Mathf.Max(1, _maxHealth);
        _normalColor = _visual == null ? Color.white : _visual.color;
    }

    public void TakeDamage(int amount)
    {
        if (Health <= 0 || amount <= 0 || Time.time < _nextDamageTime)
        {
            return;
        }

        Health = Mathf.Max(0, Health - amount);
        _nextDamageTime = Time.time + _invulnerability;
        if (Health == 0)
        {
            Died?.Invoke();
        }
    }

    private void LateUpdate()
    {
        if (_visual != null)
        {
            _visual.color = Time.time < _nextDamageTime ? new Color(1f, 0.5f, 0.5f) : _normalColor;
        }
    }
}
