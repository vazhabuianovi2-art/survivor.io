using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivorIO
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>Enemy HP/damage multiplier from the selected stage (set by StageManager).</summary>
        public static float StageEnemyMult = 1f;

        public float ElapsedTime { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool IsGameWon { get; private set; }

        /// <summary>Gold collected during this run (persisted to meta in Phase 4).</summary>
        public int Gold { get; private set; }

        public event Action GameOver;
        public event Action GameWon;
        public event Action GoldChanged;

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            Gold += amount;
            GoldChanged?.Invoke();
        }

        private void Awake()
        {
            Instance = this;
            Enemy.ResetCounters();
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (!IsGameOver && !IsGameWon)
                ElapsedTime += Time.deltaTime;

#if UNITY_EDITOR
            // Debug shortcuts
            if (UnityEngine.InputSystem.Keyboard.current?.f1Key.wasPressedThisFrame == true)
                TriggerWin();
#endif
        }

        public void TriggerGameOver()
        {
            if (IsGameOver || IsGameWon) return;
            IsGameOver = true;
            MetaProgress.BankRunGold(Gold);
            Time.timeScale = 0f;
            GameOver?.Invoke();
        }

        public void TriggerWin()
        {
            if (IsGameOver || IsGameWon) return;
            IsGameWon = true;
            MetaProgress.BankRunGold(Gold);
            Time.timeScale = 0f;
            GameWon?.Invoke();
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
