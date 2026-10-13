using System;
using UnityEngine;

public sealed class CombatSession : MonoBehaviour
{
    [SerializeField] private PlayerHealth _health;
    [SerializeField] private PlayerController _player;
    [SerializeField] private PlayerPointerFire _fire;
    [SerializeField] private EnemyWaveController _waves;
    [SerializeField] private MissilePoolController _missiles;

    public bool IsFinished { get; private set; }
    public bool IsCleared { get; private set; }
    public event Action<bool> Finished;

    private void OnEnable()
    {
        if (_health == null || _player == null || _fire == null || _waves == null || _missiles == null)
        {
            Debug.LogError("[CombatSession] 플레이어·웨이브·발사·미사일 풀을 연결하세요.", this);
            enabled = false;
            return;
        }

        _health.Died += OnPlayerDied;
        _waves.Completed += OnCleared;
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.Died -= OnPlayerDied;
        }

        if (_waves != null)
        {
            _waves.Completed -= OnCleared;
        }
    }

    private void OnPlayerDied()
    {
        Finish(false);
    }

    private void OnCleared()
    {
        Finish(true);
    }

    private void Finish(bool cleared)
    {
        if (IsFinished)
        {
            return;
        }

        IsFinished = true;
        IsCleared = cleared;
        _waves.Stop();
        _missiles.StopAll();
        _missiles.enabled = false;
        _fire.enabled = false;
        _player.enabled = false;
        Finished?.Invoke(cleared);
    }
}
