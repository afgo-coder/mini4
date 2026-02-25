using Mini4.Combat;
using Mini4.Tower;
using UnityEngine;

namespace Mini4.Enemy
{
    /// <summary>
    /// 성을 추적하되 주변 공격타워가 먼저 보이면 공격.
    /// </summary>
    public class EnemyCombatMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 1.4f;
        [SerializeField] private float attackRange = 0.7f;
        [SerializeField] private float attackDamage = 2f;
        [SerializeField] private float attackInterval = 0.8f;
        [SerializeField] private float towerPriorityRange = 2.2f;

        private CastleHealth _castle;
        private float _attackCooldown;

        private void Awake()
        {
            TryCacheCastle();
            if (GetComponent<EnemyTag>() == null)
            {
                gameObject.AddComponent<EnemyTag>();
            }
        }

        private void Update()
        {
            if (_castle == null)
            {
                TryCacheCastle();
            }

            _attackCooldown -= Time.deltaTime;
            HealthEntity target = SelectTarget();
            if (target == null)
            {
                return;
            }

            Vector2 targetPos = target.transform.position;
            float dist = Vector2.Distance(transform.position, targetPos);
            if (dist <= attackRange)
            {
                TryAttack(target);
                return;
            }

            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }

        private void TryCacheCastle()
        {
            _castle = FindObjectOfType<CastleHealth>();
            if (_castle != null)
            {
                return;
            }

            GameObject castleObj = GameObject.FindWithTag("Castle");
            if (castleObj == null)
            {
                return;
            }

            _castle = castleObj.GetComponent<CastleHealth>();
            if (_castle == null)
            {
                _castle = castleObj.AddComponent<CastleHealth>();
            }

            if (castleObj.GetComponent<HealthEntity>() == null)
            {
                castleObj.AddComponent<HealthEntity>();
            }
        }

        private HealthEntity SelectTarget()
        {
            AttackTowerMarker[] towers = FindObjectsOfType<AttackTowerMarker>();
            AttackTowerMarker nearestTower = null;
            float nearestDist = float.MaxValue;
            foreach (AttackTowerMarker tower in towers)
            {
                if (tower == null || tower.Health == null || tower.Health.CurrentHp <= 0f)
                {
                    continue;
                }

                float d = Vector2.Distance(transform.position, tower.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    nearestTower = tower;
                }
            }

            if (nearestTower != null && nearestDist <= towerPriorityRange)
            {
                return nearestTower.Health;
            }

            if (_castle == null)
            {
                return null;
            }

            return _castle.GetComponent<HealthEntity>();
        }

        private void TryAttack(HealthEntity target)
        {
            if (_attackCooldown > 0f)
            {
                return;
            }

            _attackCooldown = attackInterval;
            target.TakeDamage(attackDamage);
        }
    }
}


