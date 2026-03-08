using UnityEngine;
using UnityEngine.UI;
using ArcheageLike.Character;
using ArcheageLike.Combat;
using ArcheageLike.Core;

namespace ArcheageLike.UI
{
    /// <summary>
    /// Builds the entire game UI at runtime via code.
    /// Creates Canvas, HP/MP bars, skill bar, target frame, minimap, etc.
    /// </summary>
    public class RuntimeUIBuilder : MonoBehaviour
    {
        private Canvas _canvas;
        private HUDManager _hud;

        // References built at runtime
        private Slider _healthBar;
        private Slider _manaBar;
        private Slider _staminaBar;
        private Text _healthText;
        private Text _manaText;
        private GameObject _targetFrame;
        private Text _targetNameText;
        private Slider _targetHealthBar;
        private Text _targetHealthText;
        private SkillSlotUI[] _skillSlots;
        private GameObject _shipHUD;
        private Slider _shipHealthBar;
        private Text _shipSpeedText;
        private Text _gameStateText;
        private Text _interactionPromptText;
        private GameObject _castBarObj;
        private Slider _castBar;

        private void Start()
        {
            BuildUI();
        }

        public void BuildUI()
        {
            CreateCanvas();
            CreatePlayerFrame();
            CreateTargetFrame();
            CreateSkillBar();
            CreateShipHUD();
            CreateCastBar();
            CreateInteractionPrompt();
            CreateStateDisplay();
            CreateCrosshair();
            WireHUDManager();

            Debug.Log("[UI] Runtime UI built successfully.");
        }

        // ===== Canvas =====
        private void CreateCanvas()
        {
            var canvasGO = new GameObject("GameCanvas");
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // ===== Player Frame (top-left) =====
        private void CreatePlayerFrame()
        {
            var frame = CreatePanel("PlayerFrame", _canvas.transform,
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(10, -10), new Vector2(320, 100));

            SetPanelColor(frame, new Color(0, 0, 0, 0.6f));

            // Player name
            CreateText("PlayerName", frame.transform,
                new Vector2(10, -5), new Vector2(200, 25),
                "Player", 16, Color.white, TextAnchor.MiddleLeft);

            // HP Bar
            _healthBar = CreateBar("HealthBar", frame.transform,
                new Vector2(10, -35), new Vector2(290, 22),
                new Color(0.8f, 0.15f, 0.15f), new Color(0.3f, 0.05f, 0.05f));

            _healthText = CreateText("HealthText", _healthBar.transform,
                Vector2.zero, new Vector2(290, 22),
                "1000/1000", 13, Color.white, TextAnchor.MiddleCenter);

            // MP Bar
            _manaBar = CreateBar("ManaBar", frame.transform,
                new Vector2(10, -62), new Vector2(290, 18),
                new Color(0.15f, 0.3f, 0.85f), new Color(0.05f, 0.1f, 0.3f));

            _manaText = CreateText("ManaText", _manaBar.transform,
                Vector2.zero, new Vector2(290, 18),
                "500/500", 12, Color.white, TextAnchor.MiddleCenter);

            // Stamina Bar (thin)
            _staminaBar = CreateBar("StaminaBar", frame.transform,
                new Vector2(10, -84), new Vector2(290, 8),
                new Color(0.9f, 0.7f, 0.1f), new Color(0.3f, 0.25f, 0.05f));
        }

        // ===== Target Frame (top-center) =====
        private void CreateTargetFrame()
        {
            _targetFrame = CreatePanel("TargetFrame", _canvas.transform,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(-160, -10), new Vector2(320, 80));

            SetPanelColor(_targetFrame, new Color(0, 0, 0, 0.6f));
            _targetFrame.SetActive(false);

            _targetNameText = CreateText("TargetName", _targetFrame.transform,
                new Vector2(10, -5), new Vector2(200, 25),
                "Target", 16, Color.red, TextAnchor.MiddleLeft);

            _targetHealthBar = CreateBar("TargetHP", _targetFrame.transform,
                new Vector2(10, -35), new Vector2(290, 22),
                new Color(0.8f, 0.15f, 0.15f), new Color(0.3f, 0.05f, 0.05f));

            _targetHealthText = CreateText("TargetHPText", _targetHealthBar.transform,
                Vector2.zero, new Vector2(290, 22),
                "1000/1000", 13, Color.white, TextAnchor.MiddleCenter);
        }

        // ===== Skill Bar (bottom-center) =====
        private void CreateSkillBar()
        {
            var barPanel = CreatePanel("SkillBar", _canvas.transform,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-200, 10), new Vector2(400, 70));

            SetPanelColor(barPanel, new Color(0, 0, 0, 0.5f));

            _skillSlots = new SkillSlotUI[4];
            string[] keybinds = { "1", "2", "3", "4" };

            for (int i = 0; i < 4; i++)
            {
                var slotGO = CreatePanel($"SkillSlot_{i}", barPanel.transform,
                    new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                    new Vector2(15 + i * 95, -30), new Vector2(80, 60));

                SetPanelColor(slotGO, new Color(0.2f, 0.2f, 0.2f, 0.8f));

                // Icon
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(slotGO.transform, false);
                var iconRT = iconGO.AddComponent<RectTransform>();
                iconRT.anchoredPosition = Vector2.zero;
                iconRT.sizeDelta = new Vector2(50, 50);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.color = new Color(0.4f, 0.4f, 0.4f);
                iconImg.enabled = false;

                // Cooldown overlay
                var cdOverlay = new GameObject("CooldownOverlay");
                cdOverlay.transform.SetParent(slotGO.transform, false);
                var cdRT = cdOverlay.AddComponent<RectTransform>();
                cdRT.anchoredPosition = Vector2.zero;
                cdRT.sizeDelta = new Vector2(50, 50);
                var cdImg = cdOverlay.AddComponent<Image>();
                cdImg.color = new Color(0, 0, 0, 0.6f);
                cdImg.type = Image.Type.Filled;
                cdImg.fillMethod = Image.FillMethod.Radial360;
                cdImg.enabled = false;

                // Keybind text
                var kbText = CreateText("Keybind", slotGO.transform,
                    new Vector2(-25, 22), new Vector2(20, 16),
                    keybinds[i], 11, Color.yellow, TextAnchor.MiddleCenter);

                // Cooldown text
                var cdText = CreateText("CDText", slotGO.transform,
                    new Vector2(0, 0), new Vector2(50, 20),
                    "", 14, Color.white, TextAnchor.MiddleCenter);
                cdText.enabled = false;

                // Wire SkillSlotUI
                var slotUI = slotGO.AddComponent<SkillSlotUI>();
                // Use reflection or serialized fields — for prototype, use a helper init
                SetSkillSlotFields(slotUI, iconImg, cdImg, kbText, cdText);

                _skillSlots[i] = slotUI;
            }
        }

