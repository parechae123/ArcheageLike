using UnityEngine;

namespace ArcheageLike.Utils
{
    /// <summary>
    /// Simple on-screen FPS counter for performance monitoring.
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        private float _deltaTime;

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            int fps = Mathf.CeilToInt(1.0f / _deltaTime);
            float ms = _deltaTime * 1000f;

            var style = new GUIStyle();
            style.fontSize = 18;
            style.normal.textColor = fps > 30 ? Color.green : (fps > 15 ? Color.yellow : Color.red);
            style.alignment = TextAnchor.UpperRight;

            Rect rect = new Rect(Screen.width - 160, 10, 150, 30);
            GUI.Label(rect, $"{fps} FPS ({ms:F1}ms)", style);
        }
    }
}
