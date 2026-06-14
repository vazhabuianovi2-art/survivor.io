using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivorIO
{
    /// <summary>
    /// Owns run-level state: survival time and the game-over flow. Resets the
    /// run-scoped enemy counters on start and restores the timescale (in case a
    /// previous run left it paused).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public float ElapsedTime { get; private set; }
        public bool IsGameOver { get; private set; }

        /// <summary>Raised once when the player dies.</summary>
        public event Action GameOver;

        private void Awake()
        {
            Instance = this;
            Enemy.ResetCounters();
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (!IsGameOver)
                ElapsedTime += Time.deltaTime;
        }

        public void TriggerGameOver()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            Time.timeScale = 0f;
            GameOver?.Invoke();
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