        // ===== Ship HUD (top-right) =====
        private void CreateShipHUD()
        {
            _shipHUD = CreatePanel("ShipHUD", _canvas.transform,
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-320, -10), new Vector2(310, 80));

            SetPanelColor(_shipHUD, new Color(0, 0, 0, 0.6f));
            _shipHUD.SetActive(false);

            CreateText("ShipLabel", _shipHUD.transform,
                new Vector2(10, -5), new Vector2(200, 25),
                "SHIP", 16, Color.cyan, TextAnchor.MiddleLeft);

            _shipHealthBar = CreateBar("ShipHP", _shipHUD.transform,
                new Vector2(10, -35), new Vector2(290, 18),
                new Color(0.2f, 0.8f, 0.2f), new Color(0.1f, 0.3f, 0.1f));

            _shipSpeedText = CreateText("ShipSpeed", _shipHUD.transform,
                new Vector2(10, -58), new Vector2(290, 20),
                "Speed: 0.0 knots", 14, Color.white, TextAnchor.MiddleLeft);
        }

        // ===== Cast Bar =====
        private void CreateCastBar()
        {
            _castBarObj = CreatePanel("CastBar", _canvas.transform,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-150, 90), new Vector2(300, 25));

            SetPanelColor(_castBarObj, new Color(0, 0, 0, 0.7f));
            _castBarObj.SetActive(false);

