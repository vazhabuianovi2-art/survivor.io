using UnityEngine;
using UnityEngine.EventSystems;

namespace SurvivorIO
{
    /// <summary>
    /// A simple on-screen virtual joystick. Drag the handle around the background
    /// to produce a normalized direction in <see cref="Direction"/>.
    /// Works with touch and mouse via the EventSystem (input-handler agnostic).
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [Tooltip("How far (in background radii) the handle can travel from the centre.")]
        [SerializeField, Range(0.1f, 2f)] private float handleRange = 1f;
        [Tooltip("Inputs below this magnitude are treated as zero.")]
        [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.1f;

        /// <summary>Current joystick direction, range [-1, 1] on each axis.</summary>
        public Vector2 Direction { get; private set; }

        private Canvas _canvas;
        private Camera _uiCamera;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
                _uiCamera = _canvas.worldCamera;

            if (background == null) background = transform as RectTransform;
            if (handle == null && background.childCount > 0)
                handle = background.GetChild(0) as RectTransform;
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null) return;

            Vector2 radius = background.sizeDelta * 0.5f;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, eventData.position, _uiCamera, out Vector2 local))
                return;

            // Normalize the local point against the background radius.
            Vector2 normalized = new Vector2(local.x / radius.x, local.y / radius.y);
            Direction = normalized.magnitude > 1f ? normalized.normalized : normalized;
            if (Direction.magnitude < deadZone) Direction = Vector2.zero;

            if (handle != null)
                handle.anchoredPosition = Direction * radius * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }
    }
}
