using UnityEngine;
using UnityEngine.UI;
using ArcheageLike.Core;

namespace ArcheageLike.UI
{
    /// <summary>
    /// World-space floating damage number. Created at runtime — no prefab needed.
    /// </summary>
    public class RuntimeDamagePopup : MonoBehaviour
    {
        private Text _text;
        private float _timer;
        private float _lifetime = 1.5f;
        private Vector3 _velocity;
        private Camera _cam;

        public static void Spawn(Vector3 worldPos, float amount, DamageType type, bool isCrit = false)
        {
            var go = new GameObject("DmgPopup");
            var popup = go.AddComponent<RuntimeDamagePopup>();
            popup.Init(worldPos, amount, type, isCrit);
        }

        public static void SpawnHeal(Vector3 worldPos, float amount)
        {
            var go = new GameObject("HealPopup");
            var popup = go.AddComponent<RuntimeDamagePopup>();
            popup.InitHeal(worldPos, amount);
        }

        private void Init(Vector3 worldPos, float amount, DamageType type, bool isCrit)
        {
            _cam = Camera.main;

            // Create world-space canvas
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = _cam;

            var rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(2, 0.5f);
            transform.position = worldPos + Vector3.up * 2f;
            transform.localScale = Vector3.one * 0.02f;

            // Text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(transform, false);
            _text = textGO.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.alignment = TextAnchor.MiddleCenter;
            _text.horizontalOverflow = HorizontalWrapMode.Overflow;
            _text.verticalOverflow = VerticalWrapMode.Overflow;

            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            if (isCrit)
            {
                _text.text = $"{Mathf.CeilToInt(amount)}!";
                _text.fontSize = 42;
                _text.color = Color.yellow;
            }
            else
            {
                _text.text = $"{Mathf.CeilToInt(amount)}";
                _text.fontSize = 30;
                _text.color = type == DamageType.Physical ? Color.white : new Color(0.6f, 0.6f, 1f);
            }

            _velocity = new Vector3(Random.Range(-0.5f, 0.5f), 2f, 0f);
            _timer = _lifetime;
        }

        private void InitHeal(Vector3 worldPos, float amount)
        {
            _cam = Camera.main;
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = _cam;

            var rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(2, 0.5f);
            transform.position = worldPos + Vector3.up * 2f;
            transform.localScale = Vector3.one * 0.02f;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(transform, false);
            _text = textGO.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.text = $"+{Mathf.CeilToInt(amount)}";
            _text.fontSize = 28;
            _text.color = Color.green;
            _text.alignment = TextAnchor.MiddleCenter;
            _text.horizontalOverflow = HorizontalWrapMode.Overflow;

            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            _velocity = new Vector3(0f, 2.5f, 0f);
            _timer = _lifetime;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            transform.position += _velocity * Time.deltaTime;
            _velocity *= 0.95f;

            // Billboard
            if (_cam != null)
                transform.rotation = _cam.transform.rotation;

            // Fade
            if (_text != null)
            {
                var c = _text.color;
                c.a = Mathf.Clamp01(_timer / _lifetime);
                _text.color = c;
            }

            if (_timer <= 0f)
                Destroy(gameObject);
        }
    }
}
