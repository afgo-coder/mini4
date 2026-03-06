using UnityEngine;

namespace Mini4.Combat
{
    public class CastleHealth : MonoBehaviour
    {
        [SerializeField] private HealthEntity healthEntity;

        private void Awake()
        {
            if (healthEntity == null)
            {
                healthEntity = GetComponent<HealthEntity>();
            }
        }

        public void TakeDamage(float damage)
        {
            if (healthEntity == null)
            {
                return;
            }

            healthEntity.TakeDamage(damage);
        }
    }
}
