using UnityEngine;

namespace Mini4.Economy
{
    /// <summary>
    /// 클릭 수확 가능한 버섯 오브젝트.
    /// </summary>
    public class HarvestableMushroom : MonoBehaviour
    {
        [SerializeField] private int harvestGold;

        private MushroomFieldManager _owner;
        private bool _isHarvested;

        public void Initialize(MushroomFieldManager owner, int gold)
        {
            _owner = owner;
            harvestGold = gold;
        }

        public void Harvest()
        {
            if (_isHarvested)
            {
                return;
            }

            _isHarvested = true;
            _owner?.Harvest(this, harvestGold);
        }

        private void OnMouseDown()
        {
            Harvest();
        }
    }
}


