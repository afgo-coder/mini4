using Mini4.Tower;
using UnityEngine;
using UnityEngine.Tilemaps;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Mini4.Economy
{
    /// <summary>
    /// 성 주변(로드 타일 외 영역)에 버섯을 랜덤 생성하고 클릭 수확으로 골드를 지급.
    /// </summary>
    public class MushroomFieldManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private Transform castleCenter;
        [SerializeField] private Tilemap roadTilemap;
        [SerializeField] private TowerPlacementManager towerPlacementManager;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Collider2D insideGroundCollider;

        [Header("Mushroom Prefabs")]
        [SerializeField] private GameObject mushroom1Prefab;
        [SerializeField] private GameObject mushroom2Prefab;

        [Header("Mushroom Values")]
        [SerializeField] private int mushroom1Gold = 5;
        [SerializeField] private int mushroom2Gold = 8;

        [Header("Spawn Rule")]
        [SerializeField] private int maxMushrooms = 15;
        [SerializeField] private float spawnIntervalSeconds = 2f;
        [SerializeField] private float minRadiusFromCastle = 1.5f;
        [SerializeField] private float maxRadiusFromCastle = 6f;
        [SerializeField] private int maxTryPerSpawn = 40;
        [SerializeField] private float minDistanceBetweenMushrooms = 0.6f;

        private float _elapsed;
        private int _aliveMushrooms;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            HandleHarvestClick();

            if (economyManager == null || castleCenter == null || roadTilemap == null)
            {
                return;
            }

            if (_aliveMushrooms >= maxMushrooms)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            if (_elapsed < spawnIntervalSeconds)
            {
                return;
            }

            _elapsed -= spawnIntervalSeconds;
            TrySpawnMushroom();
        }

        public void Harvest(HarvestableMushroom mushroom, int gold)
        {
            if (mushroom == null)
            {
                return;
            }

            economyManager.AddGold(gold);
            _aliveMushrooms = Mathf.Max(0, _aliveMushrooms - 1);
            Destroy(mushroom.gameObject);
        }

        private void HandleHarvestClick()
        {
            if (!IsPrimaryPointerDown() || mainCamera == null)
            {
                return;
            }

            if (!TryGetPointerScreenPosition(out Vector2 screenPosition))
            {
                return;
            }

            Vector3 world = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
            RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero);
            if (!hit.collider)
            {
                return;
            }

            HarvestableMushroom mushroom = hit.collider.GetComponent<HarvestableMushroom>();
            mushroom?.Harvest();
        }

        private void TrySpawnMushroom()
        {
            for (int i = 0; i < maxTryPerSpawn; i++)
            {
                Vector2 candidate = GetRandomPointAroundCastle();
                if (!IsValidMushroomPosition(candidate))
                {
                    continue;
                }

                SpawnAt(candidate);
                return;
            }
        }

        private Vector2 GetRandomPointAroundCastle()
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minRadiusFromCastle, maxRadiusFromCastle);
            return (Vector2)castleCenter.position + dir * distance;
        }

        private bool IsValidMushroomPosition(Vector2 worldPos)
        {
            Vector3Int cellPos = roadTilemap.WorldToCell(worldPos);
            if (roadTilemap.HasTile(cellPos))
            {
                return false;
            }

            if (insideGroundCollider != null && !insideGroundCollider.OverlapPoint(worldPos))
            {
                return false;
            }

            if (towerPlacementManager != null && towerPlacementManager.IsPopulationTowerCell(cellPos))
            {
                return false;
            }

            HarvestableMushroom[] mushrooms = GetComponentsInChildren<HarvestableMushroom>();
            foreach (HarvestableMushroom mushroom in mushrooms)
            {
                if (Vector2.Distance(worldPos, mushroom.transform.position) < minDistanceBetweenMushrooms)
                {
                    return false;
                }
            }

            return true;
        }

        private void SpawnAt(Vector2 worldPos)
        {
            bool spawnType2 = Random.value < 0.35f;
            GameObject prefab = spawnType2 ? mushroom2Prefab : mushroom1Prefab;
            int value = spawnType2 ? mushroom2Gold : mushroom1Gold;
            if (prefab == null)
            {
                return;
            }

            GameObject go = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            HarvestableMushroom harvestable = go.GetComponent<HarvestableMushroom>();
            if (harvestable == null)
            {
                harvestable = go.AddComponent<HarvestableMushroom>();
            }

            Collider2D clickCollider = go.GetComponent<Collider2D>();
            if (clickCollider == null)
            {
                CircleCollider2D circle = go.AddComponent<CircleCollider2D>();
                circle.radius = 0.25f;
                circle.isTrigger = false;
            }

            harvestable.Initialize(this, value);
            _aliveMushrooms++;
        }

        private static bool IsPrimaryPointerDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private static bool TryGetPointerScreenPosition(out Vector2 screenPosition)
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null)
            {
                screenPosition = default;
                return false;
            }

            screenPosition = Mouse.current.position.ReadValue();
            return true;
#else
            screenPosition = Input.mousePosition;
            return true;
#endif
        }
    }
}


