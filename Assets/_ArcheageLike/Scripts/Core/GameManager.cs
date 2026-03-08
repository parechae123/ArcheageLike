using UnityEngine;
using UnityEngine.Events;

namespace ArcheageLike.Core
{
    public enum GameState
    {
        FreeRoam,
        Combat,
        Sailing,
        Housing,
        UI,
        Dialogue
    }

    /// <summary>
    /// Central game manager that controls game state and coordinates systems.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("Game State")]
        [SerializeField] private GameState _currentState = GameState.FreeRoam;

        public GameState CurrentState => _currentState;

        // Events
        public UnityEvent<GameState, GameState> OnGameStateChanged = new UnityEvent<GameState, GameState>();
        public UnityEvent OnGamePaused = new UnityEvent();
        public UnityEvent OnGameResumed = new UnityEvent();

        private bool _isPaused;
        public bool IsPaused => _isPaused;

        public void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;

            var previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameManager] State changed: {previousState} -> {newState}");
            OnGameStateChanged?.Invoke(previousState, newState);
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;

            if (_isPaused)
                OnGamePaused?.Invoke();
            else
                OnGameResumed?.Invoke();
        }

        public void SetPause(bool paused)
        {
            if (_isPaused == paused) return;
            TogglePause();
        }
    }
}
