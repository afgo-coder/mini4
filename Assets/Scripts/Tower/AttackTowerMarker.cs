using Mini4.Combat;
using UnityEngine;

namespace Mini4.Tower
{
    public class AttackTowerMarker : MonoBehaviour
    {
        public HealthEntity Health { get; private set; }

        private void Awake()
        {
            Health = GetComponent<HealthEntity>();
            if (Health == null)
            {
                Health = gameObject.AddComponent<HealthEntity>();
            }
        }
    }
}
