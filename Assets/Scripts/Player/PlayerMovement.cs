using UnityEngine;
using UnityEngine.InputSystem;

namespace SurvivorIO
{
    /// <summary>
    /// Top-down player movement. Combines on-screen joystick input (touch) with
    /// keyboard WASD / arrow keys (new Input System). Moves via Rigidbody2D so it
    /// collides cleanly with the world and enemies.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private VirtualJoystick joystick;
        [Tooltip("Sprite to flip horizontally based on travel direction. Optional.")]
        [SerializeField] private SpriteRenderer spriteToFlip;
        [Tooltip("Rigged-character visual root flipped via localScale.x (preferred over spriteToFlip).")]
        [SerializeField] private Transform visualToFlip;

        private Rigidbody2D _rb;
        private Vector2 _input;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        private void Update()
        {
            Vector2 dir = Vector2.zero;

            // Touch / mouse joystick takes priority when active.
            if (joystick != null)
                dir = joystick.Direction;

            // Fall back to keyboard when the joystick is idle.
            if (dir.sqrMagnitude < 0.01f)
                dir = ReadKeyboard();

            _input = Vector2.ClampMagnitude(dir, 1f);

            if (Mathf.Abs(_input.x) > 0.01f)
            {
                if (visualToFlip != null)
                {
                    var s = visualToFlip.localScale;
                    s.x = Mathf.Abs(s.x) * (_input.x < 0f ? -1f : 1f);
                    visualToFlip.localScale = s;
                }
                else if (spriteToFlip != null)
                {
                    spriteToFlip.flipX = _input.x < 0f;
                }
            }
        }

        private static Vector2 ReadKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return Vector2.zero;

            Vector2 dir = Vector2.zero;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) dir.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) dir.y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) dir.y += 1f;
            return dir.normalized;
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _input * moveSpeed;
        }

        /// <summary>Used by level-up skills to boost movement speed.</summary>
        public void UpgradeMoveSpeed(float multiplier) => moveSpeed *= multiplier;
    }
}
