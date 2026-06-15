using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class CreateChibiWalk
{
    public static string Execute()
    {
        // ── 1. Build animation clip ──────────────────────────────────────
        var clip = new AnimationClip();
        clip.name = "ChibiWalk";
        clip.frameRate = 24f;
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        float dur = 0.5f;

        // Helper: sine curve at given rate, amplitude, offset, N samples
        System.Func<float, float, float, float, int, AnimationCurve> Sine =
            (baseVal, amp, phase, rate, samples) =>
            {
                var keys = new Keyframe[samples + 1];
                for (int i = 0; i <= samples; i++)
                {
                    float t = dur * i / samples;
                    float v = baseVal + amp * Mathf.Sin(2 * Mathf.PI * rate * t / dur + phase);
                    keys[i] = new Keyframe(t, v);
                }
                var c = new AnimationCurve(keys);
                for (int i = 0; i < c.keys.Length; i++)
                    AnimationUtility.SetKeyLeftTangentMode(c, i, AnimationUtility.TangentMode.Auto);
                return c;
            };

        // bone_1: body sway (1 cycle) and bob (2 cycles)
        clip.SetCurve("bone_1", typeof(Transform), "localEulerAnglesRaw.z",
            Sine(88.1f, 2.5f, 0f, 1f, 12));
        clip.SetCurve("bone_1", typeof(Transform), "m_LocalPosition.y",
            Sine(1.233f, 0.04f, 0f, 2f, 24));

        // bone_5 (left leg): ±14° at 1 cycle
        clip.SetCurve("bone_1/bone_5", typeof(Transform), "localEulerAnglesRaw.z",
            Sine(214.4f, 14f, 0f, 1f, 12));

        // bone_7 (right leg): ±14° opposite phase
        clip.SetCurve("bone_1/bone_7", typeof(Transform), "localEulerAnglesRaw.z",
            Sine(158.1f, 14f, Mathf.PI, 1f, 12));

        // bone_6 (left lower leg): slight follow
        clip.SetCurve("bone_1/bone_5/bone_6", typeof(Transform), "localEulerAnglesRaw.z",
            Sine(3.6f, 5f, 0f, 1f, 12));

        // bone_8 (right lower leg): opposite
        clip.SetCurve("bone_1/bone_7/bone_8", typeof(Transform), "localEulerAnglesRaw.z",
            Sine(2.3f, 5f, Mathf.PI, 1f, 12));

        // ── 2. Save clip ─────────────────────────────────────────────────
        const string clipPath = "Assets/Animations/ChibiWalk.anim";
        AssetDatabase.CreateAsset(clip, clipPath);

        // ── 3. Build Animator controller ────────────────────────────────
        const string ctrlPath = "Assets/Animations/ChibiAnimator.controller";
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        var root = ctrl.layers[0].stateMachine;

        var walkState = root.AddState("Walk");
        walkState.motion = clip;
        root.defaultState = walkState;

        // ── 4. Add Animator to main-chibi ────────────────────────────────
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";
        var chibi = player.transform.Find("main-chibi");
        if (chibi == null) return "main-chibi not found";

        var anim = chibi.GetComponent<Animator>();
        if (anim == null) anim = chibi.gameObject.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        anim.updateMode = AnimatorUpdateMode.Normal;

        // ── 5. Save scene ────────────────────────────────────────────────
        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        return $"Created ChibiWalk.anim + ChibiAnimator.controller, Animator added to main-chibi";
    }
}
