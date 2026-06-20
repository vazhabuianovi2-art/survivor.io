using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Renders a live copy of the player's chibi (with its equipped weapon/skin)
    /// to a RenderTexture so the Equipment screen can show the actual hero. The
    /// copy lives far off-screen with its own camera; change the hero visual and
    /// the portrait updates automatically.
    /// </summary>
    public class HeroPortrait : MonoBehaviour
    {
        public static HeroPortrait Instance { get; private set; }
        public RenderTexture Texture { get; private set; }

        private Camera _cam;

        private void Awake() => Instance = this;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            var chibi = FindChild(player.transform, "main-chibi");
            if (chibi == null) return;

            Vector3 far = new Vector3(10000f, 10000f, 0f);

            var clone = Instantiate(chibi.gameObject, far, Quaternion.identity);
            clone.name = "PortraitChibi";
            clone.transform.localScale = chibi.lossyScale;
            foreach (var an in clone.GetComponentsInChildren<Animator>())
                an.updateMode = AnimatorUpdateMode.UnscaledTime;     // idle while menu is paused

            // Frame the clone by its rendered bounds.
            Bounds b = new Bounds(far, Vector3.one);
            bool has = false;
            foreach (var sr in clone.GetComponentsInChildren<SpriteRenderer>())
            {
                if (!has) { b = sr.bounds; has = true; }
                else b.Encapsulate(sr.bounds);
            }

            Texture = new RenderTexture(300, 400, 16) { name = "HeroPortraitRT" };

            var camGo = new GameObject("PortraitCamera");
            camGo.transform.position = new Vector3(b.center.x, b.center.y, far.z - 10f);
            _cam = camGo.AddComponent<Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = Mathf.Max(0.6f, b.extents.y * 1.25f);
            _cam.targetTexture = Texture;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.16f, 0.11f, 0.24f, 1f);
            _cam.cullingMask = ~0;
            _cam.depth = -10;
            _cam.enabled = false;            // only render while the Equipment screen is open
        }

        /// <summary>Enable rendering only while the portrait is visible (saves perf).</summary>
        public void SetRendering(bool on)
        {
            if (_cam != null) _cam.enabled = on;
        }

        private static Transform FindChild(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t) { var r = FindChild(c, name); if (r != null) return r; }
            return null;
        }
    }
}
