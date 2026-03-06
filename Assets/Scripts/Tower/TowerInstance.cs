using System;
using UnityEngine;

namespace Mini4.Tower
{
    /// <summary>
    /// 개별 타워의 공격력/강화 상태를 보관.
    /// </summary>
    public class TowerInstance : MonoBehaviour
    {
        [SerializeField] private AttackTowerType towerType;
        [SerializeField] private int level;
        [SerializeField] private float baseAttack = 5f;
        [SerializeField] private float currentAttack = 5f;

        public static event Action<TowerInstance> OnTowerClicked;

        public AttackTowerType TowerType => towerType;
        public int Level => level;
        public float CurrentAttack => currentAttack;
        public string DisplayName => towerType.ToString();

        public void Initialize(AttackTowerType type, float initialAttack)
        {
            towerType = type;
            level = 0;
            baseAttack = initialAttack;
            currentAttack = initialAttack;
        }

        public void ApplyUpgrade(float additive, float percentMultiplier)
        {
            level++;
            currentAttack = (currentAttack + additive) * (1f + percentMultiplier);
        }

        private void OnMouseDown()
        {
            OnTowerClicked?.Invoke(this);
        }
    }
}