            _castBar = CreateBar("CastProgress", _castBarObj.transform,
                new Vector2(5, -3), new Vector2(290, 19),
                new Color(1f, 0.8f, 0.2f), new Color(0.2f, 0.15f, 0.05f));
        }

        // ===== Interaction Prompt =====
        private void CreateInteractionPrompt()
        {
            var promptGO = new GameObject("InteractionPrompt");
            promptGO.transform.SetParent(_canvas.transform, false);
            var rt = promptGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.3f);
            rt.anchorMax = new Vector2(0.5f, 0.3f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(300, 40);

            var bg = promptGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            _interactionPromptText = CreateText("PromptText", promptGO.transform,
                Vector2.zero, new Vector2(280, 35),
                "[F] Interact", 18, Color.yellow, TextAnchor.MiddleCenter);

            promptGO.SetActive(false);
        }

        // ===== State Display (debug) =====
        private void CreateStateDisplay()
        {
            _gameStateText = CreateText("GameState", _canvas.transform,
                new Vector2(10, -120), new Vector2(200, 25),
                "FreeRoam", 14, Color.gray, TextAnchor.MiddleLeft);

            var rt = _gameStateText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
        }

        // ===== Crosshair =====
        private void CreateCrosshair()
        {
            var crosshair = new GameObject("Crosshair");
            crosshair.transform.SetParent(_canvas.transform, false);
            var rt = crosshair.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(4, 4);
            rt.anchoredPosition = Vector2.zero;

            var img = crosshair.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.5f);
        }

        // ===== Wire to HUDManager =====
        private void WireHUDManager()
        {
            var hudGO = _canvas.gameObject;
            _hud = hudGO.AddComponent<HUDManager>();

            // Use reflection to set serialized fields
            var type = typeof(HUDManager);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            type.GetField("_healthBar", flags)?.SetValue(_hud, _healthBar);
            type.GetField("_manaBar", flags)?.SetValue(_hud, _manaBar);
            type.GetField("_staminaBar", flags)?.SetValue(_hud, _staminaBar);
            type.GetField("_healthText", flags)?.SetValue(_hud, _healthText);
            type.GetField("_manaText", flags)?.SetValue(_hud, _manaText);
            type.GetField("_targetFrame", flags)?.SetValue(_hud, _targetFrame);
            type.GetField("_targetNameText", flags)?.SetValue(_hud, _targetNameText);
            type.GetField("_targetHealthBar", flags)?.SetValue(_hud, _targetHealthBar);
            type.GetField("_targetHealthText", flags)?.SetValue(_hud, _targetHealthText);
            type.GetField("_skillSlots", flags)?.SetValue(_hud, _skillSlots);
            type.GetField("_shipHUD", flags)?.SetValue(_hud, _shipHUD);
            type.GetField("_shipHealthBar", flags)?.SetValue(_hud, _shipHealthBar);
            type.GetField("_shipSpeedText", flags)?.SetValue(_hud, _shipSpeedText);
            type.GetField("_gameStateText", flags)?.SetValue(_hud, _gameStateText);
            type.GetField("_interactionPromptText", flags)?.SetValue(_hud, _interactionPromptText);
            type.GetField("_castBarObject", flags)?.SetValue(_hud, _castBarObj);
            type.GetField("_castBar", flags)?.SetValue(_hud, _castBar);
        }

        // ===== Helper: SkillSlotUI field injection =====
        private void SetSkillSlotFields(SkillSlotUI slot, Image icon, Image cdOverlay, Text keybind, Text cdText)
        {
            var type = typeof(SkillSlotUI);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            type.GetField("_iconImage", flags)?.SetValue(slot, icon);
            type.GetField("_cooldownOverlay", flags)?.SetValue(slot, cdOverlay);
            type.GetField("_keybindText", flags)?.SetValue(slot, keybind);
            type.GetField("_cooldownText", flags)?.SetValue(slot, cdText);
        }

        // ===== UI Helpers =====

        private GameObject CreatePanel(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return go;
        }

        private void SetPanelColor(GameObject panel, Color color)
        {
            var img = panel.GetComponent<Image>();
            if (img == null) img = panel.AddComponent<Image>();
            img.color = color;
        }

        private Slider CreateBar(string name, Transform parent,
            Vector2 anchoredPos, Vector2 size,
            Color fillColor, Color bgColor)
        {
            var barGO = new GameObject(name);
            barGO.transform.SetParent(parent, false);
            var rt = barGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            // Background
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(barGO.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = bgColor;

            // Fill area
            var fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(barGO.transform, false);
            var fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.sizeDelta = Vector2.zero;

            // Fill
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillArea.transform, false);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = fillColor;

            var slider = barGO.AddComponent<Slider>();
            slider.fillRect = fillRT;
            slider.targetGraphic = fillImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
            slider.interactable = false;

            return slider;
        }

        private Text CreateText(string name, Transform parent,
            Vector2 anchoredPos, Vector2 size,
            string content, int fontSize, Color color, TextAnchor alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }
    }
}
