using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Evolved Drones: a swarm of fast-firing companion drones.</summary>
    public class DroneSwarmWeapon : WeaponBase
    {
        public override int MaxLevel => 1;
        public override bool IsEvolved => true;
        public override string DisplayName => "Drone Swarm";
        public override string DescribeNext() => "Evolved";

        private const int Count = 5;
        private const float OrbitRadius = 1.3f, OrbitSpeed = 80f;
        private const float Range = 9f, Speed = 14f, Life = 1.7f, Interval = 0.45f;

        private readonly List<Transform> _drones = new List<Transform>();
        private float _angle, _fireTimer;

        protected override void OnEquip()
        {
            for (int i = 0; i < Count; i++)
            {
                var go = new GameObject($"SwarmDrone{i}");
                go.transform.SetParent(Owner, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = FxSprites.Tinted(new Color(0.3f, 0.85f, 1f));
                sr.sortingOrder = 18;
                go.transform.localScale = Vector3.one * 0.7f;
                _drones.Add(go.transform);
            }
        }

        private void Update()
        {
            if (!GameActive) return;
            _angle += OrbitSpeed * Time.deltaTime;
            for (int i = 0; i < _drones.Count; i++)
            {
                float a = (_angle + i * (360f / _drones.Count)) * Mathf.Deg2Rad;
                _drones[i].localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * OrbitRadius;
            }

            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;
            var target = FindNearestEnemy(Range);
            if (target == null) return;
            _fireTimer = Interval;

            var prefab = WeaponManager.Instance != null ? WeaponManager.Instance.ProjectilePrefab : null;
            if (prefab == null) return;

            float dmg = 14f * PlayerStats.Damage;
            foreach (var d in _drones)
            {
                Vector2 dir = ((Vector2)target.position - (Vector2)d.position).normalized;
                var go = Object.Instantiate(prefab, d.position, Quaternion.identity);
                var p = go.GetComponent<Projectile>();
                if (p != null) p.Launch(dir, Speed, dmg, Life, 0);
            }
        }
    }
}
