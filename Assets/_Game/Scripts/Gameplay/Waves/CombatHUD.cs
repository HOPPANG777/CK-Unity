using UnityEngine;
using UnityEngine.UI;

public sealed class CombatHUD : MonoBehaviour
{
    [SerializeField] private EnemyWaveController _waves;
    [SerializeField] private PlayerHealth _health;
    [SerializeField] private CombatSession _session;
    // 기존 Canvas의 UI를 연결한다.
    [SerializeField] private Text _waveText;
    [SerializeField] private Text _healthText;
    [SerializeField] private Text _statusText;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private Text _resultTitle;
    [SerializeField] private Text _resultDetail;

    private int _lastWave = -1;
    private int _lastHealth = -1;
    private int _lastEnemies = -1;
    private int _lastSeconds = -1;

    private void OnEnable()
    {
        if (_session != null)
        {
            _session.Finished += ShowResult;
        }

        if (_resultPanel != null)
        {
            _resultPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (_session != null)
        {
            _session.Finished -= ShowResult;
        }
    }

    private void Update()
    {
        if (_waves == null || _health == null || _session == null || _session.IsFinished)
        {
            return;
        }

        if (_waveText != null && _lastWave != _waves.CurrentWave)
        {
            _waveText.text = "WAVE   " + Mathf.Max(1, _waves.CurrentWave) + " / " + _waves.WaveCount;
            _lastWave = _waves.CurrentWave;
        }

        if (_healthText != null && _lastHealth != _health.Health)
        {
            _healthText.text = "LIFE   " + _health.Health + " / " + _health.MaxHealth;
            _lastHealth = _health.Health;
        }

        int seconds = Mathf.Max(1, Mathf.CeilToInt(_waves.WaitRemaining));
        if (_statusText != null && (_lastEnemies != _waves.ActiveEnemies || _lastSeconds != seconds))
        {
            _statusText.text = _waves.ActiveEnemies > 0 ? "ORCS LEFT   " + _waves.ActiveEnemies :
                "NEXT WAVE IN " + seconds;
            _lastEnemies = _waves.ActiveEnemies;
            _lastSeconds = seconds;
        }
    }

    private void ShowResult(bool cleared)
    {
        if (_resultPanel != null)
        {
            _resultPanel.SetActive(true);
        }

        if (_resultTitle != null)
        {
            _resultTitle.text = cleared ? "DUNGEON CLEARED" : "RUN ENDED";
        }

        if (_resultDetail != null)
        {
            _resultDetail.text = cleared ? "All waves defeated." : "Fell on wave " + _waves.CurrentWave;
        }

        if (_healthText != null)
        {
            _healthText.text = "LIFE   " + _health.Health + " / " + _health.MaxHealth;
        }
    }
}
