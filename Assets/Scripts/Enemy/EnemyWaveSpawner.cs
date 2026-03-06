using System.Collections;
using System.Collections.Generic;
using Mini4.Combat;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Mini4.Enemy
{
    /// <summary>
    /// 메인카메라 밖 + Road 타일 외부 위치에만 적 스폰.
    /// </summary>
    public class EnemyWaveSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Mini4.Core.MiniProjectGameManager gameManager;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Tilemap roadTilemap;
        [SerializeField] private Collider2D worldBoundsCollider;

        [Header("Enemy Prefabs (5개 이상 권장)")]
        [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

        [Header("Spawn")]
        [SerializeField] private float baseSpawnInterval = 4f;
        [SerializeField] private int baseSpawnCountPerWave = 1;
        [SerializeField] private float spawnIntervalScalePerStage = 0.9f;
        [SerializeField] private int maxTryPerSpawn = 60;

        [Header("Enemy Default Stats")]
        [SerializeField] private float enemyBaseHp = 20f;

        private int _currentStage = 1;
        private Coroutine _spawnRoutine;

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnEnemyStageChanged += HandleStageChanged;
            }

            _spawnRoutine = StartCoroutine(SpawnLoop());
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnEnemyStageChanged -= HandleStageChanged;
            }

            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
            }
        }

        private void HandleStageChanged(int stage)
        {
            _currentStage = stage;
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                int spawnCount = baseSpawnCountPerWave + (_currentStage - 1);
                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnOne();
                }

                float interval = baseSpawnInterval * Mathf.Pow(spawnIntervalScalePerStage, _currentStage - 1);
                interval = Mathf.Max(0.6f, interval);
                yield return new WaitForSeconds(interval);
            }
        }

        private void SpawnOne()
        {
            if (enemyPrefabs.Count == 0 || mainCamera == null || roadTilemap == null || worldBoundsCollider == null)
            {
                return;
            }

            if (!TryFindSpawnPoint(out Vector2 spawnPos))
            {
                return;
            }

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            if (prefab == null)
            {
                return;
            }

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            EnsureEnemyComponents(enemy);
        }

        private void EnsureEnemyComponents(GameObject enemy)
        {
            if (enemy.GetComponent<EnemyTag>() == null)
            {
                enemy.AddComponent<EnemyTag>();
            }

            HealthEntity hp = enemy.GetComponent<HealthEntity>();
            if (hp == null)
            {
                hp = enemy.AddComponent<HealthEntity>();
            }

            float stageScale = 1f + ((_currentStage - 1) * 0.2f);
            hp.Initialize(enemyBaseHp * stageScale);

            if (enemy.GetComponent<EnemyCombatMover>() == null)
            {
                enemy.AddComponent<EnemyCombatMover>();
            }
        }

        private bool TryFindSpawnPoint(out Vector2 result)
        {
            Bounds worldBounds = worldBoundsCollider.bounds;
            for (int i = 0; i < maxTryPerSpawn; i++)
            {
                Vector2 candidate = new Vector2(
                    Random.Range(worldBounds.min.x, worldBounds.max.x),
                    Random.Range(worldBounds.min.y, worldBounds.max.y));

                if (!IsStrictlyOutsideCamera(candidate))
                {
                    continue;
                }

                if (roadTilemap.HasTile(roadTilemap.WorldToCell(candidate)))
                {
                    continue;
                }

                result = candidate;
                return true;
            }

            result = Vector2.zero;
            return false;
        }

        private bool IsStrictlyOutsideCamera(Vector2 point)
        {
            Vector3 viewport = mainCamera.WorldToViewportPoint(point);
            bool inFront = viewport.z >= 0f;
            if (!inFront)
            {
                return false;
            }

            // 화면 내부 좌표(0~1 범위)는 전부 제외
            bool insideView = viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
            return !insideView;
        }
    }
}
