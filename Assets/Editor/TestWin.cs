using UnityEngine;
using UnityEditor;

public class TestWin
{
    public static string Execute()
    {
        // Trigger WIN directly to test the WIN screen
        if (SurvivorIO.GameManager.Instance != null)
        {
            SurvivorIO.GameManager.Instance.TriggerWin();
            return "WIN triggered!";
        }
        return "GameManager not found";
    }
}
