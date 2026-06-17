using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Approaches slowly, winds up, then dashes fast at the player. Takes over
    /// movement from the base Enemy chase.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class ChargerBehavior : MonoBehaviour
    {
        [SerializeField] private float approachSpeed = 1.5f;
        [SerializeField] private float dashSpeed = 12f;
        [SerializeField] private float chargeRange = 4f;
        [SerializeField] private float windup = 0.5f;
        [SerializeField] private float dashTime = 0.35f;
        [SerializeField] private float cooldown = 2f;

        private enum St { Approach, Windup, Dash, Cool }
        private St _st = St.Approach;
        private float _t;
        private Vector2 _dashDir;

        private Enemy _enemy;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _enemy.CedeMovement();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            var target = _enemy.Target;
            if (target == null) { _rb.linearVelocity = Vector2.zero; return; }

            Vector2 toP = (Vector2)target.position - _rb.position;
            float dist = toP.magnitude;
            Vector2 dirP = dist > 0.001f ? toP / dist : Vector2.zero;

            switch (_st)
            {
                case St.Approach:
                    _rb.linearVelocity = dirP * approachSpeed;
                    if (dist <= chargeRange) { _st = St.Windup; _t = windup; }
                    break;

                case St.Windup:
                    _rb.linearVelocity = Vector2.zero;     // telegraph
                    _t -= Time.fixedDeltaTime;
                    if (_t <= 0f) { _dashDir = dirP; _st = St.Dash; _t = dashTime; }
                    break;

                case St.Dash:
                    _rb.linearVelocity = _dashDir * dashSpeed;
                    _t -= Time.fixedDeltaTime;
                    if (_t <= 0f) { _st = St.Cool; _t = cooldown; }
                    break;

                case St.Cool:
                    _rb.linearVelocity = dirP * (approachSpeed * 0.5f);
                    _t -= Time.fixedDeltaTime;
                    if (_t <= 0f) _st = St.Approach;
                    break;
            }
        }
    }
}
