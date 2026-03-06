using Mini4.Economy;
using Mini4.Tower;
using TMPro;
using UnityEngine;

namespace Mini4.UI
{
    /// <summary>
    /// GameManager/Economy/Tower 이벤트를 HUD 텍스트에 연결.
    /// </summary>
    public class GameHudPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Mini4.Core.MiniProjectGameManager gameManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private TowerSystemManager towerSystemManager;

        [Header("UI")]
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text populationText;
        [SerializeField] private TMP_Text resultText;

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnTimeUpdated += HandleTimeUpdated;
                gameManager.OnEnemyStageChanged += HandleStageChanged;
                gameManager.OnGameClear += HandleGameClear;
            }

            if (economyManager != null)
            {
                economyManager.OnGoldChanged += HandleGoldChanged;
                HandleGoldChanged(economyManager.Gold);
            }

            if (towerSystemManager != null)
            {
                towerSystemManager.OnPopulationChanged += HandlePopulationChanged;
                HandlePopulationChanged(towerSystemManager.UsedPopulation, towerSystemManager.TotalPopulation);
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnTimeUpdated -= HandleTimeUpdated;
                gameManager.OnEnemyStageChanged -= HandleStageChanged;
                gameManager.OnGameClear -= HandleGameClear;
            }

            if (economyManager != null)
            {
                economyManager.OnGoldChanged -= HandleGoldChanged;
            }

            if (towerSystemManager != null)
            {
                towerSystemManager.OnPopulationChanged -= HandlePopulationChanged;
            }
        }

        private void HandleTimeUpdated(float remainingTime)
        {
            if (timeText == null)
            {
                return;
            }

            int total = Mathf.CeilToInt(remainingTime);
            int minutes = total / 60;
            int seconds = total % 60;
            timeText.text = $"Time {minutes:00}:{seconds:00}";
        }

        private void HandleStageChanged(int stage)
        {
            if (stageText == null)
            {
                return;
            }

            stageText.text = $"Stage {stage}";
        }

        private void HandleGoldChanged(int gold)
        {
            if (goldText == null)
            {
                return;
            }

            goldText.text = $"Gold {gold}";
        }

        private void HandlePopulationChanged(int used, int total)
        {
            if (populationText == null)
            {
                return;
            }

            populationText.text = $"Pop {used}/{total}";
        }

        private void HandleGameClear()
        {
            if (resultText == null)
            {
                return;
            }

            resultText.text = "CLEAR";
            resultText.gameObject.SetActive(true);
        }
    }
}
