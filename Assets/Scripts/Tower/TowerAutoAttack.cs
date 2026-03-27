using Mini4.Combat;
using Mini4.Enemy;
using UnityEngine;

namespace Mini4.Tower
{
    public class TowerAutoAttack : MonoBehaviour
    {
        [SerializeField] private TowerInstance towerInstance;
        [SerializeField] private float attackRange = 2.4f;
        [SerializeField] private float attackInterval = 0.6f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Vector3 projectileSpawnOffset = new Vector3(0f, 0.2f, 0f);

        public float AttackRange => attackRange;

        private float _cooldown;

        private void Awake()
        {
            if (towerInstance == null)
            {
                towerInstance = GetComponent<TowerInstance>();
            }
        }

        public void SetAttackRange(float range)
        {
            attackRange = Mathf.Max(0.1f, range);
        }

        public void SetProjectileConfig(GameObject prefab, Vector3 spawnOffset)
        {
            projectilePrefab = prefab;
            projectileSpawnOffset = spawnOffset;
        }

        private void Update()
        {
            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f)
            {
                return;
            }

            EnemyTag targetEnemy = FindNearestEnemy();
            if (targetEnemy == null)
            {
                return;
            }

            HealthEntity enemyHealth = targetEnemy.GetComponent<HealthEntity>();
            if (enemyHealth == null)
            {
                return;
            }

            _cooldown = attackInterval;
            float damage = towerInstance != null ? towerInstance.CurrentAttack : 1f;

            if (projectilePrefab != null)
            {
                TowerProjectile projectile = ProjectilePoolManager.Spawn(projectilePrefab, transform.position + projectileSpawnOffset, Quaternion.identity);
                if (projectile != null)
                {
                    projectile.Launch(targetEnemy.transform, damage);
                    return;
                }
            }

            enemyHealth.TakeDamage(damage);
        }

        private EnemyTag FindNearestEnemy()
        {
            EnemyTag[] enemies = FindObjectsByType<EnemyTag>(FindObjectsSortMode.None);
            EnemyTag result = null;
            float best = float.MaxValue;
            foreach (EnemyTag enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist > attackRange || dist >= best)
                {
                    continue;
                }

                best = dist;
                result = enemy;
            }

            return result;
        }
    }
}
