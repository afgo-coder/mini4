using Mini4.Tower;
using UnityEngine;

namespace Mini4.UI
{
    /// <summary>
    /// 우하단 타워 UI 버튼과 배치 매니저 연결용.
    /// 버튼 OnClick에 아래 public 메서드를 연결.
    /// </summary>
    public class TowerBuildPanelUI : MonoBehaviour
    {
        [SerializeField] private TowerPlacementManager placementManager;

        private void Awake()
        {
            if (placementManager == null)
            {
                placementManager = FindObjectOfType<TowerPlacementManager>();
            }
        }

        public void SelectArcher() => placementManager?.SelectAttackTower((int)AttackTowerType.Archer);
        public void SelectCannon() => placementManager?.SelectAttackTower((int)AttackTowerType.Cannon);
        public void SelectCrossbow() => placementManager?.SelectAttackTower((int)AttackTowerType.Crossbow);
        public void SelectIceWizard() => placementManager?.SelectAttackTower((int)AttackTowerType.IceWizard);
        public void SelectLightning() => placementManager?.SelectAttackTower((int)AttackTowerType.Lightning);
        public void SelectPoisonWizard() => placementManager?.SelectAttackTower((int)AttackTowerType.PoisonWizard);
        public void SelectPopulationTower() => placementManager?.SelectPopulationTower();
        public void CancelBuild() => placementManager?.CancelBuildMode();
    }
}
