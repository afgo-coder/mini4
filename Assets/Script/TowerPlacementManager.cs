using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Mini4.Tower
{
    public class TowerPlacementManager : MonoBehaviour
    {
        private enum BuildMode
        {
            None,
            Attack,
            Population
        }

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Tilemap roadTilemap;
        [SerializeField] private Collider2D insideGroundCollider;
        [SerializeField] private TowerSystemManager towerSystemManager;

        [Header("Parents")]
        [SerializeField] private Transform attackTowerParent;
        [SerializeField] private Transform populationTowerParent;
        [SerializeField] private GameObject populationTowerPrefab;

        private readonly HashSet<Vector3Int> _populationTowerCells = new HashSet<Vector3Int>();
        private BuildMode _mode = BuildMode.None;
        private AttackTowerType _selectedAttackType;

        public bool IsPopulationTowerCell(Vector3Int cell) => _populationTowerCells.Contains(cell);

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (_mode == BuildMode.None)
            {
                return;
            }

            if (!IsPrimaryPointerDown())
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (!TryGetPointerScreenPosition(out Vector2 screenPosition))
            {
                return;
            }

            TryPlaceAtScreenPosition(screenPosition);
        }

        public void SelectAttackTower(int attackType)
        {
            _mode = BuildMode.Attack;
            _selectedAttackType = (AttackTowerType)attackType;
        }

        public void SelectPopulationTower()
        {
            _mode = BuildMode.Population;
        }

        public void CancelBuildMode()
        {
            _mode = BuildMode.None;
        }

        private void TryPlaceAtScreenPosition(Vector2 screenPosition)
        {
            if (mainCamera == null || roadTilemap == null || towerSystemManager == null)
            {
                return;
            }

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
            worldPos.z = 0f;
            Vector3Int cell = roadTilemap.WorldToCell(worldPos);
            Vector3 cellCenter = roadTilemap.GetCellCenterWorld(cell);

            if (_mode == BuildMode.Attack)
            {
                if (!roadTilemap.HasTile(cell))
                {
                    return;
                }

                if (towerSystemManager.TryBuildAttackTower(_selectedAttackType, cellCenter, Quaternion.identity, attackTowerParent, out _))
                {
                    CancelBuildMode();
                }

                return;
            }

            if (_mode == BuildMode.Population)
            {
                if (roadTilemap.HasTile(cell))
                {
                    return;
                }

                if (insideGroundCollider != null && !insideGroundCollider.OverlapPoint(cellCenter))
                {
                    return;
                }

                if (_populationTowerCells.Contains(cell))
                {
                    return;
                }

                if (!towerSystemManager.TryBuildPopulationTower())
                {
                    return;
                }

                if (populationTowerPrefab != null)
                {
                    Instantiate(populationTowerPrefab, cellCenter, Quaternion.identity, populationTowerParent);
                }

                _populationTowerCells.Add(cell);
                CancelBuildMode();
            }
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



