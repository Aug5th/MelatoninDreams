using System;
using UnityEngine;

namespace GameKit
{
    public enum GameState { Boot, MainMenu, Loading, Playing, Paused, GameOver }

    /// <summary>
    /// Central state machine. Every other system only listens to its events;
    /// nothing calls across systems directly.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event Action<GameState, GameState> StateChanged;
        public static event Action<int> ScoreChanged;
        public static event Action<int> LivesChanged;

        public GameState State { get; private set; } = GameState.Boot;
        public GameState PreviousState { get; private set; } = GameState.Boot;
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public bool IsNewRecord { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
            StateChanged = null;
            ScoreChanged = null;
            LivesChanged = null;
        }

        void Awake() { Instance = this; }

        void Update()
        {
            if (!InputCompat.PausePressed) return;

            // The topmost screen may handle Esc itself (e.g. Settings closes).
            var ui = UI.UIManager.Instance;
            var top = ui != null ? ui.Top : null;
            if (top != null && top.ConsumesPauseKey) { top.OnPauseKey(); return; }

            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        public void ChangeState(GameState next)
        {
            if (State == next) return;
            PreviousState = State;
            State = next;
            Time.timeScale = next == GameState.Paused ? 0f : 1f;
            StateChanged?.Invoke(PreviousState, next);
        }

        public void StartGame()
        {
            Score = 0;
            Lives = GameKitConfig.StartingLives;
            IsNewRecord = false;
            ScoreChanged?.Invoke(Score);
            LivesChanged?.Invoke(Lives);

            if (string.IsNullOrEmpty(GameKitConfig.GameSceneName))
            {
                ChangeState(GameState.Playing);
                return;
            }

            ChangeState(GameState.Loading);
            SceneLoader.Instance.Load(GameKitConfig.GameSceneName, () => ChangeState(GameState.Playing));
        }

        public void Pause()  { if (State == GameState.Playing) ChangeState(GameState.Paused); }
        public void Resume() { if (State == GameState.Paused)  ChangeState(GameState.Playing); }
        public void ToMainMenu() { ChangeState(GameState.MainMenu); }

        public void AddScore(int amount)
        {
            Score = Mathf.Max(0, Score + amount);
            ScoreChanged?.Invoke(Score);
        }

        public void LoseLife(int amount = 1)
        {
            Lives = Mathf.Max(0, Lives - amount);
            LivesChanged?.Invoke(Lives);
            if (Lives <= 0) EndGame();
        }

        public void EndGame()
        {
            if (State == GameState.GameOver) return;

            var save = SaveSystem.Save;
            IsNewRecord = Score > save.highScore;
            if (IsNewRecord) save.highScore = Score;
            save.totalRuns++;
            save.lastPlayedUtc = DateTime.UtcNow.ToString("o");
            SaveSystem.SaveGame();

            ChangeState(GameState.GameOver);
        }
    }
}
