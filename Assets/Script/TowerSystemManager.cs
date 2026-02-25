using System;
using System.Collections.Generic;
using Mini4.Combat;
using Mini4.Economy;
using UnityEngine;

namespace Mini4.Tower
{
    /// <summary>
    /// 타워 건설(골드/인구) 및 강화 확률/비용 규칙 관리.
    /// </summary>
    public class TowerSystemManager : MonoBehaviour
    {
        [Serializable]
        private struct TowerCost
        {
            public AttackTowerType type;
            public int goldCost;
            public float baseAttack;
            public float hp;
            public GameObject prefab;
        }

        [Header("References")]
        [SerializeField] private EconomyManager economyManager;

        [Header("Population")]
        [SerializeField] private int startingPopulation = 10;
        [SerializeField] private int populationTowerGoldCost = 10;
        [SerializeField] private int populationProvidedPerTower = 10;

        [Header("Attack Tower Costs")]
        [SerializeField]
        private TowerCost[] attackTowerCosts = new TowerCost[]
        {
            new TowerCost { type = AttackTowerType.Archer, goldCost = 2, baseAttack = 10f, hp = 40f },
            new TowerCost { type = AttackTowerType.Cannon, goldCost = 4, baseAttack = 16f, hp = 60f },
            new TowerCost { type = AttackTowerType.Crossbow, goldCost = 5, baseAttack = 18f, hp = 50f },
            new TowerCost { type = AttackTowerType.IceWizard, goldCost = 6, baseAttack = 14f, hp = 45f },
            new TowerCost { type = AttackTowerType.Lightning, goldCost = 6, baseAttack = 14f, hp = 45f },
            new TowerCost { type = AttackTowerType.PoisonWizard, goldCost = 6, baseAttack = 14f, hp = 45f },
        };

        // index 1~5 사용
        private static readonly float[] UpgradeSuccessRate = { 0f, 0.9f, 0.8f, 0.6f, 0.4f, 0.3f };
        private static readonly int[] UpgradeCost = { 0, 1, 2, 3, 5, 7 };
        private static readonly float[] UpgradeAdd = { 0f, 1f, 2f, 4f, 6f, 10f };
        private static readonly float[] UpgradePercent = { 0f, 0.10f, 0.10f, 0.05f, 0.05f, 0.05f };

        public event Action<int, int> OnPopulationChanged;

        public int TotalPopulation { get; private set; }
        public int UsedPopulation { get; private set; }
        public int FreePopulation => Mathf.Max(0, TotalPopulation - UsedPopulation);

        private readonly Dictionary<AttackTowerType, TowerCost> _costMap = new Dictionary<AttackTowerType, TowerCost>();

        private void Awake()
        {
            BuildCostMap();
            TotalPopulation = Mathf.Max(0, startingPopulation);
            OnPopulationChanged?.Invoke(UsedPopulation, TotalPopulation);
        }

        public bool TryBuildPopulationTower()
        {
            if (economyManager == null)
            {
                return false;
            }

            if (!economyManager.TrySpend(populationTowerGoldCost))
            {
                return false;
            }

            TotalPopulation += populationProvidedPerTower;
            OnPopulationChanged?.Invoke(UsedPopulation, TotalPopulation);
            return true;
        }

        public bool TryBuildAttackTower(AttackTowerType type, Vector3 position, Quaternion rotation, Transform parent, out TowerInstance instance)
        {
            instance = null;
            if (economyManager == null || !_costMap.TryGetValue(type, out TowerCost cost))
            {
                return false;
            }

            if (FreePopulation < 1)
            {
                return false;
            }

            if (!economyManager.TrySpend(cost.goldCost))
            {
                return false;
            }

            GameObject go = cost.prefab != null ? Instantiate(cost.prefab, position, rotation, parent) : new GameObject(type.ToString());
            instance = go.GetComponent<TowerInstance>();
            if (instance == null)
            {
                instance = go.AddComponent<TowerInstance>();
            }

            instance.Initialize(type, cost.baseAttack);

            HealthEntity hp = go.GetComponent<HealthEntity>();
            if (hp == null)
            {
                hp = go.AddComponent<HealthEntity>();
            }

            hp.Initialize(cost.hp);

            if (go.GetComponent<AttackTowerMarker>() == null)
            {
                go.AddComponent<AttackTowerMarker>();
            }

            if (go.GetComponent<TowerAutoAttack>() == null)
            {
                go.AddComponent<TowerAutoAttack>();
            }

            UsedPopulation += 1;
            OnPopulationChanged?.Invoke(UsedPopulation, TotalPopulation);
            return true;
        }

        public bool TryUpgrade(TowerInstance target, out bool isSuccess)
        {
            isSuccess = false;
            if (target == null || economyManager == null)
            {
                return false;
            }

            int nextLevel = target.Level + 1;
            if (nextLevel > 5)
            {
                return false;
            }

            if (!economyManager.TrySpend(UpgradeCost[nextLevel]))
            {
                return false;
            }

            isSuccess = UnityEngine.Random.value <= UpgradeSuccessRate[nextLevel];
            if (!isSuccess)
            {
                return true;
            }

            target.ApplyUpgrade(UpgradeAdd[nextLevel], UpgradePercent[nextLevel]);
            return true;
        }

        private void BuildCostMap()
        {
            _costMap.Clear();
            foreach (TowerCost cost in attackTowerCosts)
            {
                _costMap[cost.type] = cost;
            }
        }
    }
}



