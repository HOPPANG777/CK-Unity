using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class DungeonGameController : MonoBehaviour
{
    private const int WAVE_COUNT = 5;

    [Header("Scene")]
    [SerializeField] private bool _isTitleScene;

    [Header("Existing Gameplay")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private EnemyManager _enemyManager;
    [SerializeField] private Transform _enemyParent;
    [SerializeField] private MissileController _missile;
    [SerializeField] private ScreenWorldBoundsView _bounds;
    [SerializeField] private SpriteRenderer _visual;
    [SerializeField] private Animator _animator;
    [Min(0.1f)][SerializeField] private float _waveDelay = 2.0f;

    [Header("UI")]
    [SerializeField] private Text _waveText;
    [SerializeField] private Text _healthText;
    [SerializeField] private Text _statusText;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private Text _resultTitle;
    [SerializeField] private Text _resultDetail;

    private EnemyController[] _enemies;
    private int _wave;
    private int _health = 5;
    private float _waitRemaining;
    private float _damageCooldown;
    private bool _isFighting;
    private bool _isFinished;
    private bool _isCleared;

    public int CurrentWave
    {
        get
        {
            return _wave;
        }
    }
    public int Health
    {
        get
        {
            return _health;
        }
    }
    public bool IsFinished
    {
        get
        {
            return _isFinished;
        }
    }
    public bool IsCleared
    {
        get
        {
            return _isCleared;
        }
    }

    private void Start()
    {
        if (_isTitleScene)
        {
            return;
        }

        if (!HasReferences())
        {
            return;
        }

        if (_waveDelay <= 0.0f)
        {
            return;
        }

        if (!MissileMove.IsFinite(_waveDelay))
        {
            return;
        }

        if (!_player.IsReady)
        {
            return;
        }

        if (!_missile.Init(_bounds, _player.transform))
        {
            return;
        }

        if (!_enemyManager.TryInitialize())
        {
            return;
        }

        _enemies = _enemyParent.GetComponentsInChildren<EnemyController>(true);
        if (_enemies.Length != 7)
        {
            return;
        }

        _player.Init(_missile);
        _missile.Hit += OnMissileHit;
        for (int i = 0; i < _enemies.Length; i++)
        {
            EnemyCollision collision = _enemies[i].GetComponentInChildren<EnemyCollision>(true);
            if (collision == null)
            {
                return;
            }

            collision.Contact += OnEnemyContact;
        }

        _resultPanel.SetActive(false);

        StopEnemies();

        _waitRemaining = _waveDelay;
        _healthText.text = "LIFE   " + _health + " / 5";
    }

    private void Update()
    {
        if (_isTitleScene)
        {
            return;
        }

        if (_enemies == null)
        {
            return;
        }

        if (_isFinished)
        {
            return;
        }

        _damageCooldown = Mathf.Max(0.0f, _damageCooldown - Time.deltaTime);
        UpdatePlayerVisual();

        _waveText.text = "WAVE   " + Mathf.Max(1, _wave) + " / " + WAVE_COUNT;
        if (!_isFighting)
        {
            _waitRemaining -= Time.deltaTime;
            _statusText.text = "WAVE " + (_wave + 1) + " IN " + Mathf.Max(1, Mathf.CeilToInt(_waitRemaining));
            if (_waitRemaining <= 0.0f)
            {
                BeginWave();
            }

            return;
        }

        int remaining = CountRemainingEnemies();
        _statusText.text = "ORCS LEFT   " + remaining;
        if (remaining > 0)
        {
            return;
        }

        _isFighting = false;
        _missile.gameObject.SetActive(false);
        if (_wave == WAVE_COUNT)
        {
            FinishGame(true);
            return;
        }

        _waitRemaining = _waveDelay;
    }

    private void UpdatePlayerVisual()
    {
        _visual.color = Color.white;

        if (_damageCooldown > 0.0f)
        {
            _visual.color = new Color(1.0f, 0.5f, 0.5f);
        }

        Vector2 velocity = _body.linearVelocity;
        bool moving = velocity.sqrMagnitude > 0.01f;

        _animator.SetBool("Walk", moving);
        _animator.SetBool("Idle", !moving);

        if (!moving)
        {
            return;
        }

        _animator.SetFloat("X", velocity.x);
        _animator.SetFloat("Y", velocity.y);
    }

    private int CountRemainingEnemies()
    {
        int remaining = 0;
        for (int i = 0; i < _enemies.Length; i++)
        {
            if (_enemies[i] == null)
            {
                continue;
            }

            if (!_enemies[i].IsRunning)
            {
                continue;
            }

            remaining++;
        }

        return remaining;
    }

    private void LateUpdate()
    {
        if (_isTitleScene)
        {
            return;
        }

        if (_body == null)
        {
            return;
        }

        if (_isFinished)
        {
            return;
        }

        if (_bounds == null)
        {
            return;
        }

        if (!_bounds.TryGetBounds(out ScreenWorldBoundsArea area))
        {
            return;
        }

        Vector2 position = _body.position;

        position.x = Mathf.Clamp(position.x, area.Min.x + 0.4f, area.Max.x - 0.4f);
        position.y = Mathf.Clamp(position.y, area.Min.y + 0.4f, area.Max.y - 0.4f);

        if (position == _body.position)
        {
            return;
        }

        _body.position = position;
    }

    private void OnEnemyContact(Collider2D other)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (!_isFighting)
        {
            return;
        }

        if (_isFinished)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        // 적의 충돌 콜백이 먼저 실행돼도 미사일을 종료한다.
        if (other.GetComponent<MissileController>() == _missile)
        {
            _missile.gameObject.SetActive(false);
            return;
        }

        if (_damageCooldown > 0.0f)
        {
            return;
        }

        if (!other.transform.IsChildOf(_player.transform))
        {
            return;
        }

        _health--;
        _damageCooldown = 0.75f;
        _healthText.text = "LIFE   " + _health + " / 5";

        if (_health == 0)
        {
            FinishGame(false);
        }
    }

    private void OnDestroy()
    {
        if (_missile != null)
        {
            _missile.Hit -= OnMissileHit;
        }
        if (_enemies == null)
        {
            return;
        }

        for (int i = 0; i < _enemies.Length; i++)
        {
            if (_enemies[i] == null)
            {
                continue;
            }

            EnemyCollision collision = _enemies[i].GetComponentInChildren<EnemyCollision>(true);
            if (collision == null)
            {
                continue;
            }

            collision.Contact -= OnEnemyContact;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Dungeon");
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void BeginWave()
    {
        if (!_enemyManager.TryRedistribute())
        {
            FinishGame(false);
            return;
        }

        _wave++;
        for (int i = 0; i < _enemies.Length; i++)
        {
            _enemies[i].SetColor(Color.white);
            _enemies[i].SetMoveSpeed(0.65f + _wave * 0.2f);

            Animator animator = _enemies[i].GetComponentInChildren<Animator>(true);

            if (animator != null)
            {
                animator.SetBool("Walk", true);
            }
            if (i >= _wave + 2)
            {
                _enemies[i].Deactivate();
            }
        }
        _isFighting = true;
    }

    private void OnMissileHit(Collider2D other)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (!_isFighting)
        {
            return;
        }

        if (_isFinished)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        EnemyController enemy = other.GetComponentInParent<EnemyController>();

        if (enemy == null)
        {
            return;
        }

        enemy.Deactivate();
    }

    private void StopEnemies()
    {
        for (int i = 0; i < _enemies.Length; i++)
        {
            if (_enemies[i] == null)
            {
                continue;
            }

            _enemies[i].Deactivate();
        }
    }

    private void FinishGame(bool cleared)
    {
        if (_isFinished)
        {
            return;
        }

        _isFinished = true;
        _isCleared = cleared;
        _isFighting = false;

        StopEnemies();

        _player.enabled = false;

        _missile.gameObject.SetActive(false);
        _resultPanel.SetActive(true);

        if (cleared)
        {
            _resultTitle.text = "DUNGEON CLEARED";
            _resultDetail.text = "All five waves defeated.\nThe dungeon is yours.";
            _statusText.text = "5 / 5 WAVES COMPLETE";
            return;
        }

        _resultTitle.text = "RUN ENDED";
        _resultDetail.text = "Fell on wave " + _wave + " / 5.\nTake a breath. Try again.";
        _statusText.text = "NO LIFE REMAINING";
    }

    private bool HasReferences()
    {
        if (_player == null)
        {
            return false;
        }

        if (_body == null)
        {
            return false;
        }

        if (_enemyManager == null)
        {
            return false;
        }

        if (_enemyParent == null)
        {
            return false;
        }

        if (_missile == null)
        {
            return false;
        }

        if (_bounds == null)
        {
            return false;
        }

        if (_visual == null)
        {
            return false;
        }

        if (_animator == null)
        {
            return false;
        }

        if (_waveText == null)
        {
            return false;
        }

        if (_healthText == null)
        {
            return false;
        }

        if (_statusText == null)
        {
            return false;
        }

        if (_resultPanel == null)
        {
            return false;
        }

        if (_resultTitle == null)
        {
            return false;
        }

        if (_resultDetail == null)
        {
            return false;
        }

        return true;
    }

    private void StopWithError(string message)
    {
        Debug.LogError("[DungeonGameController] " + message, this);
        enabled = false;
    }
}
