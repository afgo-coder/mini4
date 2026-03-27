using Mini4.Combat;
using Mini4.Enemy;
using UnityEngine;

namespace Mini4.Tower
{
    public class TowerProjectile : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float maxLifetimeSeconds = 3f;
        [SerializeField] private float impactDistance = 0.15f;
        [SerializeField] private bool rotateToVelocity = true;
        [SerializeField] private float rotationOffsetDegrees;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool flipX;
        [SerializeField] private bool flipY;

        public GameObject PoolPrefab { get; private set; }

        private Transform _target;
        private float _damage;
        private float _lifeRemaining;
        private bool _isLaunched;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            ApplySpriteFlip();
        }

        public void SetPoolPrefab(GameObject prefab)
        {
            PoolPrefab = prefab;
        }

        public void Launch(Transform target, float damage)
        {
            _target = target;
            _damage = damage;
            _lifeRemaining = maxLifetimeSeconds;
            _isLaunched = true;
            ApplySpriteFlip();
        }

        public void ResetState()
        {
            _target = null;
            _damage = 0f;
            _lifeRemaining = 0f;
            _isLaunched = false;
        }

        private void Update()
        {
            if (!_isLaunched)
            {
                return;
            }

            _lifeRemaining -= Time.deltaTime;
            if (_lifeRemaining <= 0f)
            {
                ProjectilePoolManager.Return(this);
                return;
            }

            if (_target == null)
            {
                ProjectilePoolManager.Return(this);
                return;
            }

            Vector3 toTarget = _target.position - transform.position;
            float distance = toTarget.magnitude;
            if (distance <= impactDistance)
            {
                HitTarget(_target);
                return;
            }

            Vector3 direction = toTarget.normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            if (rotateToVelocity && direction.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffsetDegrees);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isLaunched || other == null)
            {
                return;
            }

            Transform hitTransform = other.transform;
            if (_target != null && (hitTransform == _target || hitTransform.IsChildOf(_target)))
            {
                HitTarget(_target);
                return;
            }

            EnemyTag enemy = other.GetComponent<EnemyTag>();
            if (enemy == null)
            {
                enemy = other.GetComponentInParent<EnemyTag>();
            }

            if (enemy != null)
            {
                HitTarget(enemy.transform);
            }
        }

        private void HitTarget(Transform hitTransform)
        {
            if (!_isLaunched)
            {
                return;
            }

            HealthEntity health = hitTransform != null ? hitTransform.GetComponent<HealthEntity>() : null;
            if (health == null && hitTransform != null)
            {
                health = hitTransform.GetComponentInParent<HealthEntity>();
            }

            if (health != null)
            {
                health.TakeDamage(_damage);
            }

            ProjectilePoolManager.Return(this);
        }

        private void ApplySpriteFlip()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.flipX = flipX;
            spriteRenderer.flipY = flipY;
        }
    }
}
