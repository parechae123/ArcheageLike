using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcheageLike.Core
{
    /// <summary>
    /// Centralized input manager using Unity's New Input System.
    /// Directly polls Keyboard/Mouse every frame — no PlayerInput component needed.
    /// </summary>
    public class GameInputManager : Singleton<GameInputManager>
    {
        // Movement
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsRunning { get; private set; }
        public bool JumpPressed { get; private set; }

        // Combat
        public bool AttackPressed { get; private set; }
        public bool Skill1Pressed { get; private set; }
        public bool Skill2Pressed { get; private set; }
        public bool Skill3Pressed { get; private set; }
        public bool Skill4Pressed { get; private set; }
        public bool TabTargetPressed { get; private set; }

        // Interaction
        public bool InteractPressed { get; private set; }
        public bool InventoryToggled { get; private set; }
        public bool EscapePressed { get; private set; }

        // Housing
        public bool RotateBuildingPressed { get; private set; }
        public bool PlaceBuildingPressed { get; private set; }
        public bool CancelBuildingPressed { get; private set; }

        // Mouse
        public bool RightMouseHeld { get; private set; }
        public float ScrollValue { get; private set; }

        private void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb == null || mouse == null) return;

            // ===== Movement =====
            Vector2 move = Vector2.zero;
            if (kb.wKey.isPressed) move.y += 1f;
            if (kb.sKey.isPressed) move.y -= 1f;
            if (kb.aKey.isPressed) move.x -= 1f;
            if (kb.dKey.isPressed) move.x += 1f;
            MoveInput = move.normalized;

            // Camera look (mouse delta)
            LookInput = mouse.delta.ReadValue();

            IsRunning = kb.leftShiftKey.isPressed;
            JumpPressed = kb.spaceKey.wasPressedThisFrame;

            // ===== Combat =====
            AttackPressed = mouse.leftButton.wasPressedThisFrame;
            Skill1Pressed = kb.digit1Key.wasPressedThisFrame;
            Skill2Pressed = kb.digit2Key.wasPressedThisFrame;
            Skill3Pressed = kb.digit3Key.wasPressedThisFrame;
            Skill4Pressed = kb.digit4Key.wasPressedThisFrame;
            TabTargetPressed = kb.tabKey.wasPressedThisFrame;

            // ===== Interaction =====
            InteractPressed = kb.fKey.wasPressedThisFrame;
            InventoryToggled = kb.iKey.wasPressedThisFrame;
            EscapePressed = kb.escapeKey.wasPressedThisFrame;

            // ===== Housing =====
            RotateBuildingPressed = kb.rKey.wasPressedThisFrame;
            PlaceBuildingPressed = mouse.leftButton.wasPressedThisFrame;
            CancelBuildingPressed = kb.escapeKey.wasPressedThisFrame;

            // ===== Mouse =====
            RightMouseHeld = mouse.rightButton.isPressed;
            ScrollValue = mouse.scroll.ReadValue().y;
        }
    }
}
