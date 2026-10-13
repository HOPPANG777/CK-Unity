using System;
using UnityEngine;

[Serializable]
public sealed class EnemyWave
{
    // IEnemyFormation 구현(Grid·Circle·Dot)을 연결한다.
    [SerializeField] private MonoBehaviour _formation;
    [Range(1, 10)][SerializeField] private int _count = 3;
    // 이동 속도(월드 단위/초).
    [Min(0f)][SerializeField] private float _speed = 0.85f;

    public IEnemyFormation Formation => _formation as IEnemyFormation;
    public int Count => _count;
    public float Speed => _speed;
}
