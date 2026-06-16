using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Companion drones that hover and slowly orbit the player, each auto-firing
    /// bolts at the nearest enemy. Levels add drones and damage.
    /// </summary>
    public class DroneWeapon : WeaponBase
    {
        private const float OrbitRadius = 1.1f;
        private const float OrbitSpeed  = 55f;     // deg/sec
        private const float Range = 8f;
        private const float Speed = 12f;
        private const float Life  = 1.6f;

        private readonly List<Transform> _drones = new List<Transform>();
        private float _angle;
        private float _fireTimer;

        public override string DisplayName => "Drones";
        public override string DescribeNext()
        {
            int next = Level + 1;
            return (next == 2 || next == 4) ? "+1 drone" : "more drone damage";
        }

        private int DroneCount => 1 + (Level >= 2 ? 1 : 0) + (Level >= 4 ? 1 : 0);  // 1,2,2,3,3
        private float Damage   => (5f + (Level - 1) * 3f) * PlayerStats.Damage;
        private float Interval => Mathf.Max(0.45f, 1.1f * Mathf.Pow(0.92f, Level - 1) * PlayerStats.Cooldown);

        protected override void OnEquip()   => Rebuild();
        protected override void OnLevelUp() => Rebuild();

        private void OnDestroy()
        {
            foreach (var d in _drones) if (d != null) Destroy(d.gameObject);
        }

        private void Rebuild()
        {
            foreach (var d in _drones) if (d != null) Destroy(d.gameObject);
            _drones.Clear();
            for (int i = 0; i < DroneCount; i++)
            {
                var go = new GameObject($"Drone{i}");
                go.transform.SetParent(Owner, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = DroneSprite();
                sr.sortingOrder = 18;
                _drones.Add(go.transform);
            }
        }

        private void Update()
        {
            if (!GameActive) return;

            _angle += OrbitSpeed * Time.deltaTime;
            int count = _drones.Count;
            for (int i = 0; i < count; i++)
            {
                if (_drones[i] == null) continue;
                float a = (_angle + i * (360f / count)) * Mathf.Deg2Rad;
                _drones[i].localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * OrbitRadius;
            }

            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            var target = FindNearestEnemy(Range);
            if (target == null) return;
            _fireTimer = Interval;
            FireAll(target);
        }

        private void FireAll(Transform target)
        {
            var prefab = WeaponManager.Instance != null ? WeaponManager.Instance.ProjectilePrefab : null;
            if (prefab == null) return;

            foreach (var drone in _drones)
            {
                if (drone == null) continue;
                Vector2 dir = ((Vector2)target.position - (Vector2)drone.position).normalized;
                var go = Instantiate(prefab, drone.position, Quaternion.identity);
                var p = go.GetComponent<Projectile>();
                if (p != null) p.Launch(dir, Speed, Damage, Life, 0);
            }
        }

        private static Sprite _droneSprite;
        private static Sprite DroneSprite()
        {
            if (_droneSprite != null) return _droneSprite;

            int size = 20;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float half = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half + 0.5f) / half;
                    float dy = (y - half + 0.5f) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float core = Mathf.Clamp01(1f - d / 0.5f);
                    float a = d <= 1f ? Mathf.Max(core, 0.85f) : 0f;
                    var col = Color.Lerp(new Color(0.3f, 0.85f, 1f), Color.white, core);
                    col.a = a;
                    tex.SetPixel(x, y, col);
                }
            tex.Apply();
            _droneSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _droneSprite;
        }
    }
}
