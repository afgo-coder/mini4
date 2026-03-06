using Mini4.Combat;
using UnityEngine;

namespace Mini4.Tower
{
    /// <summary>
    /// Creates and updates a simple HP bar above each tower.
    /// </summary>
    public class TowerHealthBarUI : MonoBehaviour
    {
        [SerializeField] private HealthEntity healthEntity;
        [SerializeField] private Vector2 barSize = new Vector2(0.95f, 0.11f);
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.7f, 0f);
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.45f);
        [SerializeField] private Color fillColor = new Color(0.2f, 1f, 0.25f, 0.9f);
        [SerializeField] private int sortingOrder = 20;

        private static Sprite _pixelSprite;

        private Transform _barRoot;
        private Transform _fillTransform;

        private void Awake()
        {
            if (healthEntity == null)
            {
                healthEntity = GetComponent<HealthEntity>();
            }

            EnsureBarObjects();
        }

        private void OnEnable()
        {
            if (healthEntity != null)
            {
                healthEntity.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(healthEntity.CurrentHp, healthEntity.MaxHp);
            }
        }

        private void OnDisable()
        {
            if (healthEntity != null)
            {
                healthEntity.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void EnsureBarObjects()
        {
            if (_barRoot != null && _fillTransform != null)
            {
                return;
            }

            Transform existing = transform.Find("TowerHealthBar");
            if (existing != null)
            {
                _barRoot = existing;
                Transform existingFill = _barRoot.Find("Fill");
                if (existingFill != null)
                {
                    _fillTransform = existingFill;
                    return;
                }
            }

            _barRoot = new GameObject("TowerHealthBar").transform;
            _barRoot.SetParent(transform, false);
            _barRoot.localPosition = localOffset;

            Transform background = new GameObject("Background").transform;
            background.SetParent(_barRoot, false);
            SpriteRenderer bgRenderer = background.gameObject.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = GetPixelSprite();
            bgRenderer.color = backgroundColor;
            bgRenderer.sortingOrder = sortingOrder;
            background.localScale = new Vector3(barSize.x, barSize.y, 1f);

            _fillTransform = new GameObject("Fill").transform;
            _fillTransform.SetParent(_barRoot, false);
            SpriteRenderer fillRenderer = _fillTransform.gameObject.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = GetPixelSprite();
            fillRenderer.color = fillColor;
            fillRenderer.sortingOrder = sortingOrder + 1;

            HandleHealthChanged(healthEntity != null ? healthEntity.CurrentHp : 0f, healthEntity != null ? healthEntity.MaxHp : 0f);
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (_barRoot == null || _fillTransform == null)
            {
                return;
            }

            _barRoot.localPosition = localOffset;

            float ratio = max <= 0f ? 0f : Mathf.Clamp01(current / max);
            float width = barSize.x * ratio;

            _fillTransform.localScale = new Vector3(width, barSize.y, 1f);
            _fillTransform.localPosition = new Vector3((-barSize.x * 0.5f) + (width * 0.5f), 0f, -0.01f);
            _fillTransform.gameObject.SetActive(ratio > 0f);
        }

        private static Sprite GetPixelSprite()
        {
            if (_pixelSprite != null)
            {
                return _pixelSprite;
            }

            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();

            _pixelSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _pixelSprite;
        }
    }
}

