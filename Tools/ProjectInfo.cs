using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
using System.Text;

public static class ProjectInfo
{
    public static string Execute()
    {
        var sb = new StringBuilder();

        var rp = GraphicsSettings.currentRenderPipeline;
        sb.Append("RenderPipeline=").Append(rp != null ? rp.GetType().Name : "Built-in (no SRP)").Append('\n');

        bool hasInputSystem = System.Type.GetType(
            "UnityEngine.InputSystem.Keyboard, Unity.InputSystem") != null;
        sb.Append("InputSystem package present=").Append(hasInputSystem).Append('\n');

        var scene = EditorSceneManager.GetActiveScene();
        sb.Append("activeScene=").Append(scene.name).Append("  path=").Append(scene.path).Append('\n');
        sb.Append("root objects: ");
        foreach (var go in scene.GetRootGameObjects())
            sb.Append(go.name).Append(", ");
        sb.Append('\n');

        bool urpLit = Shader.Find("Universal Render Pipeline/Lit") != null;
        sb.Append("URP/Lit shader available=").Append(urpLit).Append('\n');
        return sb.ToString();
    }
}
