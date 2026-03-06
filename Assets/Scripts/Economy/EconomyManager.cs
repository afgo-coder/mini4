using System;
using UnityEngine;

namespace Mini4.Economy
{
    /// <summary>
    /// 골드 보유량 및 수급(패시브/수확/소비)을 담당.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        [Header("Start Value")]
        [SerializeField] private int startingGold = 50;

        [Header("Passive Income")]
        [SerializeField] private int passiveIncomeAmount = 1;
        [SerializeField] private float passiveIncomeIntervalSeconds = 3f;

        public event Action<int> OnGoldChanged;

        public int Gold => _gold;

        private int _gold;
        private float _elapsed;

        private void Start()
        {
            _gold = Mathf.Max(0, startingGold);
            OnGoldChanged?.Invoke(_gold);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < passiveIncomeIntervalSeconds)
            {
                return;
            }

            _elapsed -= passiveIncomeIntervalSeconds;
            AddGold(passiveIncomeAmount);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (_gold < amount)
            {
                return false;
            }

            _gold -= amount;
            OnGoldChanged?.Invoke(_gold);
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _gold += amount;
            OnGoldChanged?.Invoke(_gold);
        }
    }
}
