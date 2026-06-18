using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Applies the selected stage at scene load: sets the global enemy difficulty
    /// multiplier and tints the background. Runs before enemies spawn.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class StageManager : MonoBehaviour
    {
        private void Awake()
        {
            var stage = MetaProgress.Stages[MetaProgress.SelectedStage];
            GameManager.StageEnemyMult = stage.Difficulty;

            var bg = GameObject.Find("InfiniteBackground");
            if (bg != null)
            {
                var sr = bg.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = stage.Tint;
            }
        }
    }
}
