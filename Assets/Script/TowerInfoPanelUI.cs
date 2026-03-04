using Mini4.Combat;
using Mini4.Tower;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Mini4.UI
{
    /// <summary>
    /// 설치된 타워 클릭 시 정보/강화 확인 UI를 표시.
    /// TowerUI / ConfirmUI를 번갈아 켜는 전환 흐름.
    /// </summary>
    public class TowerInfoPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TowerSystemManager towerSystemManager;
        [SerializeField] private Camera mainCamera;

        [Header("Click Detection")]
        [SerializeField] private LayerMask towerClickMask = Physics2D.DefaultRaycastLayers;
        [SerializeField] private bool blockWhenPointerOverUI;

        [Header("TowerUI")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private Image towerImage;
        [SerializeField] private TMP_Text statText;

        [Header("ConfirmUI")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private GameObject confirmPanel;
        [SerializeField] private TMP_Text confirmHeaderText;
        [SerializeField] private Image confirmTowerImage;
        [SerializeField] private TMP_Text confirmStatText;
        [SerializeField] private TMP_Text confirmExplainText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        [Header("Panel Close Hit Area")]
        [SerializeField] private RectTransform towerBackgroundRect;
        [SerializeField] private RectTransform confirmBackgroundRect;

        private TowerInstance _selectedTower;

        private void Awake()
        {
            if (towerSystemManager == null)
            {
                towerSystemManager = FindObjectOfType<TowerSystemManager>();
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(OpenUpgradeConfirm);
            }

            // 미연결 상태 자동 보정 (권장 하이어라키: TowerUI / ConfirmUI)
            if (panelRoot == null)
            {
                Transform t = transform.Find("TowerUI");
                panelRoot = t != null ? t.gameObject : null;
            }

            if (confirmPanel == null)
            {
                Transform t = transform.Find("ConfirmUI");
                confirmPanel = t != null ? t.gameObject : null;
            }

            if (towerBackgroundRect == null && panelRoot != null)
            {
                Transform t = panelRoot.transform.Find("BackGround");
                towerBackgroundRect = t != null ? t.GetComponent<RectTransform>() : null;
            }

            if (confirmBackgroundRect == null && confirmPanel != null)
            {
                Transform t = confirmPanel.transform.Find("BackGround");
                confirmBackgroundRect = t != null ? t.GetComponent<RectTransform>() : null;
            }

            // ConfirmUI도 TowerUI와 동일한 구조(헤더/스탯/이미지/설명/Yes/No)를 자동 참조
            if (confirmPanel != null)
            {
                if (confirmHeaderText == null)
                {
                    Transform t = confirmPanel.transform.Find("Header");
                    confirmHeaderText = t != null ? t.GetComponent<TMP_Text>() : null;
                }

                if (confirmStatText == null)
                {
                    Transform t = confirmPanel.transform.Find("Stat");
                    confirmStatText = t != null ? t.GetComponent<TMP_Text>() : null;
                }

                if (confirmTowerImage == null)
                {
                    Transform t = confirmPanel.transform.Find("Image");
                    confirmTowerImage = t != null ? t.GetComponent<Image>() : null;
                }

                if (confirmExplainText == null)
                {
                    Transform t = confirmPanel.transform.Find("Explane");
                    if (t == null)
                    {
                        t = confirmPanel.transform.Find("Explain");
                    }

                    confirmExplainText = t != null ? t.GetComponent<TMP_Text>() : null;
                }

                if (confirmYesButton == null)
                {
                    Transform t = confirmPanel.transform.Find("Yes");
                    confirmYesButton = t != null ? t.GetComponent<Button>() : null;
                }

                if (confirmNoButton == null)
                {
                    Transform t = confirmPanel.transform.Find("No");
                    confirmNoButton = t != null ? t.GetComponent<Button>() : null;
                }
            }

            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.AddListener(OnConfirmUpgradeYes);
            }

            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.AddListener(OnConfirmUpgradeNo);
            }

            ShowOnlyTowerPanel(false);
            ShowOnlyConfirmPanel(false);
        }

        private void OnEnable()
        {
            TowerInstance.OnTowerClicked += HandleTowerClicked;
        }

        private void OnDisable()
        {
            TowerInstance.OnTowerClicked -= HandleTowerClicked;
        }

        private void Update()
        {
            if (TryClosePanelOnOutsideClick())
            {
                return;
            }

            TrySelectTowerByWorldClick();

            if (_selectedTower == null || panelRoot == null || !panelRoot.activeSelf)
            {
                return;
            }

            RefreshSelectedTowerInfo();
        }

        public void OnConfirmUpgradeYes()
        {
            if (_selectedTower != null && towerSystemManager != null)
            {
                towerSystemManager.TryUpgrade(_selectedTower, out _);
            }

            RefreshSelectedTowerInfo();
            ShowOnlyConfirmPanel(false);
            ShowOnlyTowerPanel(_selectedTower != null);
        }

        public void OnConfirmUpgradeNo()
        {
            ShowOnlyConfirmPanel(false);
            ShowOnlyTowerPanel(_selectedTower != null);
        }

        public void CloseTowerInfo()
        {
            _selectedTower = null;
            ShowOnlyTowerPanel(false);
            ShowOnlyConfirmPanel(false);
        }


        private bool TryClosePanelOnOutsideClick()
        {
            if (!IsPrimaryPointerDown())
            {
                return false;
            }

            bool isTowerOpen = panelRoot != null && panelRoot.activeSelf;
            bool isConfirmOpen = confirmPanel != null && confirmPanel.activeSelf;
            if (!isTowerOpen && !isConfirmOpen)
            {
                return false;
            }

            if (!TryGetPointerScreenPosition(out Vector2 screenPosition))
            {
                return false;
            }

            bool isInsideTowerBackground = isTowerOpen && IsInsideBackgroundRect(towerBackgroundRect, screenPosition);
            bool isInsideConfirmBackground = isConfirmOpen && IsInsideBackgroundRect(confirmBackgroundRect, screenPosition);
            if (isInsideTowerBackground || isInsideConfirmBackground)
            {
                return false;
            }

            CloseTowerInfo();
            return true;
        }

        private void TrySelectTowerByWorldClick()
        {
            if (!IsPrimaryPointerDown())
            {
                return;
            }

            if (blockWhenPointerOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                return;
            }

            if (!TryGetPointerScreenPosition(out Vector2 screenPosition))
            {
                return;
            }

            Vector3 world3 = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
            Vector2 world = new Vector2(world3.x, world3.y);

            Collider2D[] hits = Physics2D.OverlapPointAll(world, towerClickMask);
            if (hits == null || hits.Length == 0)
            {
                return;
            }

            TowerInstance best = null;
            float nearest = float.MaxValue;
            foreach (Collider2D hit in hits)
            {
                if (hit == null)
                {
                    continue;
                }

                TowerInstance tower = hit.GetComponent<TowerInstance>();
                if (tower == null)
                {
                    tower = hit.GetComponentInParent<TowerInstance>();
                }

                if (tower == null)
                {
                    continue;
                }

                float distance = Vector2.Distance(world, tower.transform.position);
                if (distance < nearest)
                {
                    nearest = distance;
                    best = tower;
                }
            }

            if (best != null)
            {
                HandleTowerClicked(best);
            }
        }

        private void HandleTowerClicked(TowerInstance tower)
        {
            _selectedTower = tower;
            ShowOnlyConfirmPanel(false);
            ShowOnlyTowerPanel(tower != null);
            RefreshSelectedTowerInfo();
        }

        private void RefreshSelectedTowerInfo()
        {
            if (_selectedTower == null)
            {
                return;
            }

            if (headerText != null)
            {
                headerText.text = $"{_selectedTower.DisplayName} +{_selectedTower.Level}";
            }

            if (towerImage != null)
            {
                SpriteRenderer sr = _selectedTower.GetComponentInChildren<SpriteRenderer>();
                towerImage.sprite = sr != null ? sr.sprite : null;
            }

            float hpCurrent = 0f;
            float hpMax = 0f;
            HealthEntity hp = _selectedTower.GetComponent<HealthEntity>();
            if (hp != null)
            {
                hpCurrent = hp.CurrentHp;
                hpMax = hp.MaxHp;
            }

            float range = 0f;
            TowerAutoAttack autoAttack = _selectedTower.GetComponent<TowerAutoAttack>();
            if (autoAttack != null)
            {
                range = autoAttack.AttackRange;
            }

            if (statText != null)
            {
                statText.text = BuildBaseStatText(_selectedTower.CurrentAttack, hpCurrent, hpMax, range);
            }

            if (upgradeButton != null)
            {
                bool canPreview = towerSystemManager != null &&
                                  towerSystemManager.TryGetUpgradePreview(_selectedTower, out _, out _, out _, out _, out _);
                upgradeButton.interactable = canPreview;
            }
        }

        private void OpenUpgradeConfirm()
        {
            if (_selectedTower == null || towerSystemManager == null || confirmPanel == null)
            {
                return;
            }

            if (!towerSystemManager.TryGetUpgradePreview(_selectedTower, out _, out int cost, out float add, out float percent, out float successRate))
            {
                if (confirmHeaderText != null)
                {
                    confirmHeaderText.text = $"{_selectedTower.DisplayName} +{_selectedTower.Level}";
                }

                if (confirmTowerImage != null)
                {
                    SpriteRenderer sr = _selectedTower.GetComponentInChildren<SpriteRenderer>();
                    confirmTowerImage.sprite = sr != null ? sr.sprite : null;
                }

                if (confirmStatText != null)
                {
                    FillConfirmStatText(false, 0f, 0f);
                }

                if (confirmExplainText != null)
                {
                    confirmExplainText.text = "이미 최대 강화(+5)입니다.";
                }

                ShowOnlyTowerPanel(false);
                ShowOnlyConfirmPanel(true);
                return;
            }

            if (confirmHeaderText != null)
            {
                confirmHeaderText.text = $"{_selectedTower.DisplayName} +{_selectedTower.Level}";
            }

            if (confirmTowerImage != null)
            {
                SpriteRenderer sr = _selectedTower.GetComponentInChildren<SpriteRenderer>();
                confirmTowerImage.sprite = sr != null ? sr.sprite : null;
            }

            if (confirmStatText != null)
            {
                FillConfirmStatText(true, add, percent);
            }

            if (confirmExplainText != null)
            {
                float successDisplay = successRate * 100f;
                confirmExplainText.text =
                    $"Use Cost: {cost}, Meal+1\n" +
                    $"Success Chance: {successDisplay:0.#}%\n\n" +
                    "Upgrade this tower?";
            }

            ShowOnlyTowerPanel(false);
            ShowOnlyConfirmPanel(true);
        }

        private void ShowOnlyTowerPanel(bool show)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(show);
            }
        }

        private void ShowOnlyConfirmPanel(bool show)
        {
            if (confirmPanel != null)
            {
                confirmPanel.SetActive(show);
            }
        }


        private static bool IsInsideBackgroundRect(RectTransform rect, Vector2 screenPosition)
        {
            return rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, null);
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

        private static string BuildBaseStatText(float attack, float hpCurrent, float hpMax, float range)
        {
            return
                $"Atk: {attack:0.0}\n" +
                $"HP: {hpCurrent:0.0} / {hpMax:0.0}\n" +
                $"Range: {range:0.00}";
        }

        private void FillConfirmStatText(bool showPreview, float add, float percent)
        {
            if (_selectedTower == null || confirmStatText == null)
            {
                return;
            }

            float hpCurrent = 0f;
            float hpMax = 0f;
            HealthEntity hp = _selectedTower.GetComponent<HealthEntity>();
            if (hp != null)
            {
                hpCurrent = hp.CurrentHp;
                hpMax = hp.MaxHp;
            }

            float range = 0f;
            TowerAutoAttack autoAttack = _selectedTower.GetComponent<TowerAutoAttack>();
            if (autoAttack != null)
            {
                range = autoAttack.AttackRange;
            }

            float currentAttack = _selectedTower.CurrentAttack;
            float attackDelta = showPreview ? add + (currentAttack * percent) : 0f;
            string attackDeltaText = showPreview ? $" <color=#55FF55FF>+{attackDelta:0.0}</color>" : string.Empty;

            confirmStatText.text =
                $"Atk: {currentAttack:0.0}{attackDeltaText}\n" +
                $"HP: {hpCurrent:0.0} / {hpMax:0.0}\n" +
                $"Range: {range:0.00}";
        }
    }
}
