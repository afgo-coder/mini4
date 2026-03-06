using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

        [Header("Placement Visual Offset")]
        [SerializeField] private float attackTowerYOffset = 0.28f;
        [SerializeField] private float populationTowerYOffset = 0.22f;
        [SerializeField] private float previewZOffset = -0.05f;

        [Header("Feedback")]
        [SerializeField] private TMP_Text placementMessageText;
        [SerializeField] private float placementMessageSeconds = 1.1f;

        private readonly HashSet<Vector3Int> _populationTowerCells = new HashSet<Vector3Int>();
        private readonly HashSet<Vector3Int> _attackTowerCells = new HashSet<Vector3Int>();

        private BuildMode _mode = BuildMode.None;
        private AttackTowerType _selectedAttackType;

        private GameObject _previewObject;
        private SpriteRenderer _previewRenderer;
        private GameObject _cellOverlayObject;
        private SpriteRenderer _cellOverlayRenderer;
        private GameObject _rangeOverlayObject;
        private SpriteRenderer _rangeOverlayRenderer;
        private Coroutine _messageRoutine;
        private Sprite _squareSprite;
        private Sprite _circleSprite;
        private float _previewAttackRange;

        public bool IsPopulationTowerCell(Vector3Int cell) => _populationTowerCells.Contains(cell);

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            EnsureOverlayObjects();
            HidePlacementVisuals();

            if (placementMessageText != null)
            {
                placementMessageText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (_mode == BuildMode.None)
            {
                return;
            }

            if (!TryGetPointerScreenPosition(out Vector2 screenPosition))
            {
                return;
            }

            UpdatePlacementVisuals(screenPosition);

            if (!IsPrimaryPointerDown())
            {
                return;
            }

            TryPlaceAtScreenPosition(screenPosition);
        }

        public void SelectAttackTower(int attackType)
        {
            _mode = BuildMode.Attack;
            _selectedAttackType = (AttackTowerType)attackType;
            _previewAttackRange = 0f;
            if (towerSystemManager != null)
            {
                towerSystemManager.TryGetTowerRangeByType(_selectedAttackType, out _previewAttackRange);
            }

            CreatePreviewForCurrentMode();
        }

        public void SelectPopulationTower()
        {
            _mode = BuildMode.Population;
            _previewAttackRange = 0f;
            CreatePreviewForCurrentMode();
        }

        public void CancelBuildMode()
        {
            _mode = BuildMode.None;
            HidePlacementVisuals();
        }

        private void TryPlaceAtScreenPosition(Vector2 screenPosition)
        {
            if (!TryGetCellFromScreen(screenPosition, out Vector3Int cell, out Vector3 cellCenter))
            {
                return;
            }

            bool isValid = IsCurrentPlacementValid(cell, cellCenter);
            if (!isValid)
            {
                ShowPlacementMessage("Invalid build position.");
                return;
            }

            if (_mode == BuildMode.Attack)
            {
                if (!towerSystemManager.CanBuildAttackTower(_selectedAttackType))
                {
                    ShowPlacementMessage("Not enough resources or population.");
                    return;
                }

                Vector3 placePos = cellCenter + new Vector3(0f, attackTowerYOffset, 0f);
                if (!towerSystemManager.TryBuildAttackTower(_selectedAttackType, placePos, Quaternion.identity, attackTowerParent, out _))
                {
                    ShowPlacementMessage("Failed to place tower.");
                    return;
                }

                _attackTowerCells.Add(cell);
                CancelBuildMode();
                return;
            }

            if (!towerSystemManager.CanBuildPopulationTower())
            {
                ShowPlacementMessage("Not enough gold.");
                return;
            }

            if (!towerSystemManager.TryBuildPopulationTower())
            {
                ShowPlacementMessage("Failed to place tower.");
                return;
            }

            if (populationTowerPrefab != null)
            {
                Vector3 placePos = cellCenter + new Vector3(0f, populationTowerYOffset, 0f);
                Instantiate(populationTowerPrefab, placePos, Quaternion.identity, populationTowerParent);
            }

            _populationTowerCells.Add(cell);
            CancelBuildMode();
        }

        private bool IsCurrentPlacementValid(Vector3Int cell, Vector3 cellCenter)
        {
            if (roadTilemap == null || towerSystemManager == null)
            {
                return false;
            }

            if (_mode == BuildMode.Attack)
            {
                if (!roadTilemap.HasTile(cell))
                {
                    return false;
                }

                return !_attackTowerCells.Contains(cell) && !_populationTowerCells.Contains(cell);
            }

            if (roadTilemap.HasTile(cell))
            {
                return false;
            }

            if (insideGroundCollider != null && !insideGroundCollider.OverlapPoint(cellCenter))
            {
                return false;
            }

            return !_populationTowerCells.Contains(cell) && !_attackTowerCells.Contains(cell);
        }

        private void UpdatePlacementVisuals(Vector2 screenPosition)
        {
            if (!TryGetCellFromScreen(screenPosition, out Vector3Int cell, out Vector3 cellCenter))
            {
                HidePlacementVisuals();
                return;
            }

            bool valid = IsCurrentPlacementValid(cell, cellCenter);
            EnsureOverlayObjects();

            _cellOverlayObject.SetActive(true);
            _cellOverlayObject.transform.position = cellCenter;
            _cellOverlayRenderer.color = valid ? new Color(0.1f, 1f, 0.2f, 0.28f) : new Color(1f, 0.2f, 0.2f, 0.28f);

            if (_previewObject != null)
            {
                _previewObject.SetActive(true);
                float yOffset = _mode == BuildMode.Attack ? attackTowerYOffset : populationTowerYOffset;
                _previewObject.transform.position = cellCenter + new Vector3(0f, yOffset, previewZOffset);
                if (_previewRenderer != null)
                {
                    _previewRenderer.color = valid ? new Color(1f, 1f, 1f, 0.65f) : new Color(1f, 0.5f, 0.5f, 0.65f);
                }
            }

            UpdateAttackRangeOverlay(cellCenter, valid);
        }

        private bool TryGetCellFromScreen(Vector2 screenPosition, out Vector3Int cell, out Vector3 center)
        {
            cell = default;
            center = default;
            if (mainCamera == null || roadTilemap == null)
            {
                return false;
            }

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
            worldPos.z = 0f;
            cell = roadTilemap.WorldToCell(worldPos);
            center = roadTilemap.GetCellCenterWorld(cell);
            return true;
        }

        private void CreatePreviewForCurrentMode()
        {
            EnsureOverlayObjects();

            if (_previewObject != null)
            {
                Destroy(_previewObject);
            }

            GameObject prefab = null;
            if (_mode == BuildMode.Attack && towerSystemManager != null)
            {
                prefab = towerSystemManager.GetAttackTowerPrefab(_selectedAttackType);
            }
            else if (_mode == BuildMode.Population)
            {
                prefab = populationTowerPrefab;
            }

            if (prefab == null)
            {
                _previewObject = null;
                _previewRenderer = null;
                return;
            }

            _previewObject = Instantiate(prefab);
            _previewObject.name = $"Preview_{prefab.name}";

            foreach (Collider2D col in _previewObject.GetComponentsInChildren<Collider2D>())
            {
                col.enabled = false;
            }

            foreach (MonoBehaviour behaviour in _previewObject.GetComponentsInChildren<MonoBehaviour>())
            {
                if (behaviour == null)
                {
                    continue;
                }

                behaviour.enabled = false;
            }

            _previewRenderer = _previewObject.GetComponentInChildren<SpriteRenderer>();
            if (_previewRenderer != null)
            {
                _previewRenderer.color = new Color(1f, 1f, 1f, 0.65f);
                _previewRenderer.sortingOrder += 20;
            }
        }

        private void EnsureOverlayObjects()
        {
            if (_squareSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _squareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }

            if (_cellOverlayObject == null)
            {
                _cellOverlayObject = new GameObject("PlacementCellOverlay");
                _cellOverlayRenderer = _cellOverlayObject.AddComponent<SpriteRenderer>();
                _cellOverlayRenderer.sprite = _squareSprite;
                _cellOverlayRenderer.sortingOrder = 100;
                _cellOverlayObject.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            if (_circleSprite == null)
            {
                _circleSprite = BuildCircleSprite();
            }

            if (_rangeOverlayObject == null)
            {
                _rangeOverlayObject = new GameObject("PlacementRangeOverlay");
                _rangeOverlayRenderer = _rangeOverlayObject.AddComponent<SpriteRenderer>();
                _rangeOverlayRenderer.sprite = _circleSprite;
                _rangeOverlayRenderer.sortingOrder = 99;
                _rangeOverlayRenderer.color = new Color(0.1f, 0.8f, 1f, 0.18f);
            }
        }

        private void HidePlacementVisuals()
        {
            if (_previewObject != null)
            {
                Destroy(_previewObject);
                _previewObject = null;
                _previewRenderer = null;
            }

            if (_cellOverlayObject != null)
            {
                _cellOverlayObject.SetActive(false);
            }

            if (_rangeOverlayObject != null)
            {
                _rangeOverlayObject.SetActive(false);
            }
        }

        private void UpdateAttackRangeOverlay(Vector3 cellCenter, bool isValid)
        {
            if (_rangeOverlayObject == null || _rangeOverlayRenderer == null)
            {
                return;
            }

            bool show = _mode == BuildMode.Attack && _previewAttackRange > 0.01f;
            _rangeOverlayObject.SetActive(show);
            if (!show)
            {
                return;
            }

            _rangeOverlayObject.transform.position = cellCenter + new Vector3(0f, attackTowerYOffset - 0.25f, 0f);
            _rangeOverlayObject.transform.localScale = new Vector3(_previewAttackRange * 2f, _previewAttackRange * 2f, 1f);
            _rangeOverlayRenderer.color = isValid ? new Color(0.1f, 0.8f, 1f, 0.18f) : new Color(1f, 0.3f, 0.3f, 0.18f);
        }

        private static Sprite BuildCircleSprite()
        {
            const int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float radius = (size - 2f) * 0.5f;
            Vector2 center = new Vector2((size - 1f) * 0.5f, (size - 1f) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = distance <= radius ? 1f : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void ShowPlacementMessage(string message)
        {
            if (placementMessageText == null)
            {
                return;
            }

            if (_messageRoutine != null)
            {
                StopCoroutine(_messageRoutine);
            }

            _messageRoutine = StartCoroutine(ShowPlacementMessageRoutine(message));
        }

        private IEnumerator ShowPlacementMessageRoutine(string message)
        {
            placementMessageText.text = message;
            placementMessageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(placementMessageSeconds);
            if (placementMessageText != null)
            {
                placementMessageText.gameObject.SetActive(false);
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

