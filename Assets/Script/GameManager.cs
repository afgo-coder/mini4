using System;
using UnityEngine;

namespace Mini4.Core
{
    /// <summary>
    /// 5분 동안 생존하는 미니 프로젝트 3.1용 루프 매니저.
    /// - 1분마다 적 단계 상승
    /// - 남은 시간 및 단계 이벤트 전달
    /// </summary>
    public class MiniProjectGameManager : MonoBehaviour
    {
        [Header("Game Rule")]
        [SerializeField] private float playTimeSeconds = 300f;
        [SerializeField] private float stageIntervalSeconds = 60f;

        public event Action<float> OnTimeUpdated;
        public event Action<int> OnEnemyStageChanged;
        public event Action OnGameClear;

        public float RemainingTime => _remainingTime;
        public int CurrentEnemyStage => _currentEnemyStage;

        private float _remainingTime;
        private int _currentEnemyStage = 1;
        private float _elapsedForStage;
        private bool _isGameEnded;

        private void Start()
        {
            _remainingTime = playTimeSeconds;
            OnTimeUpdated?.Invoke(_remainingTime);
            OnEnemyStageChanged?.Invoke(_currentEnemyStage);
        }

        private void Update()
        {
            if (_isGameEnded)
            {
                return;
            }

            TickTimer(Time.deltaTime);
            TickEnemyStage(Time.deltaTime);
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


