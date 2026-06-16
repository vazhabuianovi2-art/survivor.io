using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Shared runtime-generated sprites for weapon visuals (cached).</summary>
    public static class FxSprites
    {
        private static Sprite _orb;
        private static Sprite _softDisc;

        /// <summary>Glowing orb with a white core (purple→white).</summary>
        public static Sprite Orb()
        {
            if (_orb != null) return _orb;
            _orb = BuildDisc(new Color(0.6f, 0.4f, 1f), 0.45f, true);
            return _orb;
        }

        /// <summary>Soft translucent disc for auras.</summary>
        public static Sprite SoftDisc()
        {
            if (_softDisc != null) return _softDisc;
            _softDisc = BuildDisc(Color.white, 1f, false);
            return _softDisc;
        }

        public static Sprite Tinted(Color core)
        {
            // not cached — small one-offs
            return BuildDisc(core, 0.45f, true);
        }

        private static Sprite BuildDisc(Color color, float coreFrac, bool hardCore)
        {
            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float half = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half + 0.5f) / half;
                    float dy = (y - half + 0.5f) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float core = Mathf.Clamp01(1f - d / coreFrac);
                    float glow = Mathf.Pow(Mathf.Clamp01(1f - d), 0.7f);
                    float a = hardCore ? Mathf.Max(core, glow * 0.85f) : glow * 0.6f;
                    var col = Color.Lerp(color, Color.white, hardCore ? core : 0f);
                    col.a = a;
                    tex.SetPixel(x, y, col);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
