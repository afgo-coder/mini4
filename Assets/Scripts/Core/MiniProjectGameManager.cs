using System;
using UnityEngine;

namespace Mini4.Core
{
    /// <summary>
    /// 5 minute survival loop manager.
    /// - Adds warmup delay before run starts
    /// - Broadcasts time and stage updates
    /// </summary>
    public class MiniProjectGameManager : MonoBehaviour
    {
        [Header("Game Rule")]
        [SerializeField] private float warmupSeconds = 15f;
        [SerializeField] private float playTimeSeconds = 300f;
        [SerializeField] private float stageIntervalSeconds = 60f;

        public event Action<float> OnTimeUpdated;
        public event Action<float> OnWarmupUpdated;
        public event Action<int> OnEnemyStageChanged;
        public event Action OnRunStarted;
        public event Action OnGameClear;

        public float RemainingTime => _remainingTime;
        public float WarmupRemaining => _warmupRemaining;
        public bool IsRunStarted => _isRunStarted;
        public int CurrentEnemyStage => _currentEnemyStage;

        private float _warmupRemaining;
        private float _remainingTime;
        private int _currentEnemyStage = 1;
        private float _elapsedForStage;
        private bool _isRunStarted;
        private bool _isGameEnded;

        private void Start()
        {
            _warmupRemaining = Mathf.Max(0f, warmupSeconds);
            _remainingTime = playTimeSeconds;

            OnTimeUpdated?.Invoke(_remainingTime);
            OnWarmupUpdated?.Invoke(_warmupRemaining);

            if (_warmupRemaining <= 0f)
            {
                StartRun();
            }
        }

        private void Update()
        {
            if (_isGameEnded)
            {
                return;
            }

            if (!_isRunStarted)
            {
                TickWarmup(Time.deltaTime);
                return;
            }

            TickTimer(Time.deltaTime);
            TickEnemyStage(Time.deltaTime);
        }

        private void TickWarmup(float deltaTime)
        {
            _warmupRemaining -= deltaTime;
            if (_warmupRemaining < 0f)
            {
                _warmupRemaining = 0f;
            }

            OnWarmupUpdated?.Invoke(_warmupRemaining);
            if (_warmupRemaining <= 0f)
            {
                StartRun();
            }
        }

        private void StartRun()
        {
            if (_isRunStarted)
            {
                return;
            }

            _isRunStarted = true;
            OnRunStarted?.Invoke();
            OnEnemyStageChanged?.Invoke(_currentEnemyStage);
        }

        private void TickTimer(float deltaTime)
        {
            _remainingTime -= deltaTime;
            if (_remainingTime < 0f)
            {
                _remainingTime = 0f;
            }

            OnTimeUpdated?.Invoke(_remainingTime);

            if (_remainingTime <= 0f)
            {
                _isGameEnded = true;
                OnGameClear?.Invoke();
            }
        }

        private void TickEnemyStage(float deltaTime)
        {
            _elapsedForStage += deltaTime;
            if (_elapsedForStage < stageIntervalSeconds)
            {
                return;
            }

            _elapsedForStage -= stageIntervalSeconds;
            _currentEnemyStage++;
            OnEnemyStageChanged?.Invoke(_currentEnemyStage);
        }
    }
}

