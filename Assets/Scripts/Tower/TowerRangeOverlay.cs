using UnityEngine;

namespace Mini4.Tower
{
    /// <summary>
    /// Draws a translucent circle for tower attack range.
    /// Visibility is controlled externally.
    /// </summary>
    public class TowerRangeOverlay : MonoBehaviour
    {
        [SerializeField] private TowerAutoAttack towerAutoAttack;
        [SerializeField] private Color overlayColor = new Color(0.1f, 0.8f, 1f, 0.2f);
        [SerializeField] private float yOffset = -0.25f;
        [SerializeField] private int sortingOrder = 1;

        private static Sprite _circleSprite;
        private SpriteRenderer _overlayRenderer;
        private float _lastRange = -1f;
        private bool _isVisible;

        private void Awake()
        {
            if (towerAutoAttack == null)
            {
                towerAutoAttack = GetComponent<TowerAutoAttack>();
            }

            EnsureOverlayRenderer();
            SetVisible(false);
            RefreshVisual(force: true);
        }

        private void LateUpdate()
        {
            RefreshVisual(force: false);
        }

        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            RefreshVisual(force: true);
        }

        private void EnsureOverlayRenderer()
        {
            if (_overlayRenderer != null)
            {
                return;
            }

            Transform child = transform.Find("RangeOverlay");
            if (child == null)
            {
                GameObject go = new GameObject("RangeOverlay");
                go.transform.SetParent(transform, false);
                child = go.transform;
            }

            _overlayRenderer = child.GetComponent<SpriteRenderer>();
            if (_overlayRenderer == null)
            {
                _overlayRenderer = child.gameObject.AddComponent<SpriteRenderer>();
            }

            _overlayRenderer.sprite = GetCircleSprite();
            _overlayRenderer.sortingOrder = sortingOrder;
            _overlayRenderer.color = overlayColor;
        }

        private void RefreshVisual(bool force)
        {
            if (_overlayRenderer == null)
            {
                return;
            }

            float range = towerAutoAttack != null ? towerAutoAttack.AttackRange : 0f;
            if (!force && Mathf.Abs(_lastRange - range) < 0.001f)
            {
                return;
            }

            _lastRange = range;
            bool visible = _isVisible && range > 0.01f;
            _overlayRenderer.enabled = visible;
            if (!visible)
            {
                return;
            }

            _overlayRenderer.color = overlayColor;
            _overlayRenderer.transform.localPosition = new Vector3(0f, yOffset, 0f);
            _overlayRenderer.transform.localScale = new Vector3(range * 2f, range * 2f, 1f);
        }

        private static Sprite GetCircleSprite()
        {
            if (_circleSprite != null)
            {
                return _circleSprite;
            }

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
            _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _circleSprite;
        }
    }
}
