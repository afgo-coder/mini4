using System;
using System.Collections;
using UnityEngine;

namespace Mini4.Combat
{
    public class HealthEntity : MonoBehaviour
    {
        [SerializeField] private float maxHp = 20f;
        [SerializeField] private SpriteRenderer blinkRenderer;
        [SerializeField] private Color blinkColor = new Color(1f, 0.45f, 0.45f, 1f);
        [SerializeField] private float blinkDuration = 0.08f;
        [SerializeField] private Transform hpBarFill;

        public event Action<float, float> OnHealthChanged;
        public event Action<HealthEntity> OnDied;

        public float CurrentHp { get; private set; }
        public float MaxHp => maxHp;

        private Coroutine _blinkRoutine;

        private void Awake()
        {
            CurrentHp = maxHp;
            if (blinkRenderer == null)
            {
                blinkRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            NotifyHealthChanged();
        }

        public void Initialize(float hp)
        {
            maxHp = Mathf.Max(1f, hp);
            CurrentHp = maxHp;
            NotifyHealthChanged();
        }

        public void TakeDamage(float damage)
        {
            if (damage <= 0f || CurrentHp <= 0f)
            {
                return;
            }

            CurrentHp = Mathf.Max(0f, CurrentHp - damage);
            NotifyHealthChanged();
            Blink();

            if (CurrentHp <= 0f)
            {
                OnDied?.Invoke(this);
                Destroy(gameObject);
            }
        }

        private void NotifyHealthChanged()
        {
            OnHealthChanged?.Invoke(CurrentHp, maxHp);
            if (hpBarFill == null)
            {
                return;
            }

            float ratio = maxHp <= 0f ? 0f : CurrentHp / maxHp;
            Vector3 scale = hpBarFill.localScale;
            scale.x = Mathf.Clamp01(ratio);
            hpBarFill.localScale = scale;
        }

        private void Blink()
        {
            if (blinkRenderer == null)
            {
                return;
            }

            if (_blinkRoutine != null)
            {
                StopCoroutine(_blinkRoutine);
            }

            _blinkRoutine = StartCoroutine(BlinkRoutine());
        }

        private IEnumerator BlinkRoutine()
        {
            Color origin = blinkRenderer.color;
            blinkRenderer.color = blinkColor;
            yield return new WaitForSeconds(blinkDuration);
            if (blinkRenderer != null)
            {
                blinkRenderer.color = origin;
            }
        }
    }
}

