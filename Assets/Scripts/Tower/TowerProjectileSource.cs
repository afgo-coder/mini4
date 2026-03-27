using UnityEngine;

namespace Mini4.Tower
{
    public class TowerProjectileSource : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Vector3 projectileSpawnOffset = new Vector3(0f, 0.2f, 0f);

        public GameObject ProjectilePrefab => projectilePrefab;
        public Vector3 ProjectileSpawnOffset => projectileSpawnOffset;
    }
}
